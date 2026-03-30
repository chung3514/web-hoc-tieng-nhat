using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace JapaneseLearningWeb.Controllers
{
    [Authorize]
    public class KanjisController : Controller
    {
        private readonly AppDbContext _context;
        public KanjisController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index(int? courseId, int pageNumber = 1, int pageSize = 6, string keyword = "")
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
                return View(new PagedListViewModel<Kanji>
                {
                    Items = new List<Kanji>(),
                    TotalItems = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Keyword = keyword
                });
            }

            // Truy vấn Kanji thuộc các bài học của khóa học đã chọn
            var query = _context.Kanjis
                .Include(k => k.KanjiKanjiRadicals)
                    .ThenInclude(kkr => kkr.KanjiRadical)
                .Include(k => k.Lesson)
                .Where(k => k.Lesson.CourseId == courseId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(k => k.Character.Contains(keyword) || k.Meaning.Contains(keyword));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(k => k.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new PagedListViewModel<Kanji>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Keyword = keyword
            };

            return View(model);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var lessons = _context.Lessons.ToList();
            if (!lessons.Any())
            {
                TempData["Error"] = "Không có bài học nào. Vui lòng tạo bài học trước khi tạo Kanji.";
                return RedirectToAction("Index", "Lessons");
            }

            ViewBag.Lessons = new SelectList(lessons, "LessonId", "Title");
            ViewBag.Radicals = _context.KanjiRadicals
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.RadicalKanji} - {r.RadicalHanjaMeaning}"
                }).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Kanji model, string SelectedRadicalIds)
        {
            if (ModelState.IsValid)
            {
                _context.Kanjis.Add(model);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(SelectedRadicalIds))
                {
                    var ids = SelectedRadicalIds.Split(',').Select(int.Parse).ToList();
                    foreach (var id in ids)
                    {
                        _context.KanjiKanjiRadicals.Add(new KanjiKanjiRadical
                        {
                            KanjiId = model.Id,
                            KanjiRadicalId = id
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Kanji đã được tạo thành công.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Lessons = new SelectList(_context.Lessons.ToList(), "LessonId", "Title", model.LessonId);
            ViewBag.Radicals = _context.KanjiRadicals
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.RadicalKanji} - {r.RadicalHanjaMeaning}"
                }).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var kanji = await _context.Kanjis
                .Include(k => k.KanjiKanjiRadicals)
                    .ThenInclude(kkr => kkr.KanjiRadical)
                .Include(k => k.Lesson)
                .Include(k => k.VocabularyKanjis)
                    .ThenInclude(vk => vk.Vocabulary)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (kanji == null)
                return NotFound();

            return View(kanji);
        }

        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id)
        {
            var kanji = await _context.Kanjis
                .Include(k => k.KanjiKanjiRadicals)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (kanji == null)
                return NotFound();

            ViewBag.Lessons = new SelectList(_context.Lessons.ToList(), "LessonId", "Title", kanji.LessonId);

            var selectedRadicals = kanji.KanjiKanjiRadicals.Select(x => x.KanjiRadicalId).ToList();
            ViewBag.Radicals = _context.KanjiRadicals
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.RadicalKanji} - {r.RadicalHanjaMeaning}",
                    Selected = selectedRadicals.Contains(r.Id)
                }).ToList();

            return View(kanji);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id, Kanji model, string[] SelectedRadicalIds)
        {
            if (id != model.Id)
                return BadRequest("ID không khớp.");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingKanji = await _context.Kanjis
                        .Include(k => k.KanjiKanjiRadicals)
                        .FirstOrDefaultAsync(k => k.Id == id);

                    if (existingKanji == null)
                        return NotFound();

                    existingKanji.Character = model.Character;
                    existingKanji.Meaning = model.Meaning;
                    existingKanji.OnYomi = model.OnYomi;
                    existingKanji.KunYomi = model.KunYomi;
                    existingKanji.LessonId = model.LessonId;

                    _context.KanjiKanjiRadicals.RemoveRange(existingKanji.KanjiKanjiRadicals);

                    if (SelectedRadicalIds != null && SelectedRadicalIds.Any())
                    {
                        var ids = SelectedRadicalIds.Select(int.Parse).ToList();
                        foreach (var idRadical in ids)
                        {
                            _context.KanjiKanjiRadicals.Add(new KanjiKanjiRadical
                            {
                                KanjiId = existingKanji.Id,
                                KanjiRadicalId = idRadical
                            });
                        }
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật Kanji thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Kanjis.Any(e => e.Id == id))
                        return NotFound();

                    throw;
                }
            }

            ViewBag.Lessons = new SelectList(_context.Lessons.ToList(), "LessonId", "Title", model.LessonId);
            ViewBag.Radicals = _context.KanjiRadicals
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.RadicalKanji} - {r.RadicalHanjaMeaning}"
                }).ToList();

            return View(model);
        }





        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var kanji = await _context.Kanjis
                .Include(k => k.Lesson)
                .Include(k => k.KanjiKanjiRadicals)
                    .ThenInclude(kkr => kkr.KanjiRadical)
                .Include(k => k.VocabularyKanjis)
                    .ThenInclude(vk => vk.Vocabulary)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (kanji == null)
                return NotFound();

            return View(kanji);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kanji = await _context.Kanjis.FindAsync(id);
            if (kanji == null)
                return NotFound();

            var links = _context.KanjiKanjiRadicals.Where(x => x.KanjiId == id);
            _context.KanjiKanjiRadicals.RemoveRange(links);
            _context.Kanjis.Remove(kanji);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Kanji đã được xóa thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return RedirectToAction(nameof(Index));

            var results = await _context.Kanjis
                .Include(k => k.KanjiKanjiRadicals)
                    .ThenInclude(kkr => kkr.KanjiRadical)
                .Include(k => k.Lesson)
                .Where(k => k.Character.Contains(keyword) || k.Meaning.Contains(keyword))
                .ToListAsync();

            var model = new PagedListViewModel<Kanji>
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
