

using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace JapaneseLearningWeb.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách danh mục với phân trang
        [AllowAnonymous]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string keyword = "")
        {
            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c => c.Name.Contains(keyword) || c.Description.Contains(keyword));
            }

            var totalItems = await query.CountAsync();
            var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            var model = new PagedListViewModel<Category>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Keyword = keyword
            };

            return View(model);
        }

        // Hiển thị chi tiết danh mục
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Hiển thị form tạo mới danh mục
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new Category());
        }

        // Xử lý tạo mới danh mục
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("CategoryId,Name,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                category.CreatedAt = DateTime.Now;
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // Hiển thị form chỉnh sửa danh mục
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Xử lý chỉnh sửa danh mục
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name,Description,CreatedAt")] Category category)
        {
            if (id != category.CategoryId)
            {
                return BadRequest("ID trong URL không khớp với ID trong model.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCategory = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == id);
                    if (existingCategory == null)
                    {
                        return NotFound();
                    }

                    _context.Entry(category).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.CategoryId == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(category);
        }

        // Hiển thị form xác nhận xóa danh mục
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Xử lý xóa danh mục
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Tìm kiếm danh mục
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction(nameof(Index));
            }

            var results = await _context.Categories
            .Where(c => c.Name.Contains(keyword) || c.Description.Contains(keyword))
            .ToListAsync();

            var model = new PagedListViewModel<Category>
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