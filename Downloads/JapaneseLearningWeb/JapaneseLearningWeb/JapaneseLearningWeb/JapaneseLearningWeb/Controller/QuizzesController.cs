using System.Security.Claims;
using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using JapaneseLearningWeb.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JapaneseLearningWeb.Models.DTO;
using System.Security.Claims;

namespace JapaneseLearningWeb.Controllers
{
    [Authorize] // Bắt buộc user phải đăng nhập để truy cập các action trong Controller này
    public class QuizzesController : Controller
    {
        private readonly AppDbContext _context;

        public QuizzesController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== INDEX ====================
        // Cho phép tất cả user (anonymous hoặc đã login) xem danh sách quiz
        // Nếu bạn muốn bắt buộc login để xem, giữ nguyên [Authorize] trên Controller.
        // Nếu muốn bất kỳ ai (anonymous) cũng xem được, gỡ bỏ [Authorize] trên Controller
        public async Task<IActionResult> Index()
        {
            var quizzes = await _context.Quizzes
                                        .Include(q => q.Lesson)
                                        .ThenInclude(l => l.Course)
                                        .OrderByDescending(q => q.CreatedAt)
                                        .ToListAsync();
            return View(quizzes);
        }

        // ==================== CREATE ====================
        // GET: Quizzes/Create → Chỉ dành cho Admin & Employee
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách Courses để bind vào dropdown
            var courses = await _context.Courses.ToListAsync();
            if (!courses.Any())
            {
                TempData["Error"] = "Hiện tại chưa có khóa học nào. Vui lòng tạo khóa học trước.";
                ViewBag.Courses = new List<Course>();
                ViewBag.Lessons = new List<Lesson>();
                return View(new Quiz());
            }

            ViewBag.Courses = courses;

            // Chọn mặc định Course đầu tiên, load Lesson tương ứng
            int defaultCourseId = courses.First().CourseId;
            var lessons = await _context.Lessons
                                        .Where(l => l.CourseId == defaultCourseId)
                                        .ToListAsync();
            ViewBag.Lessons = lessons;

            // Khởi tạo Quiz mới với 1 Question + 4 Option rỗng
            var model = new Quiz
            {
                CourseId = defaultCourseId,
                LessonId = lessons.Any() ? lessons.First().LessonId : 0,
                Title = $"Quiz {DateTime.Now:yyyyMMddHHmmss}",
                Questions = new List<Question>
                {
                    new Question
                    {
                        Text = string.Empty,
                        Options = new List<Option>
                        {
                            new Option { Text = string.Empty },
                            new Option { Text = string.Empty },
                            new Option { Text = string.Empty },
                            new Option { Text = string.Empty },
                        }
                    }
                }
            };
            return View(model);
        }

