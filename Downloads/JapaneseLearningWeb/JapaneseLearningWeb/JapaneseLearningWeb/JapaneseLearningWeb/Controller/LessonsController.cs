
//using JapaneseLearningWeb.Data;
//using JapaneseLearningWeb.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.EntityFrameworkCore;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Identity;

//namespace JapaneseLearningWeb.Controllers
//{
//    public class LessonsController : Controller
//    {
//        private readonly AppDbContext _context;
//        private readonly UserManager<IdentityUser> _userManager;

//        public LessonsController(AppDbContext context, UserManager<IdentityUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        // Hiển thị danh sách bài học với phân trang
//        //public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 6, string keyword = "")
//        //{
//        //    pageSize = pageSize <= 0 ? 10 : pageSize; // Đảm bảo pageSize không bao giờ là 0
//        //    var query = _context.Lessons
//        //        .Include(l => l.Course)
//        //        .AsQueryable();

//        //    if (!string.IsNullOrEmpty(keyword))
//        //    {
//        //        keyword = keyword.Replace("%", "[%]").Replace("_", "[_]");
//        //        query = query.Where(l => l.Title.Contains(keyword) || l.Content.Contains(keyword));
//        //    }

//        //    var totalItems = await query.CountAsync();
//        //    var items = await query
//        //        .Skip((pageNumber - 1) * pageSize)
//        //        .Take(pageSize)
//        //        .ToListAsync() ?? new List<Lesson>();

//        //    var model = new PagedListViewModel<Lesson>
//        //    {
//        //        Items = items,
//        //        TotalItems = totalItems,
//        //        PageNumber = pageNumber,
//        //        PageSize = pageSize,
//        //        Keyword = keyword
//        //    };

//        //    return View(model);
//        //}

//        //public async Task<IActionResult> Index(int? courseId, int pageNumber = 1, int pageSize = 6, string keyword = "")
//        //{
//        //    var courses = await _context.Courses.ToListAsync();
//        //    ViewBag.Courses = courses;
//        //    ViewBag.SelectedCourseId = courseId;

//        //    if (courseId == null)
//        //    {
//        //        // Nếu chưa chọn khóa học, chỉ hiển thị danh sách khóa học
//        //        return View(new PagedListViewModel<Lesson>
//        //        {
//        //            Items = new List<Lesson>(),
//        //            TotalItems = 0,
//        //            PageNumber = 1,
//        //            PageSize = pageSize,
//        //            Keyword = keyword
//        //        });
//        //    }

//        //    pageSize = pageSize <= 0 ? 10 : pageSize;
//        //    var query = _context.Lessons
//        //        .Include(l => l.Course)
//        //        .Where(l => l.CourseId == courseId)
//        //        .AsQueryable();

//        //    if (!string.IsNullOrEmpty(keyword))
//        //    {
//        //        keyword = keyword.Replace("%", "[%]").Replace("_", "[_]");
//        //        query = query.Where(l => l.Title.Contains(keyword) || l.Content.Contains(keyword));
//        //    }

//        //    var totalItems = await query.CountAsync();
//        //    var items = await query
//        //        .Skip((pageNumber - 1) * pageSize)
//        //        .Take(pageSize)
//        //        .ToListAsync();

//        //    var model = new PagedListViewModel<Lesson>
//        //    {
//        //        Items = items,
//        //        TotalItems = totalItems,
//        //        PageNumber = pageNumber,
//        //        PageSize = pageSize,
//        //        Keyword = keyword
//        //    };

//        //    return View(model);
//        //}

//        [Authorize]
//        public async Task<IActionResult> Index(int? courseId, int pageNumber = 1, int pageSize = 6, string keyword = "")
//        {
//            // Lấy ID người dùng hiện tại
//            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

//            // Lấy danh sách ID các khóa học mà người dùng đã mua
//            var purchasedCourseIds = await _context.UserCourses
//                .Where(uc => uc.UserId == userId)
//                .Select(uc => uc.CourseId)
//                .ToListAsync();

//            // Lấy danh sách khóa học đã mua hoặc miễn phí
//            var courses = await _context.Courses
//                .Where(c => purchasedCourseIds.Contains(c.CourseId) || c.IsFree)
//                .ToListAsync();

//            ViewBag.Courses = courses;
//            ViewBag.SelectedCourseId = courseId;

