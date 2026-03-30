using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace JapaneseLearningWeb.Controllers
{
    public class CoursesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;


        public CoursesController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        // GET: Courses (mọi user truy cập được)
        //public async Task<IActionResult> Index()
        //{
        //    var courses = await _context.Courses
        //        .Include(c => c.Category)
        //        .ToListAsync();
        //    return View(courses);
        //}

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Nếu là Admin hoặc Employee thì xem tất cả khóa học
            if (User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                var allCourses = await _context.Courses
                    .Include(c => c.Category)
                    .ToListAsync();
                return View(allCourses);
            }

            // Nếu là người dùng bình thường
            var userCourseIds = await _context.UserCourses
                .Where(uc => uc.UserId == user.Id)
                .Select(uc => uc.CourseId)
                .ToListAsync();

            var courses = await _context.Courses
                .Include(c => c.Category)
                .Where(c => c.IsFree || userCourseIds.Contains(c.CourseId))
                .ToListAsync();

            return View(courses);
        }


        // GET: Courses/Details/5 (mọi user truy cập được)
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var course = await _context.Courses
        //        .Include(c => c.Category)
        //        .Include(c => c.Lessons)
        //        .FirstOrDefaultAsync(m => m.CourseId == id);

        //    if (course == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(course);
        //}

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(m => m.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Chỉ Admin/Employee hoặc người dùng đã đăng ký mới được xem
            if (!course.IsFree && !(User.IsInRole("Admin") || User.IsInRole("Employee")))
            {
                var hasAccess = await _context.UserCourses
                    .AnyAsync(uc => uc.UserId == user.Id && uc.CourseId == course.CourseId);

                if (!hasAccess)
                {
                    TempData["Error"] = "Bạn cần đăng ký khóa học để xem nội dung.";
                    return RedirectToAction("Index");
                }
            }

            return View(course);
        }


        // GET: Courses/Create
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories
                .Select(c => new { c.CategoryId, c.Name })
                .ToList();

            return View(new Course());
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create([Bind("Title,Description,IsFree,Price,CategoryId")] Course course)
        {
            if (course.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Vui lòng chọn một danh mục.");
            }

            if (ModelState.IsValid)
            {
                course.CreatedAt = DateTime.Now;

                _context.Add(course);
                try
                {
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Khóa học đã được tạo thành công." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "Lỗi khi lưu vào database: " + ex.Message } });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToList();
            return Json(new { success = false, errors = errors });
        }

        // GET: Courses/Edit/5
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.Categories
                .Select(c => new { c.CategoryId, c.Name })
                .ToList();
            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,Title,Description,IsFree,Price,CategoryId")] Course course)
        {
            if (id != course.CourseId)
            {
                return NotFound();
            }

            if (course.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Vui lòng chọn một danh mục.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    course.CreatedAt = (await _context.Courses.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.CourseId == id))?.CreatedAt ?? DateTime.Now;
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseId))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            ViewBag.Categories = _context.Categories
                .Select(c => new { c.CategoryId, c.Name })
                .ToList();
            return View(course);
        }

        // GET: Courses/Delete/5
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
        // GET: Courses/Details/5 - Hiển thị khóa học và bài học của nó
        
    }
}
