using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JapaneseLearningWeb.Controllers
{
    [Authorize]
    public class KanjiRadicalController : Controller
    {
        private readonly AppDbContext _context;

        public KanjiRadicalController(AppDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 6, string keyword = "")
        {
            var radicals = _context.KanjiRadicals
                .Include(k => k.KanjiKanjiRadicals)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                radicals = radicals.Where(r => r.RadicalKanji.Contains(keyword) || r.RadicalHanjaMeaning.Contains(keyword));
            }

            var totalItems = await radicals.CountAsync();
            var items = await radicals
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new PagedListViewModel<KanjiRadical>
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
            var kanjis = _context.Kanjis.ToList();
            ViewBag.Kanjis = new SelectList(kanjis, "Id", "Character");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("RadicalKanji,RadicalHanjaMeaning,KanjiId")] KanjiRadical radical)
        {
            if (ModelState.IsValid)
            {
                _context.KanjiRadicals.Add(radical);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Bộ thủ đã được tạo thành công.";
                return RedirectToAction(nameof(Index));
            }

            var kanjis = _context.Kanjis.ToList();
            ViewBag.Kanjis = new SelectList(kanjis, "Id", "Character", radical.Id);
            return View(radical);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var radical = await _context.KanjiRadicals.FindAsync(id);
            if (radical == null)
            {
                return NotFound();
            }

            ViewBag.Kanjis = new SelectList(_context.Kanjis, "Id", "Character", radical.Id);
            return View(radical);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RadicalKanji,RadicalHanjaMeaning,KanjiId")] KanjiRadical radical)
        {
            if (id != radical.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(radical);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Bộ thủ đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.KanjiRadicals.Any(r => r.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            ViewBag.Kanjis = new SelectList(_context.Kanjis, "Id", "Character", radical.Id);
            return View(radical);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var radical = await _context.KanjiRadicals
                .FirstOrDefaultAsync(r => r.Id == id);

            if (radical == null)
            {
                return NotFound();
            }

            return View(radical);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var radical = await _context.KanjiRadicals.FindAsync(id);
            if (radical == null)
            {
                return NotFound();
            }

            _context.KanjiRadicals.Remove(radical);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Bộ thủ đã được xóa thành công.";
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var radical = await _context.KanjiRadicals
                .Include(r => r.KanjiKanjiRadicals)
                    .ThenInclude(kr => kr.Kanji)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (radical == null)
            {
                return NotFound();
            }

            return View(radical);
        }

    }
}
