using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace JapaneseLearningWeb.Controllers
{
    public class VocabulariesController : Controller
    {
        private readonly AppDbContext _context;

        public VocabulariesController(AppDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách từ vựng với phân trang
        [Authorize]
        public async Task<IActionResult> Index(int? courseId, int pageNumber = 1, int pageSize = 10, string keyword = "")
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Lấy danh sách khóa học người dùng đã mua
            var purchasedCourseIds = await _context.UserCourses
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CourseId)
                .ToListAsync();

            // Lấy tất cả các khóa học người dùng được phép truy cập (mua hoặc miễn phí)
            var accessibleCourses = await _context.Courses
                .Where(c => purchasedCourseIds.Contains(c.CourseId) || c.IsFree)
                .ToListAsync();

            ViewBag.Courses = accessibleCourses;
            ViewBag.SelectedCourseId = courseId;

            // Nếu chưa chọn hoặc chọn khóa học không hợp lệ
            if (courseId == null || !accessibleCourses.Any(c => c.CourseId == courseId.Value))
            {
                return View(new PagedListViewModel<Vocabulary>
                {
                    Items = new List<Vocabulary>(),
                    TotalItems = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Keyword = keyword
                });
            }

            // Truy vấn từ vựng thuộc các bài học của khóa học đã chọn
            var query = _context.Vocabularies
                .Include(v => v.Lesson)
                .Where(v => v.Lesson.CourseId == courseId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(v =>
                    v.Kanji.Contains(keyword) ||
                    v.Hiragana.Contains(keyword) ||
                    v.Romaji.Contains(keyword) ||
                    v.Meaning.Contains(keyword) ||
                    v.Lesson.Title.Contains(keyword));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(v => v.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new PagedListViewModel<Vocabulary>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Keyword = keyword
            };

            return View(model);
        }

        // Hiển thị chi tiết từ vựng
        public async Task<IActionResult> Details(int id)
        {
            var vocabulary = await _context.Vocabularies
                .Include(v => v.Lesson)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vocabulary == null)
                return NotFound();

            return View(vocabulary);
        }

        // GET: Hiển thị form tạo mới từ vựng
        public IActionResult Create()
        {
            // Lấy danh sách Lesson để bind dropdown
            var lessons = _context.Lessons
                                  .Select(l => new { l.LessonId, l.Title })
                                  .ToList();

            if (!lessons.Any())
            {
                TempData["Error"] = "Không có bài học nào. Vui lòng tạo bài học trước khi tạo từ vựng.";
                return RedirectToAction("Index", "Lessons");
            }

            ViewBag.Lessons = new SelectList(lessons, "LessonId", "Title");
            return View();
        }

        // POST: Xử lý tạo mới từ vựng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LessonId,Kanji,Hiragana,Romaji,Meaning")] Vocabulary vocabulary)
        {
            if (ModelState.IsValid)
            {
                _context.Vocabularies.Add(vocabulary);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu validation thất bại, load lại danh sách Lesson để dropdown không null
            var lessons = _context.Lessons
                                  .Select(l => new { l.LessonId, l.Title })
                                  .ToList();
            ViewBag.Lessons = new SelectList(lessons, "LessonId", "Title", vocabulary.LessonId);
            return View(vocabulary);
        }

        // GET: Hiển thị form chỉnh sửa từ vựng
        public async Task<IActionResult> Edit(int id)
        {
            var vocabulary = await _context.Vocabularies.FindAsync(id);
            if (vocabulary == null)
                return NotFound();

            var lessons = _context.Lessons
                                  .Select(l => new { l.LessonId, l.Title })
                                  .ToList();

            if (!lessons.Any())
            {
                TempData["Error"] = "Không có bài học nào. Vui lòng tạo bài học trước khi chỉnh sửa từ vựng.";
                return RedirectToAction("Index", "Lessons");
            }

            ViewBag.Lessons = new SelectList(lessons, "LessonId", "Title", vocabulary.LessonId);
            return View(vocabulary);
        }

        // POST: Xử lý chỉnh sửa từ vựng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LessonId,Kanji,Hiragana,Romaji,Meaning")] Vocabulary vocabulary)
        {
            if (id != vocabulary.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vocabulary);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VocabularyExists(vocabulary.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Nếu validation lỗi, load lại danh sách Lesson
            var lessons = _context.Lessons
                                  .Select(l => new { l.LessonId, l.Title })
                                  .ToList();
            ViewBag.Lessons = new SelectList(lessons, "LessonId", "Title", vocabulary.LessonId);
            return View(vocabulary);
        }

        // GET: Hiển thị form xóa từ vựng
        public async Task<IActionResult> Delete(int id)
        {
            var vocabulary = await _context.Vocabularies
                                           .Include(v => v.Lesson)
                                           .FirstOrDefaultAsync(v => v.Id == id);

            if (vocabulary == null)
                return NotFound();

            return View(vocabulary);
        }

        // POST: Xử lý xóa từ vựng
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vocabulary = await _context.Vocabularies.FindAsync(id);
            if (vocabulary == null)
                return NotFound();

            _context.Vocabularies.Remove(vocabulary);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Tìm kiếm từ vựng
        [HttpGet]
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return RedirectToAction(nameof(Index));

            var results = await _context.Vocabularies
                .Include(v => v.Lesson)
                .Where(v => v.Kanji.Contains(keyword)
                         || v.Hiragana.Contains(keyword)
                         || v.Romaji.Contains(keyword)
                         || v.Meaning.Contains(keyword)
                         || v.Lesson.Title.Contains(keyword))
                .ToListAsync();

            var model = new PagedListViewModel<Vocabulary>
            {
                Items = results,
                TotalItems = results.Count,
                PageNumber = 1,
                PageSize = results.Count,
                Keyword = keyword
            };

            return View("Index", model);
        }

        private bool VocabularyExists(int id)
        {
            return _context.Vocabularies.Any(e => e.Id == id);
        }
    }
}
