
using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JapaneseLearningWeb.Controllers

{
    [Authorize]
    public class GrammarsController : Controller
    {
        private readonly AppDbContext _context;

        public GrammarsController(AppDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách ngữ pháp với phân trang
        [Authorize]
        public async Task<IActionResult> Index(int? courseId, int pageNumber = 1, int pageSize = 10, string keyword = "")
        {
            // Lấy ID người dùng hiện tại
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Lấy danh sách ID các khóa học người dùng đã mua hoặc miễn phí
            var purchasedCourseIds = await _context.UserCourses
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CourseId)
                .ToListAsync();

            var courses = await _context.Courses
                .Where(c => purchasedCourseIds.Contains(c.CourseId) || c.IsFree)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;

            // Nếu chưa chọn khóa học hoặc không thuộc danh sách được truy cập
            if (courseId == null || !courses.Any(c => c.CourseId == courseId.Value))
            {
                return View(new PagedListViewModel<Grammar>
                {
                    Items = new List<Grammar>(),
                    TotalItems = 0,
                    PageNumber = 1,
                    PageSize = pageSize,
                    Keyword = keyword
                });
            }

            // Truy vấn danh sách ngữ pháp của bài học thuộc khóa học đã chọn
            var query = _context.Grammars
                .Include(g => g.Lesson)
                .ThenInclude(l => l.Course)
                .Where(g => g.Lesson.CourseId == courseId)
                .AsQueryable();

            // Tìm kiếm theo từ khóa trong Rule, Example, Notes
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(g =>
                    g.Rule.Contains(keyword) ||
                    g.Example.Contains(keyword) ||
                    g.Notes.Contains(keyword));
            }

            // Đếm tổng số ngữ pháp phù hợp
            var totalItems = await query.CountAsync();

            // Lấy dữ liệu phân trang
            var items = await query
                .OrderBy(g => g.Lesson.OrderInCourse) // nếu Lesson có OrderInCourse
.ThenBy(g => g.GrammarId)

                .ThenBy(g => g.GrammarId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tạo model phân trang
            var model = new PagedListViewModel<Grammar>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Keyword = keyword
            };

            return View(model);
        }


        // Hiển thị chi tiết ngữ pháp
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var grammar = await _context.Grammars
                .Include(g => g.Lesson)
                .FirstOrDefaultAsync(g => g.GrammarId == id);
            if (grammar == null)
            {
                return NotFound();
            }
            return View(grammar);
        }

        // Hiển thị form tạo mới ngữ pháp
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var lessons = await _context.Lessons.ToListAsync();
            if (!lessons.Any())
            {
                TempData["Error"] = "Không có bài học nào. Vui lòng tạo bài học trước khi tạo ngữ pháp.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Lessons = lessons.Select(l => new SelectListItem
            {
                Text = l.Title,
                Value = l.LessonId.ToString()
            }).ToList();

            return View(new Grammar());
        }

        // Xử lý tạo mới ngữ pháp
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("GrammarId,LessonId,Rule,Example,Level,Notes")] Grammar grammar)
        {
            if (ModelState.IsValid)
            {
                grammar.CreatedAt = DateTime.Now;
                _context.Grammars.Add(grammar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Lessons = await _context.Lessons.ToListAsync();
            return View(grammar);
        }

        // Hiển thị form chỉnh sửa ngữ pháp
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id)
        {
            var grammar = await _context.Grammars
                .Include(g => g.Lesson)
                .FirstOrDefaultAsync(g => g.GrammarId == id);
            if (grammar == null)
            {
                return NotFound();
            }

            var lessons = await _context.Lessons.ToListAsync();
            ViewBag.Lessons = lessons;
            return View(grammar);
        }

        // Xử lý chỉnh sửa ngữ pháp
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id, [Bind("GrammarId,LessonId,Rule,Example,Level,Notes,CreatedAt")] Grammar grammar)
        {
            if (id != grammar.GrammarId)
            {
                return BadRequest("ID trong URL không khớp với ID trong model.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingGrammar = await _context.Grammars.AsNoTracking().FirstOrDefaultAsync(g => g.GrammarId == id);
                    if (existingGrammar == null)
                    {
                        return NotFound();
                    }

                    _context.Entry(grammar).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Grammars.Any(e => e.GrammarId == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            ViewBag.Lessons = await _context.Lessons.ToListAsync();
            return View(grammar);
        }

        // Hiển thị form xác nhận xóa ngữ pháp
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var grammar = await _context.Grammars
                .Include(g => g.Lesson)
                .FirstOrDefaultAsync(g => g.GrammarId == id);
            if (grammar == null)
            {
                return NotFound();
            }
            return View(grammar);
        }

        // Xử lý xóa ngữ pháp
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grammar = await _context.Grammars.FindAsync(id);
            if (grammar == null)
            {
                return NotFound();
            }

            _context.Grammars.Remove(grammar);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Tìm kiếm ngữ pháp
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction(nameof(Index));
            }

            var query = _context.Grammars
                .Include(g => g.Lesson)
                .Where(g => g.Rule.Contains(keyword) || g.Example.Contains(keyword) || g.Notes.Contains(keyword));

            var results = await query.ToListAsync();

            var model = new PagedListViewModel<Grammar>
            {
                Items = results,
                TotalItems = results.Count,
                PageNumber = 1,
                PageSize = results.Count,
                Keyword = keyword
            };

            return View("Index", model);
        }
    }
}