
//using JapaneseLearningWeb.Data;
//using JapaneseLearningWeb.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;

//namespace JapaneseLearningWeb.Controllers
//{
//    [Authorize]
//    public class UserCourseEnrollmentsController : Controller
//    {
//        private readonly AppDbContext _context;

//        public UserCourseEnrollmentsController(AppDbContext context)
//        {
//            _context = context;
//        }

//        // Hiển thị danh sách khóa học đã đăng ký
//        [Authorize(Roles = "User,Admin,Employee")]
//        public async Task<IActionResult> Index()
//        {
//            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            var enrollments = await _context.UserCourseEnrollments
//                .Include(e => e.Course)
//                .ThenInclude(c => c.Category)
//                .Include(e => e.Course)
//                .ThenInclude(c => c.Lessons)
//                .Include(e => e.Progresses)
//                .ThenInclude(p => p.Lesson)
//                .Where(e => e.UserId == userId)
//                .ToListAsync();

//            return View(enrollments);
//        }

//        // Đăng ký khóa học
//        [Authorize(Roles = "User,Admin,Employee")]
//        [HttpGet]
//        public async Task<IActionResult> Enroll(int courseId)
//        {
//            var course = await _context.Courses
//                .Include(c => c.Category)
//                .FirstOrDefaultAsync(c => c.CourseId == courseId);
//            if (course == null)
//            {
//                return NotFound();
//            }

//            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            var existingEnrollment = await _context.UserCourseEnrollments
//                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

//            if (existingEnrollment != null)
//            {
//                TempData["Error"] = "Bạn đã đăng ký khóa học này rồi.";
//                return RedirectToAction(nameof(Index));
//            }

//            if (!course.IsFree && course.Price > 0)
//            {
//                TempData["Error"] = "Khóa học này yêu cầu thanh toán. Vui lòng hoàn tất thanh toán trước khi đăng ký.";
//                return RedirectToAction(nameof(Index));
//            }

//            var enrollment = new UserCourseEnrollments
//            {
//                UserId = userId,
//                CourseId = courseId,
//                Status = "Enrolled",
//                EnrolledAt = DateTime.Now
//            };

//            _context.UserCourseEnrollments.Add(enrollment);
//            await _context.SaveChangesAsync();
//            TempData["Success"] = "Đăng ký khóa học thành công!";
//            return RedirectToAction(nameof(Index));
//        }

//        // Bắt đầu một bài học
//        [Authorize(Roles = "User,Admin,Employee")]
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> StartLesson(int enrollmentId, int lessonId)
//        {
//            var enrollment = await _context.UserCourseEnrollments
//                .Include(e => e.Course)
//                .ThenInclude(c => c.Lessons)
//                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

//            if (enrollment == null)
//            {
//                return NotFound();
//            }

//            var lesson = enrollment.Course.Lessons.FirstOrDefault(l => l.LessonId == lessonId);
//            if (lesson == null)
//            {
//                return NotFound();
//            }

//            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            var progress = await _context.UserProgresses
//                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

//            if (progress == null)
//            {
//                progress = new UserProgress
//                {
//                    UserId = userId,
//                    LessonId = lessonId,
//                    Status = "InProgress",
//                    CompletedAt = null
//                };
//                _context.UserProgresses.Add(progress);
//            }
//            else if (progress.Status != "Completed")
//            {
//                progress.Status = "InProgress";
//                progress.CompletedAt = null;
//            }
//            else
//            {
//                TempData["Error"] = "Bài học này đã được hoàn thành.";
//                return RedirectToAction(nameof(Progress), new { enrollmentId });
//            }

//            if (enrollment.Status == "Enrolled")
//            {
//                enrollment.Status = "InProgress";
//            }

//            await _context.SaveChangesAsync();
//            TempData["Success"] = "Bắt đầu bài học thành công!";
//            return RedirectToAction(nameof(Progress), new { enrollmentId });
//        }

//        // Hoàn thành một bài học
//        [Authorize(Roles = "User,Admin,Employee")]
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> CompleteLesson(int enrollmentId, int lessonId)
//        {
//            var enrollment = await _context.UserCourseEnrollments
//                .Include(e => e.Course)
//                .ThenInclude(c => c.Lessons)
//                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