//            // Nếu chưa chọn khóa học hoặc khóa học không thuộc danh sách được phép học
//            if (courseId == null || !courses.Any(c => c.CourseId == courseId.Value))
//            {
//                return View(new PagedListViewModel<Lesson>
//                {
//                    Items = new List<Lesson>(),
//                    TotalItems = 0,
//                    PageNumber = 1,
//                    PageSize = pageSize,
//                    Keyword = keyword
//                });
//            }

//            // Phân trang và tìm kiếm bài học trong khóa học đã chọn
//            pageSize = pageSize <= 0 ? 10 : pageSize;

//            var query = _context.Lessons
//                .Include(l => l.Course)
//                .Where(l => l.CourseId == courseId)
//                .AsQueryable();

//            if (!string.IsNullOrEmpty(keyword))
//            {
//                keyword = keyword.Replace("%", "[%]").Replace("_", "[_]"); // Escape ký tự SQL
//                query = query.Where(l => l.Title.Contains(keyword) || l.Content.Contains(keyword));
//            }

//            var totalItems = await query.CountAsync();
//            var items = await query
//                .Skip((pageNumber - 1) * pageSize)
//                .Take(pageSize)
//                .ToListAsync();

//            var model = new PagedListViewModel<Lesson>
//            {
//                Items = items,
//                TotalItems = totalItems,
//                PageNumber = pageNumber,
//                PageSize = pageSize,
//                Keyword = keyword
//            };

//            return View(model);
//        }


//        // Hiển thị chi tiết bài học
//        public async Task<IActionResult> Details(int id)
//        {
//            var lesson = await _context.Lessons
//                .Include(l => l.Course)
//                .Include(l => l.Kanjis)
//                .Include(l => l.Vocabularies) // Bao gồm từ vựng
//                .Include(l => l.Grammars)    // Bao gồm ngữ pháp
//                .FirstOrDefaultAsync(l => l.LessonId == id);

//            if (lesson == null)
//            {
//                return NotFound();
//            }
//            return View(lesson);
//        }

//        // Hiển thị form tạo mới bài học
//        public IActionResult Create()
//        {
//            var courses = _context.Courses.ToList();
//            if (!courses.Any())
//            {
//                TempData["Error"] = "Không có khóa học nào. Vui lòng tạo khóa học trước khi tạo bài học.";
//                return RedirectToAction(nameof(Index));
//            }
//            ViewBag.Courses = courses;
//            return View(new Lesson());
//        }

//        // Xử lý tạo mới bài học
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("LessonId,Title,Content,CourseId,OrderInCourse,Vocabularies,Grammars")] Lesson lesson)
//        {
//            if (ModelState.IsValid)
//            {
//                lesson.CreatedAt = DateTime.Now; // Đảm bảo CreatedAt được gán
//                _context.Lessons.Add(lesson);

//                // Gán LessonId cho từ vựng và ngữ pháp
//                if (lesson.Vocabularies != null)
//                {
//                    foreach (var vocab in lesson.Vocabularies)
//                    {
//                        vocab.LessonId = lesson.LessonId;
//                    }
//                }
//                if (lesson.Grammars != null)
//                {
//                    foreach (var grammar in lesson.Grammars)
//                    {
//                        grammar.LessonId = lesson.LessonId;
//                    }
//                }

//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }

//            // Nếu validation thất bại, gán lại ViewBag.Courses để render lại form
//            ViewBag.Courses = _context.Courses.ToList();
//            return View(lesson);
//        }

//        // Hiển thị form chỉnh sửa bài học
//        public async Task<IActionResult> Edit(int id)
//        {
//            var lesson = await _context.Lessons
//                .Include(l => l.Course)
//                .Include(l => l.Vocabularies) // Bao gồm từ vựng
//                .Include(l => l.Grammars)    // Bao gồm ngữ pháp
//                .FirstOrDefaultAsync(l => l.LessonId == id);

//            if (lesson == null)
//            {
//                return NotFound();
//            }

//            var courses = await _context.Courses.ToListAsync();
//            if (courses == null || !courses.Any())
//            {
//                TempData["Error"] = "Không có khóa học nào. Vui lòng tạo khóa học trước khi chỉnh sửa bài học.";
//                return RedirectToAction(nameof(Index));
//            }

//            ViewBag.Courses = courses;
//            return View(lesson);
//        }