        // POST: Quizzes/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Quiz quiz)
        {
            // 1. Validation cơ bản
            if (quiz.CourseId <= 0)
                ModelState.AddModelError("CourseId", "Vui lòng chọn khóa học.");

            if (quiz.LessonId <= 0)
                ModelState.AddModelError("LessonId", "Vui lòng chọn bài học.");

            if (string.IsNullOrWhiteSpace(quiz.Title))
                ModelState.AddModelError("Title", "Tiêu đề không được để trống.");

            if (quiz.Questions == null || !quiz.Questions.Any())
            {
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một câu hỏi.");
            }
            else
            {
                for (int i = 0; i < quiz.Questions.Count; i++)
                {
                    var question = quiz.Questions[i];
                    if (string.IsNullOrWhiteSpace(question.Text))
                    {
                        ModelState.AddModelError($"Questions[{i}].Text", $"Câu hỏi thứ {i + 1} không được để trống.");
                    }

                    if (question.Options == null || question.Options.Count != 4)
                    {
                        ModelState.AddModelError($"Questions[{i}].Options", $"Câu hỏi thứ {i + 1} phải có đúng 4 đáp án.");
                    }
                    else
                    {
                        // Kiểm tra từng option
                        bool hasCorrect = false;
                        for (int j = 0; j < question.Options.Count; j++)
                        {
                            var opt = question.Options[j];
                            if (string.IsNullOrWhiteSpace(opt.Text))
                            {
                                ModelState.AddModelError(
                                    $"Questions[{i}].Options[{j}].Text",
                                    $"Đáp án {(char)('A' + j)} của câu hỏi thứ {i + 1} không được để trống."
                                );
                            }
                            if (opt.IsCorrect)
                                hasCorrect = true;
                        }

                        if (!hasCorrect)
                        {
                            ModelState.AddModelError($"Questions[{i}].Options", $"Câu hỏi thứ {i + 1} phải có ít nhất 1 đáp án đúng.");
                        }
                    }
                }
            }

            // 2. Nếu có lỗi, repopulate dropdown & trả về View
            if (!ModelState.IsValid)
            {
                ViewBag.Courses = await _context.Courses.ToListAsync();
                ViewBag.Lessons = quiz.CourseId > 0
                    ? await _context.Lessons.Where(l => l.CourseId == quiz.CourseId).ToListAsync()
                    : new List<Lesson>();
                return View(quiz);
            }

            // 3. Nếu valid, gán quan hệ & lưu xuống DB
            try
            {
                if (string.IsNullOrEmpty(quiz.Title))
                    quiz.Title = $"Quiz {DateTime.Now:yyyyMMddHHmmss}";

                foreach (var q in quiz.Questions)
                {
                    q.Quiz = quiz;
                    foreach (var opt in q.Options)
                    {
                        opt.Question = q;
                    }
                }

                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi lưu dữ liệu. Vui lòng thử lại.";
                Console.WriteLine($"[Create Quiz] Exception: {ex.Message}");
                ViewBag.Courses = await _context.Courses.ToListAsync();
                ViewBag.Lessons = quiz.CourseId > 0
                    ? await _context.Lessons.Where(l => l.CourseId == quiz.CourseId).ToListAsync()
                    : new List<Lesson>();
                return View(quiz);
            }
        }