//            if (enrollment == null)
//            {
//                return NotFound();
//            }

//            var lesson = enrollment.Course.Lessons.FirstOrDefault(l => l.LessonId == lessonId);
//            if (lesson == null)
//            {
//                return NotFound();
//            }

//            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            var progress = await _context.UserProgresses
//                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

//            if (progress == null)
//            {
//                progress = new UserProgress
//                {
//                    UserId = userId,
//                    LessonId = lessonId,
//                    Status = "Completed",
//                    CompletedAt = DateTime.Now
//                };
//                _context.UserProgresses.Add(progress);
//            }
//            else if (progress.Status != "Completed")
//            {
//                progress.Status = "Completed";
//                progress.CompletedAt = DateTime.Now;
//            }
//            else
//            {
//                TempData["Error"] = "Bài học này đã được hoàn thành.";
//                return RedirectToAction(nameof(Progress), new { enrollmentId });
//            }

//            var totalLessons = enrollment.Course.Lessons.Count;
//            var completedLessons = await _context.UserProgresses
//                .CountAsync(p => p.UserId == userId && enrollment.Course.Lessons.Select(l => l.LessonId).Contains(p.LessonId) && p.Status == "Completed");

//            if (completedLessons >= totalLessons)
//            {
//                enrollment.Status = "Completed";
//                enrollment.CompletedAt = DateTime.Now;
//            }
//            else if (enrollment.Status == "Enrolled")
//            {
//                enrollment.Status = "InProgress";
//            }

//            await _context.SaveChangesAsync();
//            TempData["Success"] = "Hoàn thành bài học thành công!";
//            return RedirectToAction(nameof(Progress), new { enrollmentId });
//        }

//        // Hiển thị tiến trình học
//        [Authorize(Roles = "User,Admin,Employee")]
//        [HttpGet]
//        public async Task<IActionResult> Progress(int enrollmentId)
//        {
//            var enrollment = await _context.UserCourseEnrollments
//                .Include(e => e.Course)
//                .ThenInclude(c => c.Lessons)
//                .Include(e => e.Course)
//                .ThenInclude(c => c.Category)
//                .Include(e => e.Progresses)
//                .ThenInclude(p => p.Lesson)
//                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

//            if (enrollment == null)
//            {
//                return NotFound();
//            }

//            return View(enrollment);
//        }

//        // Xóa bản ghi đăng ký (chỉ Admin)
//        [Authorize(Roles = "Admin")]
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var enrollment = await _context.UserCourseEnrollments
//                .Include(e => e.Course)
//                .ThenInclude(c => c.Lessons)
//                .FirstOrDefaultAsync(e => e.Id == id);

//            if (enrollment == null)
//            {
//                return NotFound();
//            }

//            var progresses = await _context.UserProgresses
//                .Where(p => p.UserId == enrollment.UserId && enrollment.Course.Lessons.Select(l => l.LessonId).Contains(p.LessonId))
//                .ToListAsync();
//            _context.UserProgresses.RemoveRange(progresses);