//        // Xử lý chỉnh sửa bài học
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("LessonId,Title,Content,CourseId,OrderInCourse,CreatedAt,Vocabularies,Grammars")] Lesson lesson)
//        {
//            if (id != lesson.LessonId)
//            {
//                return BadRequest("ID trong URL không khớp với ID trong model.");
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    // Kiểm tra xem bài học có tồn tại không
//                    var existingLesson = await _context.Lessons
//                        .Include(l => l.Vocabularies)
//                        .Include(l => l.Grammars)
//                        .AsNoTracking()
//                        .FirstOrDefaultAsync(l => l.LessonId == id);

//                    if (existingLesson == null)
//                    {
//                        return NotFound();
//                    }

//                    // Xóa từ vựng và ngữ pháp cũ
//                    _context.Vocabularies.RemoveRange(_context.Vocabularies.Where(v => v.LessonId == id));
//                    _context.Grammars.RemoveRange(_context.Grammars.Where(g => g.LessonId == id));

//                    // Thêm từ vựng và ngữ pháp mới
//                    if (lesson.Vocabularies != null)
//                    {
//                        foreach (var vocab in lesson.Vocabularies)
//                        {
//                            vocab.LessonId = id;
//                            _context.Vocabularies.Add(vocab);
//                        }
//                    }
//                    if (lesson.Grammars != null)
//                    {
//                        foreach (var grammar in lesson.Grammars)
//                        {
//                            grammar.LessonId = id;
//                            _context.Grammars.Add(grammar);
//                        }
//                    }

//                    // Cập nhật bài học
//                    _context.Entry(lesson).State = EntityState.Modified;
//                    await _context.SaveChangesAsync();
//                    return RedirectToAction(nameof(Index));
//                }
//                catch (DbUpdateConcurrencyException ex)
//                {
//                    Console.WriteLine($"Concurrency error: {ex.Message}");
//                    if (!_context.Lessons.Any(e => e.LessonId == id))
//                    {
//                        return NotFound();
//                    }
//                    throw;
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error updating lesson: {ex.Message}");
//                    return View(lesson); // Trả về view với lỗi
//                }
//            }

//            // Nếu ModelState không hợp lệ, gán lại ViewBag.Courses
//            ViewBag.Courses = await _context.Courses.ToListAsync();
//            return View(lesson);
//        }

//        // Hiển thị form xác nhận xóa bài học
//        public async Task<IActionResult> Delete(int id)
//        {
//            var lesson = await _context.Lessons
//                .Include(l => l.Course)
//                .Include(l => l.Vocabularies) // Bao gồm từ vựng
//                .Include(l => l.Grammars)    // Bao gồm ngữ pháp
//                .FirstOrDefaultAsync(l => l.LessonId == id);

//            if (lesson == null)
//            {
//                return NotFound();
//            }
//            return View(lesson);
//        }

//        // Xử lý xóa bài học
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var lesson = await _context.Lessons
//                .Include(l => l.Vocabularies) // Bao gồm từ vựng để xóa
//                .Include(l => l.Grammars)    // Bao gồm ngữ pháp để xóa
//                .FirstOrDefaultAsync(l => l.LessonId == id);

//            if (lesson == null)
//            {
//                return NotFound();
//            }

//            // Xóa từ vựng và ngữ pháp liên quan
//            _context.Vocabularies.RemoveRange(lesson.Vocabularies);
//            _context.Grammars.RemoveRange(lesson.Grammars);
//            _context.Lessons.Remove(lesson);

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        // Tìm kiếm bài học
//        [HttpGet]
//        public async Task<IActionResult> Search(string keyword)
//        {
//            if (string.IsNullOrEmpty(keyword))
//            {
//                return RedirectToAction(nameof(Index));
//            }

//            keyword = keyword.Replace("%", "[%]").Replace("_", "[_]");
//            var results = await _context.Lessons
//                .Include(l => l.Course)
//                .Include(l => l.Vocabularies) // Bao gồm từ vựng
//                .Include(l => l.Grammars)    // Bao gồm ngữ pháp
//                .Where(l => l.Title.Contains(keyword) || l.Content.Contains(keyword))
//                .ToListAsync();

//            var model = new PagedListViewModel<Lesson>
//            {
//                Items = results,
//                TotalItems = results.Count,
//                PageNumber = 1,
//                PageSize = results.Count,
//                Keyword = keyword
//            };