        // ==================== EDIT ====================
        // GET: Quizzes/Edit/5
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id)
        {
            var quiz = await _context.Quizzes
                                     .Include(q => q.Questions)
                                     .ThenInclude(q => q.Options)
                                     .FirstOrDefaultAsync(q => q.QuizId == id);
            if (quiz == null) return NotFound();

            // Chuẩn hóa CorrectAnswerIndex từ Option.IsCorrect
            foreach (var question in quiz.Questions)
            {
                var correctOpt = question.Options.FirstOrDefault(o => o.IsCorrect);
                question.CorrectAnswerIndex = correctOpt != null
                    ? question.Options.IndexOf(correctOpt)
                    : 0;
            }

            ViewBag.Courses = await _context.Courses.ToListAsync();
            ViewBag.Lessons = await _context.Lessons
                                    .Where(l => l.CourseId == quiz.CourseId)
                                    .ToListAsync();
            return View(quiz);
        }

        // POST: Quizzes/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Quiz quiz)
        {
            if (id != quiz.QuizId) return NotFound();

            // Validation tương tự Create
            if (quiz.CourseId <= 0)
                ModelState.AddModelError("CourseId", "Vui lòng chọn khóa học.");
            if (quiz.LessonId <= 0)
                ModelState.AddModelError("LessonId", "Vui lòng chọn bài học.");
            if (string.IsNullOrWhiteSpace(quiz.Title))
                ModelState.AddModelError("Title", "Tiêu đề không được để trống.");

            if (quiz.Questions == null || !quiz.Questions.Any())
            {
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một câu hỏi.");
            }
            else
            {
                for (int i = 0; i < quiz.Questions.Count; i++)
                {
                    var question = quiz.Questions[i];
                    if (string.IsNullOrWhiteSpace(question.Text))
                    {
                        ModelState.AddModelError($"Questions[{i}].Text", $"Câu hỏi thứ {i + 1} không được để trống.");
                    }

                    if (question.Options == null || question.Options.Count != 4)
                    {
                        ModelState.AddModelError($"Questions[{i}].Options", $"Câu hỏi thứ {i + 1} phải có đúng 4 đáp án.");
                    }
                    else
                    {
                        bool hasCorrect = false;
                        for (int j = 0; j < question.Options.Count; j++)
                        {
                            var opt = question.Options[j];
                            if (string.IsNullOrWhiteSpace(opt.Text))
                            {
                                ModelState.AddModelError(
                                    $"Questions[{i}].Options[{j}].Text",
                                    $"Đáp án {(char)('A' + j)} của câu hỏi thứ {i + 1} không được để trống."
                                );
                            }
                            if (opt.IsCorrect) hasCorrect = true;
                        }
                        if (!hasCorrect)
                        {
                            ModelState.AddModelError($"Questions[{i}].Options", $"Câu hỏi thứ {i + 1} phải có ít nhất 1 đáp án đúng.");
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Courses = await _context.Courses.ToListAsync();
                ViewBag.Lessons = quiz.CourseId > 0
                    ? await _context.Lessons.Where(l => l.CourseId == quiz.CourseId).ToListAsync()
                    : new List<Lesson>();
                return View(quiz);
            }

            try
            {
                // Trước hết xóa hết các Question + Option cũ của Quiz này (để làm sạch)
                var existingQuestions = _context.Questions
                                                .Where(q => q.QuizId == id)
                                                .Include(q => q.Options)
                                                .ToList();
                _context.Options.RemoveRange(existingQuestions.SelectMany(q => q.Options));
                _context.Questions.RemoveRange(existingQuestions);
                await _context.SaveChangesAsync();

                // Bây giờ thêm lại Questions + Options mới
                foreach (var q in quiz.Questions)
                {
                    q.QuizId = quiz.QuizId; // gán khóa ngoại
                    foreach (var opt in q.Options)
                    {
                        opt.Question = q; // gán navigation tạm
                    }
                }

                // Cập nhật lại các trường của Quiz (như Title, CourseId, LessonId, CreatedAt giữ nguyên)
                _context.Quizzes.Update(quiz);
                _context.Questions.AddRange(quiz.Questions);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Quizzes.Any(q => q.QuizId == id))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật quiz. Vui lòng thử lại.";
                Console.WriteLine($"[Edit Quiz] Exception: {ex.Message}");
                ViewBag.Courses = await _context.Courses.ToListAsync();
                ViewBag.Lessons = quiz.CourseId > 0
                    ? await _context.Lessons.Where(l => l.CourseId == quiz.CourseId).ToListAsync()
                    : new List<Lesson>();
                return View(quiz);
            }
        }

        // ==================== DELETE ====================
        // GET: Quizzes/Delete/5
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();

            // Xóa luôn (không có view confirm riêng)
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==================== TAKE (LÀM BÀI KIỂM TRA) ====================
        // GET: Quizzes/Take/5
        // Phải login để làm bài; nếu chỉ Student làm được bạn có thể để [Authorize(Roles="Student")]
        [Authorize]
        public async Task<IActionResult> Take(int id)
        {
            var quiz = await _context.Quizzes
                                     .Include(q => q.Lesson)
                                     .ThenInclude(l => l.Course)
                                     .Include(q => q.Questions)
                                     .ThenInclude(q => q.Options)
                                     .FirstOrDefaultAsync(q => q.QuizId == id);
            if (quiz == null) return NotFound();

            return View(quiz);
        }

        // ==================== SUBMIT (NỘP BÀI) ====================
        // POST: Quizzes/Submit
        [HttpPost]
        [Authorize] // Chỉ user đã đăng nhập mới nộp bài
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int QuizId, int[] Answers)
        {
            // Lấy quiz đầy đủ (có câu hỏi + đáp án)
            var quiz = await _context.Quizzes
                                     .Include(q => q.Questions)
                                     .ThenInclude(q => q.Options)
                                     .FirstOrDefaultAsync(q => q.QuizId == QuizId);
            if (quiz == null) return NotFound();

            // Kiểm tra số câu trả về có khớp không
            if (Answers == null || Answers.Length != quiz.Questions.Count)
            {
                TempData["Error"] = "Bạn chưa hoàn thành đầy đủ đáp án.";
                return RedirectToAction("Take", new { id = QuizId });
            }

            int totalQ = quiz.Questions.Count;
            int correctCount = 0;

            for (int i = 0; i < totalQ; i++)
            {
                var question = quiz.Questions[i];
                int chosenIndex = Answers[i]; // 0..3
                if (chosenIndex >= 0 && chosenIndex < question.Options.Count)
                {
                    if (question.Options[chosenIndex].IsCorrect)
                        correctCount++;
                }
            }

            ViewBag.Score = correctCount;
            ViewBag.Total = totalQ;
            ViewBag.QuizTitle = quiz.Title;
            return View("Result");
        }

        // ==================== API GetLessonsByCourse ====================
        [HttpGet]
        public async Task<JsonResult> GetLessonsByCourse(int courseId)
        {
            if (courseId <= 0)
                return Json(new List<object>());

            var lessons = await _context.Lessons
                                        .Where(l => l.CourseId == courseId)
                                        .Select(l => new { l.LessonId, l.Title })
                                        .ToListAsync();
            return Json(lessons);
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.LessonId == dto.LessonId);

            if (quiz == null) return NotFound();

            int correct = 0;
            var results = new List<object>();

            foreach (var q in quiz.Questions)
            {
                var correctOpt = q.Options.First(o => o.IsCorrect);
                var userAnswer = dto.Answers.FirstOrDefault(a => a.QuestionId == q.QuestionId);

                bool isCorrect = userAnswer != null && userAnswer.ChoiceId == correctOpt.OptionId;
                if (isCorrect) correct++;

                results.Add(new
                {
                    questionId = q.QuestionId,
                    correctChoiceId = correctOpt.OptionId,
                    userChoiceId = userAnswer?.ChoiceId,
                    isCorrect
                });
            }

            int total = quiz.Questions.Count;
            int score = (int)Math.Round(correct * 100.0 / total);

            // ==========================
            // 1️⃣ LƯU QUIZ RESULT
            // ==========================
            _context.QuizResults.Add(new QuizResult
            {
                LessonId = dto.LessonId,
                Score = score,
                UserId = userId,
                DateTaken = DateTime.UtcNow
            });

            // ==========================
            // 2️⃣ UPDATE USER PROGRESS (QUAN TRỌNG)
            // ==========================
            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == dto.LessonId);

            if (progress == null)
            {
                progress = new UserProgress
                {
                    UserId = userId,
                    LessonId = dto.LessonId,
                    Status = "InProgress"
                };
                _context.UserProgresses.Add(progress);
            }

            // chỉ cập nhật nếu điểm cao hơn
            if (score > progress.BestQuizScore)
            {
                progress.BestQuizScore = score;
            }

            // nếu đủ điều kiện thì hoàn thành
            if (progress.BestQuizScore >= 80 &&
                progress.ReadVocab &&
                progress.ReadGrammar &&
                progress.ReadKanji)
            {
                progress.Status = "Completed";
                progress.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                correct,
                total,
                score,
                results
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetQuizStatus(int lessonId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var results = await _context.QuizResults
                .Where(x => x.UserId == userId && x.LessonId == lessonId)
                .OrderByDescending(x => x.Score)
                .ToListAsync();

            var best = results.FirstOrDefault(x => x.Score >= 80);

            return Json(new
            {
                hasDone = best != null,          // ❗ CHỈ TRUE KHI PASS
                passed = best != null,
                score = best?.Score ?? 0
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetQuizHistory(int lessonId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var list = await _context.QuizResults
                .Where(x => x.UserId == userId && x.LessonId == lessonId)
                .OrderByDescending(x => x.DateTaken)
                .Select(x => new
                {
                    score = x.Score,
                    dateTaken = x.DateTaken
                })
                .ToListAsync();

            return Json(list);
        }

    }
}