//            _context.UserCourseEnrollments.Remove(enrollment);
//            await _context.SaveChangesAsync();
//            TempData["Success"] = "Xóa đăng ký khóa học thành công!";
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JapaneseLearningWeb.Controllers
{
    [Authorize]
    public class UserCourseEnrollmentsController : Controller
    {
        private readonly AppDbContext _context;

        public UserCourseEnrollmentsController(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Hiển thị danh sách khóa học đã đăng ký
        [Authorize(Roles = "User,Admin,Employee")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng.");
            }

            var enrollments = await _context.UserCourseEnrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Category)
                .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
                .Include(e => e.Progresses)
                .ThenInclude(p => p.Lesson)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            return View(enrollments);
        }

        // Đăng ký khóa học
        [Authorize(Roles = "User,Admin,Employee")]
        [HttpGet]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng.");
            }

            var existingEnrollment = await _context.UserCourseEnrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (existingEnrollment != null)
            {
                TempData["Error"] = "Bạn đã đăng ký khóa học này rồi.";
                return RedirectToAction(nameof(Index));
            }

            if (!course.IsFree && course.Price > 0)
            {
                TempData["Error"] = "Khóa học này yêu cầu thanh toán. Vui lòng hoàn tất thanh toán trước khi đăng ký.";
                return RedirectToAction(nameof(Index));
            }

            var enrollment = new UserCourseEnrollments
            {
                UserId = userId,
                CourseId = courseId,
                Status = "Enrolled",
                EnrolledAt = DateTime.Now
            };

            _context.UserCourseEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đăng ký khóa học thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Bắt đầu một bài học
        [Authorize(Roles = "User,Admin,Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartLesson(int enrollmentId, int lessonId)
        {
            var enrollment = await _context.UserCourseEnrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
            {
                return NotFound();
            }

            var lesson = enrollment.Course.Lessons.FirstOrDefault(l => l.LessonId == lessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng.");
            }

            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

            if (progress == null)
            {
                progress = new UserProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    Status = "InProgress",
                    CompletedAt = null
                };
                _context.UserProgresses.Add(progress);
            }
            else if (progress.Status != "Completed")
            {
                progress.Status = "InProgress";
                progress.CompletedAt = null;
            }
            else
            {
                TempData["Error"] = "Bài học này đã được hoàn thành.";
                return RedirectToAction(nameof(Progress), new { enrollmentId });
            }

            if (enrollment.Status == "Enrolled")
            {
                enrollment.Status = "InProgress";
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Bắt đầu bài học thành công!";
            return RedirectToAction(nameof(Progress), new { enrollmentId });
        }

        // Hoàn thành một bài học
        [Authorize(Roles = "User,Admin,Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteLesson(int enrollmentId, int lessonId)
        {
            var enrollment = await _context.UserCourseEnrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
            {
                return NotFound();
            }

            var lesson = enrollment.Course.Lessons.FirstOrDefault(l => l.LessonId == lessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng.");
            }

            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

            if (progress == null)
            {
                progress = new UserProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    Status = "Completed",
                    CompletedAt = DateTime.Now
                };
                _context.UserProgresses.Add(progress);
            }
            else if (progress.Status != "Completed")
            {
                progress.Status = "Completed";
                progress.CompletedAt = DateTime.Now;
            }
            else
            {
                TempData["Error"] = "Bài học này đã được hoàn thành.";
                return RedirectToAction(nameof(Progress), new { enrollmentId });
            }

            var totalLessons = enrollment.Course.Lessons.Count;
            var completedLessons = await _context.UserProgresses
                .CountAsync(p => p.UserId == userId && enrollment.Course.Lessons.Select(l => l.LessonId).Contains(p.LessonId) && p.Status == "Completed");

            if (completedLessons >= totalLessons)
            {
                enrollment.Status = "Completed";
                enrollment.CompletedAt = DateTime.Now;
            }
            else if (enrollment.Status == "Enrolled")
            {
                enrollment.Status = "InProgress";
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Hoàn thành bài học thành công!";
            return RedirectToAction(nameof(Progress), new { enrollmentId });
        }

        // Hiển thị tiến trình học
        [Authorize(Roles = "User,Admin,Employee")]
        [HttpGet]
        public async Task<IActionResult> Progress(int enrollmentId)
        {
            var enrollment = await _context.UserCourseEnrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
                .Include(e => e.Course)
                .ThenInclude(c => c.Category)
                .Include(e => e.Progresses)
                .ThenInclude(p => p.Lesson)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // Xóa bản ghi đăng ký (chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var enrollment = await _context.UserCourseEnrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment == null)
            {
                return NotFound();
            }

            var progresses = await _context.UserProgresses
                .Where(p => p.UserId == enrollment.UserId && enrollment.Course.Lessons.Select(l => l.LessonId).Contains(p.LessonId))
                .ToListAsync();
            _context.UserProgresses.RemoveRange(progresses);

            _context.UserCourseEnrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa đăng ký khóa học thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}