//            return View("Index", model);
//        }
//        // Thêm hành động để cập nhật link YouTube
//        [HttpPost]
//        [Authorize] // Yêu cầu người dùng đã đăng nhập
//        public async Task<IActionResult> UpdateYouTubeLink(int lessonId, string contentType, string youtubeLink)
//        {
//            // Kiểm tra vai trò
//            if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
//            {
//                return Forbid("Bạn không có quyền thực hiện hành động này. Chỉ Admin và Employee được phép thêm video.");
//            }

//            if (string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(youtubeLink))
//            {
//                return BadRequest("Thiếu thông tin loại nội dung hoặc link YouTube.");
//            }

//            var lesson = await _context.Lessons
//                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

//            if (lesson == null)
//            {
//                return NotFound("Không tìm thấy bài học.");
//            }

//            // Chuyển đổi short URL sang embed URL nếu cần
//            if (youtubeLink.StartsWith("https://youtu.be/"))
//            {
//                string videoId = youtubeLink.Split("https://youtu.be/")[1].Split("?")[0]; // Lấy videoId, bỏ tham số
//                youtubeLink = $"https://www.youtube.com/embed/{videoId}";
//            }
//            else if (!youtubeLink.StartsWith("https://www.youtube.com/embed/"))
//            {
//                return BadRequest("Link YouTube không hợp lệ. Vui lòng sử dụng dạng https://www.youtube.com/embed/VIDEO_ID hoặc https://youtu.be/VIDEO_ID.");
//            }

//            // Cập nhật link dựa trên contentType
//            if (contentType == "vocab")
//            {
//                lesson.VocabYouTubeLink = youtubeLink;
//            }
//            else if (contentType == "grammar")
//            {
//                lesson.GrammarYouTubeLink = youtubeLink;
//            }
//            else
//            {
//                return BadRequest("Loại nội dung không hợp lệ.");
//            }

//            await _context.SaveChangesAsync();
//            return Ok(new { message = "Cập nhật link YouTube thành công." });
//        }
//        [HttpGet]
//        public IActionResult GetYouTubeLink(int lessonId, string contentType)
//        {
//            var lesson = _context.Lessons.FirstOrDefault(l => l.LessonId == lessonId);
//            if (lesson == null)
//            {
//                return NotFound();
//            }

//            string youtubeLink = "";
//            if (contentType == "vocab")
//            {
//                youtubeLink = lesson.VocabYouTubeLink;
//            }
//            else if (contentType == "grammar")
//            {
//                youtubeLink = lesson.GrammarYouTubeLink;
//            }

//            return Ok(new { youtubeLink });
//        }
//        [HttpGet]
//        public IActionResult GetLessonContent(int id, string type)
//        {
//            // Kiểm tra dữ liệu đầu vào
//            if (string.IsNullOrEmpty(type))
//                return BadRequest("Missing type");

//            // Gọi dữ liệu theo type
//            var lesson = _context.Lessons
//                  .Include(l => l.Vocabularies)
//                  .Include(l => l.Grammars)
//                  .Include(l => l.Kanjis)
//                  .ThenInclude(k => k.KanjiKanjiRadicals)
//             .ThenInclude(kr => kr.KanjiRadical)
//                  .FirstOrDefault(l => l.LessonId == id);

//            if (lesson == null)
//                return NotFound();

//            string youtubeLink = "";
//            switch (type)
//            {
//                case "vocab":
//                    youtubeLink = lesson.VocabYouTubeLink;
//                    return PartialView("_VocabPartial", lesson.Vocabularies);
//                case "grammar":
//                    youtubeLink = lesson.GrammarYouTubeLink;
//                    return PartialView("_GrammarPartial", lesson.Grammars);
//                case "kanji":
//                    return PartialView("_KanjiPartial", lesson.Kanjis);
//                default:
//                    return BadRequest("Invalid content type");
//            }
//        }

//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using JapaneseLearningWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JapaneseLearningWeb.Controllers
{
    [Authorize]
    public class LessonsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly WeeklyAsrService _weeklyAsrService;
        public LessonsController(AppDbContext context, WeeklyAsrService weeklyAsrService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _weeklyAsrService = weeklyAsrService;
            _userManager = userManager;
        }

        private string CurrentUserId()
            => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        // ===========================
        // 1) Danh sách & chi tiết
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Index(int? courseId, int pageNumber = 1, int pageSize = 6, string keyword = "")
        {
            var userId = CurrentUserId();

            var purchasedCourseIds = await _context.UserCourses
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CourseId)
                .ToListAsync();

            var courses = await _context.Courses
                .Where(c => purchasedCourseIds.Contains(c.CourseId) || c.IsFree)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;

            if (courseId == null || !courses.Any(c => c.CourseId == courseId.Value))
            {
                return View(new PagedListViewModel<Lesson>
                {
                    Items = new List<Lesson>(),
                    TotalItems = 0,
                    PageNumber = 1,
                    PageSize = pageSize,
                    Keyword = keyword
                });
            }

            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _context.Lessons
                .Include(l => l.Course)
                .Where(l => l.CourseId == courseId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Replace("%", "[%]").Replace("_", "[_]");
                query = query.Where(l => l.Title.Contains(keyword) || l.Content.Contains(keyword));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(l => l.OrderInCourse)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new PagedListViewModel<Lesson>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Keyword = keyword
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .Include(l => l.Kanjis)
                .Include(l => l.Vocabularies)
                .Include(l => l.Grammars)
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null) return NotFound();
            return View(lesson);
        }

        // ===========================
        // 2) Load nội dung theo tab (vocab/grammar/kanji)
        // ===========================
        [HttpGet]
        public IActionResult GetLessonContent(int id, string type)
        {
            if (string.IsNullOrEmpty(type)) return BadRequest("Missing type");

            var lesson = _context.Lessons
                .Include(l => l.Vocabularies)
                .Include(l => l.Grammars)
                .Include(l => l.Kanjis)
                .ThenInclude(k => k.KanjiKanjiRadicals)
                .ThenInclude(kr => kr.KanjiRadical)
                .FirstOrDefault(l => l.LessonId == id);

            if (lesson == null) return NotFound();

            switch (type)
            {
                case "vocab":
                    return PartialView("_VocabPartial", lesson.Vocabularies);
                case "grammar":
                    return PartialView("_GrammarPartial", lesson.Grammars);
                case "kanji":
                    return PartialView("_KanjiPartial", lesson.Kanjis);
                default:
                    return BadRequest("Invalid content type");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null)
                return NotFound();

            ViewBag.Courses = await _context.Courses.ToListAsync();
            return View(lesson);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Lesson lesson)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Courses = await _context.Courses.ToListAsync();
                return View(lesson);
            }

            _context.Update(lesson);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // ===========================
        // 3) YouTube link cho từng tab
        // ===========================
        [HttpGet]
        public IActionResult GetYouTubeLink(int lessonId, string contentType)
        {
            var lesson = _context.Lessons.FirstOrDefault(l => l.LessonId == lessonId);
            if (lesson == null) return NotFound();

            string youtubeLink = "";
            if (contentType == "vocab") youtubeLink = lesson.VocabYouTubeLink;
            else if (contentType == "grammar") youtubeLink = lesson.GrammarYouTubeLink;

            return Ok(new { youtubeLink });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateYouTubeLink(int lessonId, string contentType, string youtubeLink)
        {
            if (string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(youtubeLink))
                return BadRequest("Thiếu thông tin.");

            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonId == lessonId);
            if (lesson == null) return NotFound("Không tìm thấy bài học.");

            // Chuẩn hóa embed
            if (youtubeLink.StartsWith("https://youtu.be/"))
            {
                var videoId = youtubeLink.Split("https://youtu.be/")[1].Split("?")[0];
                youtubeLink = $"https://www.youtube.com/embed/{videoId}";
            }
            else if (!youtubeLink.StartsWith("https://www.youtube.com/embed/"))
            {
                return BadRequest("Link YouTube không hợp lệ. Dùng https://www.youtube.com/embed/VIDEO_ID hoặc https://youtu.be/VIDEO_ID.");
            }

            if (contentType == "vocab") lesson.VocabYouTubeLink = youtubeLink;
            else if (contentType == "grammar") lesson.GrammarYouTubeLink = youtubeLink;
            else return BadRequest("Loại nội dung không hợp lệ.");

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật link YouTube thành công." });
        }

        // ===========================
        // 4) Quiz (Student)
        // ===========================
        public class GetQuizResponse
        {
            public List<QuestionDto> Questions { get; set; } = new();
            public class QuestionDto
            {
                public int Id { get; set; }
                public string Text { get; set; } = "";
                public List<ChoiceDto> Choices { get; set; } = new();
            }
            public class ChoiceDto
            {
                public int Id { get; set; }
                public string Text { get; set; } = "";
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetQuiz(int lessonId)
        {
            // Lấy quiz theo lesson
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.LessonId == lessonId);

            var res = new GetQuizResponse();

            if (quiz != null)
            {
                foreach (var q in quiz.Questions.OrderBy(x => x.QuestionId))
                {
                    res.Questions.Add(new GetQuizResponse.QuestionDto
                    {
                        Id = q.QuestionId,
                        Text = q.Text,
                        Choices = q.Options.OrderBy(o => o.OptionId)
                            .Select(o => new GetQuizResponse.ChoiceDto { Id = o.OptionId, Text = o.Text }).ToList()
                    });
                }
            }

            return Ok(res);
        }

        public class SubmitQuizRequest
        {
            public int LessonId { get; set; }
            public List<AnswerItem> Answers { get; set; } = new();
            public class AnswerItem
            {
                public int QuestionId { get; set; }
                public int ChoiceId { get; set; }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizRequest req)
        {
            try
            {
                if (req == null || req.LessonId <= 0) return BadRequest("Dữ liệu không hợp lệ.");

                var uid = CurrentUserId();
                if (string.IsNullOrEmpty(uid)) return Unauthorized("Bạn chưa đăng nhập.");

                // 1) Lấy danh sách QuestionId thuộc lesson
                var lessonQuestionIds = await _context.Questions
                    .Where(q => q.Quiz.LessonId == req.LessonId)
                    .Select(q => q.QuestionId)
                    .ToListAsync();

                if (lessonQuestionIds.Count == 0) return BadRequest("Bài này chưa có câu hỏi.");

                // 2) Lọc các câu đã nộp chỉ giữ câu thuộc bài & duy nhất
                var submitted = (req.Answers ?? new List<SubmitQuizRequest.AnswerItem>())
                    .GroupBy(a => a.QuestionId).Select(g => g.First())
                    .Where(a => lessonQuestionIds.Contains(a.QuestionId))
                    .ToList();

                if (submitted.Count == 0) return BadRequest("Bạn chưa chọn đáp án.");

                // 3) Map đáp án đúng
                var correctMap = await _context.Options
                    .Where(o => o.IsCorrect && lessonQuestionIds.Contains(o.QuestionId))
                    .Select(o => new { o.QuestionId, o.OptionId })
                    .ToListAsync();

                int total = submitted.Count;
                int correct = submitted.Count(a => correctMap.Any(c => c.QuestionId == a.QuestionId && c.OptionId == a.ChoiceId));
                int score = (int)Math.Round(correct * 100.0 / total);

                // 4) Lưu QuizResult (map đúng AspNetUsers)
                _context.QuizResults.Add(new QuizResult
                {
                    UserId = uid,
                    LessonId = req.LessonId,     // thêm cột này trong QuizResult model
                    Score = score,
                    DateTaken = DateTime.UtcNow
                });

                // 5) Cập nhật tiến độ (lưu best score)
                var prog = await _context.UserProgresses
                    .FirstOrDefaultAsync(x => x.LessonId == req.LessonId && x.UserId == uid);

                if (prog == null)
                {
                    prog = new UserProgress { LessonId = req.LessonId, UserId = uid };
                    _context.UserProgresses.Add(prog);
                }
                prog.BestQuizScore = Math.Max(prog.BestQuizScore, score);

                await _context.SaveChangesAsync();
                return Ok(new { score, correct, total });
            }
            catch (Exception ex)
            {
                var baseMsg = ex.GetBaseException()?.Message ?? ex.Message;
                return StatusCode(500, "Lỗi server khi nộp bài: " + baseMsg);
            }
        }

        // ===========================
        // 5) Quiz (Admin)
        // ===========================
        public class AdminGetQuizResponse
        {
            public List<QuestionAdminDto> Questions { get; set; } = new();
            public class QuestionAdminDto
            {
                public int Id { get; set; }
                public string Text { get; set; } = "";
                public List<OptionAdminDto> Options { get; set; } = new();
            }
            public class OptionAdminDto
            {
                public int Id { get; set; }
                public string Text { get; set; } = "";
                public bool IsCorrect { get; set; }
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> AdminGetQuiz(int lessonId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.LessonId == lessonId);

            var res = new AdminGetQuizResponse();
            if (quiz != null)
            {
                foreach (var q in quiz.Questions.OrderBy(x => x.QuestionId))
                {
                    res.Questions.Add(new AdminGetQuizResponse.QuestionAdminDto
                    {
                        Id = q.QuestionId,
                        Text = q.Text,
                        Options = q.Options.OrderBy(o => o.OptionId)
                            .Select(o => new AdminGetQuizResponse.OptionAdminDto
                            {
                                Id = o.OptionId,
                                Text = o.Text,
                                IsCorrect = o.IsCorrect
                            }).ToList()
                    });
                }
            }
            return Ok(res);
        }

        public class AdminAddQuestionRequest
        {
            public int LessonId { get; set; }
            public string Text { get; set; } = "";
            public List<ChoiceItem> Choices { get; set; } = new(); // chỉ text
            public int CorrectChoiceIndex { get; set; } = 0;

            public class ChoiceItem { public string Text { get; set; } = ""; }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> AdminAddQuestion([FromBody] AdminAddQuestionRequest req)
        {
            if (req == null || req.LessonId <= 0 || string.IsNullOrWhiteSpace(req.Text))
                return BadRequest("Thiếu dữ liệu.");
            if (req.Choices == null || req.Choices.Count < 2)
                return BadRequest("Cần ít nhất 2 lựa chọn.");

            // Lấy quiz của lesson, nếu chưa có thì tạo
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.LessonId == req.LessonId);
            if (quiz == null)
            {
                quiz = new Quiz
                {
                    CourseId = await _context.Lessons.Where(l => l.LessonId == req.LessonId).Select(l => l.CourseId).FirstAsync(),
                    LessonId = req.LessonId,
                    Title = $"Quiz bài {req.LessonId}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();
            }

            var q = new Question
            {
                QuizId = quiz.QuizId,
                Text = req.Text,
                CreatedAt = DateTime.UtcNow
            };
            _context.Questions.Add(q);
            await _context.SaveChangesAsync(); // cần Id cho Options

            for (int i = 0; i < req.Choices.Count; i++)
            {
                var ch = req.Choices[i];
                if (string.IsNullOrWhiteSpace(ch.Text)) continue;
                _context.Options.Add(new Option
                {
                    QuestionId = q.QuestionId,
                    Text = ch.Text,
                    IsCorrect = (i == req.CorrectChoiceIndex)
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> AdminDeleteQuestion(int questionId)
        {
            var q = await _context.Questions
                .Include(x => x.Options)
                .FirstOrDefaultAsync(x => x.QuestionId == questionId);

            if (q == null) return NotFound();
            _context.Options.RemoveRange(q.Options);
            _context.Questions.Remove(q);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Courses = await _context.Courses
                .OrderBy(c => c.Title)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lesson lesson)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Courses = await _context.Courses.ToListAsync();
                return View(lesson);
            }

            // Tự set ngày tạo
            lesson.CreatedAt = DateTime.UtcNow;

            // Tự sắp xếp thứ tự bài học trong khóa
            var maxOrder = await _context.Lessons
                .Where(l => l.CourseId == lesson.CourseId)
                .MaxAsync(l => (int?)l.OrderInCourse) ?? 0;

            lesson.OrderInCourse = maxOrder + 1;

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { courseId = lesson.CourseId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null)
                return NotFound();

            return View(lesson);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int lessonId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Kanjis)
                .Include(l => l.Vocabularies)
                .Include(l => l.Grammars)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null)
                return NotFound();

            int courseId = lesson.CourseId;

            // ❗ Xóa dữ liệu liên quan trước (tránh lỗi FK)
            _context.Kanjis.RemoveRange(lesson.Kanjis);
            _context.Vocabularies.RemoveRange(lesson.Vocabularies);
            _context.Grammars.RemoveRange(lesson.Grammars);

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { courseId });
        }




        // ===========================
        // 6) Đánh dấu sự kiện & lấy tiến độ
        // ===========================
        [HttpPost]
        public async Task<IActionResult> MarkEvent(int lessonId, string eventType)
        {
            var uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(uid)) return Unauthorized();

            var p = await _context.UserProgresses
                .FirstOrDefaultAsync(x => x.UserId == uid && x.LessonId == lessonId);

            if (p == null)
            {
                p = new UserProgress
                {
                    UserId = uid,
                    LessonId = lessonId,
                    Status = "InProgress"
                };
                _context.UserProgresses.Add(p);
            }

            switch (eventType)
            {
                case "video_vocab_done":
                    p.ReadVocab = true;
                    break;
                case "video_grammar_done":
                    p.ReadGrammar = true;
                    break;
                case "asr_passed":
                    // TẠM dùng ReadKanji để lưu cờ ASR đã đậu
                    p.ReadKanji = true;
                    break;
                default:
                    return BadRequest("Sự kiện không hợp lệ.");
            }

            // Nếu đã hoàn thành 100% thì cập nhật trạng thái
            if (IsLessonCompleted(p))
            {
                p.Status = "Completed";
                p.CompletedAt = DateTime.UtcNow;
            }
            else if (p.Status == "NotStarted")
            {
                p.Status = "InProgress";
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // helper
        private static bool IsLessonCompleted(UserProgress p)
        {
            // 4 mốc: video vocab, video grammar, ASR (tạm dùng ReadKanji), Quiz≥80
            int done = 0;
            if (p.ReadVocab) done++;
            if (p.ReadGrammar) done++;
            if (p.ReadKanji) done++;               // ASR passed (tạm map)
            if (p.BestQuizScore >= 80) done++;
            return done >= 4;
        }


        public class ProgressDto
        {
            public int Done { get; set; }
            public int Required { get; set; }
            public int Percent { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetProgress(int lessonId)
        {
            var uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(uid)) return Unauthorized();

            var p = await _context.UserProgresses
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == uid && x.LessonId == lessonId);

            // 4 mốc: video vocab, video grammar, ASR (tạm dùng ReadKanji), Quiz≥80
            int required = 4, done = 0;

            if (p?.ReadVocab == true) done++;
            if (p?.ReadGrammar == true) done++;
            if (p?.ReadKanji == true) done++;                 // ASR passed (tạm map)
            if ((p?.BestQuizScore ?? 0) >= 80) done++;

            int percent = (int)Math.Round(done * 100.0 / required);

            return Ok(new ProgressDto { Done = done, Required = required, Percent = percent });
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> AdminGetQuestion(int questionId)
        {
            var q = await _context.Questions
                .Include(x => x.Options)
                .FirstOrDefaultAsync(x => x.QuestionId == questionId);

            if (q == null) return NotFound();

            return Ok(new
            {
                id = q.QuestionId,
                text = q.Text,
                choices = q.Options
                    .OrderBy(o => o.OptionId)
                    .Select(o => new
                    {
                        id = o.OptionId,
                        text = o.Text,
                        isCorrect = o.IsCorrect
                    })
            });
        }

        public class AdminUpdateQuestionRequest
        {
            public int QuestionId { get; set; }
            public string Text { get; set; } = "";
            public List<ChoiceItem> Choices { get; set; } = new();
            public int CorrectChoiceIndex { get; set; }

            public class ChoiceItem
            {
                public string Text { get; set; } = "";
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminUpdateQuestion(
    [FromBody] AdminUpdateQuestionRequest req)
        {
            if (req == null || req.QuestionId <= 0 || string.IsNullOrWhiteSpace(req.Text))
                return BadRequest("Dữ liệu không hợp lệ.");

            if (req.Choices == null || req.Choices.Count < 2)
                return BadRequest("Cần ít nhất 2 đáp án.");

            if (req.CorrectChoiceIndex < 0 || req.CorrectChoiceIndex >= req.Choices.Count)
                return BadRequest("Đáp án đúng không hợp lệ.");

            var q = await _context.Questions
                .Include(x => x.Options)
                .FirstOrDefaultAsync(x => x.QuestionId == req.QuestionId);

            if (q == null) return NotFound("Không tìm thấy câu hỏi.");

            // cập nhật nội dung
            q.Text = req.Text;

            // xoá đáp án cũ
            _context.Options.RemoveRange(q.Options);

            // thêm đáp án mới
            for (int i = 0; i < req.Choices.Count; i++)
            {
                _context.Options.Add(new Option
                {
                    QuestionId = q.QuestionId,
                    Text = req.Choices[i].Text,
                    IsCorrect = (i == req.CorrectChoiceIndex)
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
