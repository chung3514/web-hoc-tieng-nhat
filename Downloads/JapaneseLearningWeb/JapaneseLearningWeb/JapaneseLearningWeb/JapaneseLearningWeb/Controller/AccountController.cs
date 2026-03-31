//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using JapaneseLearningWeb.Data;
//using JapaneseLearningWeb.Models;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System;
//using Microsoft.AspNetCore.Identity.UI.Services;

//namespace JapaneseLearningWeb.Controllers
//{
//    public class AccountController : Controller
//    {
//        private readonly UserManager<IdentityUser> _userManager;
//        private readonly AppDbContext _context;
//        private readonly IEmailSender _emailSender;
//        public AccountController(UserManager<IdentityUser> userManager, AppDbContext context, IEmailSender emailSender)
//        {
//            _userManager = userManager;
//            _context = context;
//            _emailSender = emailSender;
//        }

//        [Authorize]
//        public async Task<IActionResult> Profile()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null) return RedirectToAction("Login");

//            var roles = await _userManager.GetRolesAsync(user);

//            var enrolledCourses = _context.UserCourses
//                .Where(uc => uc.UserId == user.Id)
//                .Select(uc => uc.Course.Title)
//                .ToList();

//            var model = new UserProfileViewModel
//            {
//                UserName = user.UserName,
//                Email = user.Email,
//                Roles = roles,
//                EnrolledCourses = enrolledCourses
//            };

//            return View(model);
//        }
//        // GET: /Account/ForgotPassword
//        [HttpGet]
//        public IActionResult ForgotPassword()
//        {
//            return View();
//        }

//        // POST: /Account/ForgotPassword
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ForgotPassword(string email)
//        {
//            if (string.IsNullOrEmpty(email))
//            {
//                TempData["Error"] = "Vui lòng nhập email.";
//                return View();
//            }

//            var user = await _userManager.FindByEmailAsync(email);
//            if (user == null)
//            {
//                // Không tiết lộ user không tồn tại
//                TempData["Message"] = "Nếu email hợp lệ, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu.";
//                return View();
//            }

//            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
//            var link = Url.Action("ResetPassword",
//                                   "Account",
//                                   new { email = user.Email, token },
//                                   Request.Scheme);

//            var html = $"Nhấn <a href='{link}'>vào đây</a> để đặt lại mật khẩu.";
//            await _emailSender.SendEmailAsync(email, "Đặt lại mật khẩu", html);

//            TempData["Message"] = "Hướng dẫn đặt lại mật khẩu đã được gửi.";
//            return View();
//        }

//        // GET: /Account/ResetPassword?email=…&token=…
//        [HttpGet]
//        public IActionResult ResetPassword(string email, string token)
//        {
//            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
//            {
//                TempData["Error"] = "Liên kết không hợp lệ.";
//                return RedirectToAction("Login", "Home");
//            }

//            var vm = new ResetPasswordViewModel
//            {
//                Email = email,
//                Token = token
//            };
//            return View(vm);
//        }

//        // POST: /Account/ResetPassword
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
//        {
//            if (!ModelState.IsValid)
//                return View(model);

//            var user = await _userManager.FindByEmailAsync(model.Email);
//            if (user == null)
//            {
//                ModelState.AddModelError("", "Không tìm thấy người dùng.");
//                return View(model);
//            }

//            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
//            if (result.Succeeded)
//            {
//                TempData["Message"] = "Mật khẩu đã được thay đổi thành công!";
//                return RedirectToAction("Login", "Home");
//            }

//            foreach (var err in result.Errors)
//                ModelState.AddModelError("", err.Description);

//            return View(model);
//        }

//        [Authorize]
//        [HttpGet]
//        public async Task<IActionResult> RegisterCourse()
//        {
//            //List<int> selectedCourseIds
//            //if (selectedCourseIds == null || !selectedCourseIds.Any())
//            //{
//            //    TempData["Error"] = "Vui lòng chọn ít nhất một khóa học để thanh toán.";
//            //    return RedirectToAction("RegisterCourse");
//            //}

//            var user = await _userManager.GetUserAsync(User);
//            if (user == null) return RedirectToAction("Login");

//            var enrolledCourseIds = _context.UserCourses
//                .Where(e => e.UserId == user.Id)
//                .Select(e => e.CourseId)
//                .ToList();

//            var availableCourses = _context.Courses
//                .Where(c => !enrolledCourseIds.Contains(c.CourseId))
//                .ToList();

//            ViewBag.Courses = availableCourses;
//            return View();
//        }

//        [Authorize]
//        [HttpPost]
//        public async Task<IActionResult> RegisterFreeCourse(int courseId)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null) return RedirectToAction("Login");

//            bool exists = _context.UserCourses
//                .Any(e => e.UserId == user.Id && e.CourseId == courseId);

//            if (!exists)
//            {
//                _context.UserCourses.Add(new UserCourse
//                {
//                    UserId = user.Id,
//                    CourseId = courseId
//                });
//                await _context.SaveChangesAsync();
//                TempData["Message"] = "Bạn đã đăng ký khóa học miễn phí.";
//            }
//            else
//            {
//                TempData["Message"] = "Bạn đã đăng ký khóa học này rồi.";
//            }

//            return RedirectToAction("RegisterCourse");
//        }

//        [Authorize]
//        [HttpPost]
//        public async Task<IActionResult> Payment(List<int> selectedCourseIds)
//        {

//            var user = await _userManager.GetUserAsync(User);
//            if (user == null) return RedirectToAction("Login");

//            var enrolledCourseIds = _context.UserCourses
//                .Where(e => e.UserId == user.Id)
//                .Select(e => e.CourseId)
//                .ToList();

//            var selectedCourses = _context.Courses
//                .Where(c => selectedCourseIds.Contains(c.CourseId)
//                            && !c.IsFree
//                            && !enrolledCourseIds.Contains(c.CourseId))
//                .ToList();

//            //if (!selectedCourses.Any())
//            //{
//            //    TempData["Error"] = "Không có khóa học hợp lệ để thanh toán.";
//            //    return RedirectToAction("RegisterCourse");
//            //}

//            var total = selectedCourses.Sum(c => c.Price);

//            ViewBag.SelectedCourses = selectedCourses;
//            ViewBag.Total = total;

//            return View("Payment");
//        }

//        [HttpPost]
//        [IgnoreAntiforgeryToken] // Chấp nhận gọi từ JS nếu không truyền token
//        public IActionResult ConfirmPaidCourses([FromBody] ConfirmCourseRequest model)
//        {
//            if (string.IsNullOrEmpty(model.UserId) || model.PaidCourseIds == null || !model.PaidCourseIds.Any())
//            {
//                return BadRequest("Thiếu thông tin.");
//            }

//            var user = _context.Users.FirstOrDefault(u => u.Id == model.UserId);
//            if (user == null)
//            {
//                return NotFound("Không tìm thấy người dùng.");
//            }

//            foreach (var courseId in model.PaidCourseIds)
//            {
//                if (!_context.UserCourses.Any(uc => uc.UserId == user.Id && uc.CourseId == courseId))
//                {
//                    _context.UserCourses.Add(new UserCourse
//                    {
//                        UserId = user.Id,
//                        CourseId = courseId
//                    });
//                }
//            }

//            _context.SaveChanges();
//            return Ok("Đã xác nhận khóa học.");
//        }

//        // Model để nhận từ body
//        public class ConfirmCourseRequest
//        {
//            public string UserId { get; set; }
//            public List<int> PaidCourseIds { get; set; }
//        }
//        [Authorize]
//        public async Task<IActionResult> CheckUserIdStorage()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            ViewBag.UserId = user?.Id;
//            return View();
//        }
//        public IActionResult LocalStorageDebug()
//        {
//            return View();
//        }

//        [Authorize]
//        public async Task<IActionResult> MyCourses()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null) return RedirectToAction("Login");

//            // Khóa học miễn phí: luôn hiển thị
//            var freeCourses = _context.Courses.Where(c => c.IsFree).ToList();

//            // Khóa học đã mua: lấy từ UserCourses
//            var paidCourseIds = _context.UserCourses
//                .Where(e => e.UserId == user.Id)
//                .Select(e => e.CourseId)
//                .ToList();

//            var paidCourses = _context.Courses
//                .Where(c => paidCourseIds.Contains(c.CourseId))
//                .ToList();

//            // Gộp vào list truyền ra View
//            var myCourses = freeCourses.Concat(paidCourses).ToList();

//            return View(myCourses);
//        }

//        public async Task<IActionResult> LoginSuccess()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            ViewBag.UserId = user?.Id; // gửi userId ra view
//            return View();
//        }

//        [Authorize]
//        public async Task<IActionResult> AccessCourse(int courseId)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null) return RedirectToAction("Login");

//            var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
//            if (course == null)
//            {
//                return NotFound("Khóa học không tồn tại.");
//            }

//            // Nếu là khóa học miễn phí thì cho phép truy cập luôn
//            if (course.IsFree)
//            {
//                return RedirectToAction("LessonList", "Lesson", new { courseId = course.CourseId });
//            }

//            // Nếu là khóa học trả phí, kiểm tra đã thanh toán chưa
//            var isEnrolled = _context.UserCourses
//                .Any(uc => uc.UserId == user.Id && uc.CourseId == courseId);

//            if (!isEnrolled)
//            {
//                TempData["Error"] = "Bạn cần đăng ký/đã thanh toán để truy cập khóa học này.";
//                return RedirectToAction("RegisterCourse");
//            }

//            // Đã thanh toán -> cho truy cập bài học
//            return RedirectToAction("LessonList", "Lesson", new { courseId = course.CourseId });
//        }


//        //[Authorize]
//        //public async Task<IActionResult> History()
//        //{
//        //    var user = await _userManager.GetUserAsync(User);
//        //    if (user == null) return RedirectToAction("Login");

//        //    var history = _context.UserCourseEnrollments
//        //        .Where(e => e.UserId == user.Id && !e.Course.IsFree)
//        //        .Select(e => new PurchaseHistoryViewModel
//        //        {
//        //            CourseTitle = e.Course.Title,
//        //            EnrolledAt = e.EnrolledAt,
//        //            Price = e.Course.Price
//        //        })
//        //        .ToList();

//        //    return View(history);
//        //}

//    }

//}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace JapaneseLearningWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<IdentityUser> userManager, AppDbContext context, IEmailSender emailSender)
        {
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
        }

        // ===================== PROFILE =====================
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var roles = await _userManager.GetRolesAsync(user);

            var enrolledCourses = _context.UserCourses
                .Where(uc => uc.UserId == user.Id)
                .Select(uc => uc.Course.Title)
                .ToList();

            var model = new UserProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles,
                EnrolledCourses = enrolledCourses
            };

            return View(model);
        }

        // ===================== FORGOT PASSWORD =====================
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Vui lòng nhập email.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Message"] = "Nếu email hợp lệ, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu.";
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("ResetPassword",
                                   "Account",
                                   new { email = user.Email, token },
                                   Request.Scheme);

            var html = $"Nhấn <a href='{link}'>vào đây</a> để đặt lại mật khẩu.";
            await _emailSender.SendEmailAsync(email, "Đặt lại mật khẩu", html);

            TempData["Message"] = "Hướng dẫn đặt lại mật khẩu đã được gửi.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Liên kết không hợp lệ.";
                return RedirectToAction("Login", "Home");
            }

            var vm = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Không tìm thấy người dùng.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["Message"] = "Mật khẩu đã được thay đổi thành công!";
                return RedirectToAction("Login", "Home");
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);

            return View(model);
        }

        // ===================== REGISTER COURSE =====================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RegisterCourse()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var enrolledCourseIds = _context.UserCourses
                .Where(e => e.UserId == user.Id)
                .Select(e => e.CourseId)
                .ToList();

            var availableCourses = _context.Courses
                .Where(c => !enrolledCourseIds.Contains(c.CourseId))
                .ToList();

            ViewBag.Courses = availableCourses;

            // ✅ LẤY VOUCHER CỦA USER (chưa dùng + chưa hết hạn)
            var now = DateTime.UtcNow;

            var userVouchers = await _context.UserVouchers
                .Include(uv => uv.Voucher)
                .Where(uv =>
                    uv.UserId == user.Id &&
                    !uv.IsUsed &&
                    uv.Voucher != null &&
                    (uv.Voucher.ExpireAt == null || uv.Voucher.ExpireAt > now))
                .Select(uv => new VoucherPickVm
                {
                    Code = uv.Voucher!.Code,
                    DiscountAmount = uv.Voucher!.DiscountAmount,
                    OnlyPaidCourse = uv.Voucher!.OnlyPaidCourse,
                    ExpireAt = uv.Voucher!.ExpireAt
                })
                .ToListAsync();

            ViewBag.UserVouchers = userVouchers;

            return View();
        }

        // VM để View hiển thị danh sách voucher
        public class VoucherPickVm
        {
            public string Code { get; set; } = "";
            public decimal DiscountAmount { get; set; }
            public bool OnlyPaidCourse { get; set; }
            public DateTime? ExpireAt { get; set; }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RegisterFreeCourse(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            bool exists = _context.UserCourses
                .Any(e => e.UserId == user.Id && e.CourseId == courseId);

            if (!exists)
            {
                _context.UserCourses.Add(new UserCourse
                {
                    UserId = user.Id,
                    CourseId = courseId
                });
                await _context.SaveChangesAsync();
                TempData["Message"] = "Bạn đã đăng ký khóa học miễn phí.";
            }
            else
            {
                TempData["Message"] = "Bạn đã đăng ký khóa học này rồi.";
            }

            return RedirectToAction("RegisterCourse");
        }

        // ===================== PAYMENT (APPLY VOUCHER) =====================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(List<int> selectedCourseIds, string? voucherCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var enrolledCourseIds = _context.UserCourses
                .Where(e => e.UserId == user.Id)
                .Select(e => e.CourseId)
                .ToList();

            var selectedCourses = _context.Courses
                .Where(c => selectedCourseIds.Contains(c.CourseId)
                            && !c.IsFree
                            && !enrolledCourseIds.Contains(c.CourseId))
                .ToList();

            if (!selectedCourses.Any())
            {
                TempData["Error"] = "Không có khóa học hợp lệ để thanh toán.";
                return RedirectToAction("RegisterCourse");
            }

            var total = selectedCourses.Sum(c => c.Price);

            // ✅ kiểm tra voucher thuộc user + chưa dùng + chưa hết hạn
            decimal discount = 0m;
            string? appliedVoucherCode = null;

            if (!string.IsNullOrWhiteSpace(voucherCode))
            {
                var now = DateTime.UtcNow;

                var uv = await _context.UserVouchers
                    .Include(x => x.Voucher)
                    .FirstOrDefaultAsync(x =>
                        x.UserId == user.Id &&
                        !x.IsUsed &&
                        x.Voucher != null &&
                        x.Voucher.Code == voucherCode &&
                        (x.Voucher.ExpireAt == null || x.Voucher.ExpireAt > now));

                if (uv?.Voucher != null)
                {
                    // voucher tiền: giảm trực tiếp trên total (khóa trả phí)
                    discount = uv.Voucher.DiscountAmount;
                    if (discount > total) discount = total;

                    appliedVoucherCode = uv.Voucher.Code;
                }
            }

            var finalTotal = total - discount;

            ViewBag.SelectedCourses = selectedCourses;
            ViewBag.Total = total;
            ViewBag.Discount = discount;
            ViewBag.FinalTotal = finalTotal;
            ViewBag.VoucherCode = appliedVoucherCode; // có thể null nếu không chọn/không hợp lệ

            return View("Payment");
        }

        // ===================== CONFIRM PAID COURSES + MARK VOUCHER USED =====================
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ConfirmPaidCourses([FromBody] ConfirmCourseRequest model)
        {
            if (string.IsNullOrEmpty(model.UserId) || model.PaidCourseIds == null || !model.PaidCourseIds.Any())
            {
                return BadRequest("Thiếu thông tin.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            foreach (var courseId in model.PaidCourseIds)
            {
                if (!_context.UserCourses.Any(uc => uc.UserId == user.Id && uc.CourseId == courseId))
                {
                    _context.UserCourses.Add(new UserCourse
                    {
                        UserId = user.Id,
                        CourseId = courseId
                    });
                }
            }

            // ✅ nếu có voucherCode => đánh dấu đã dùng (1 lần)
            if (!string.IsNullOrWhiteSpace(model.VoucherCode))
            {
                var now = DateTime.UtcNow;
                var uv = await _context.UserVouchers
                    .Include(x => x.Voucher)
                    .FirstOrDefaultAsync(x =>
                        x.UserId == user.Id &&
                        !x.IsUsed &&
                        x.Voucher != null &&
                        x.Voucher.Code == model.VoucherCode &&
                        (x.Voucher.ExpireAt == null || x.Voucher.ExpireAt > now));

                if (uv != null)
                {
                    uv.IsUsed = true;
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Đã xác nhận khóa học.");
        }

        public class ConfirmCourseRequest
        {
            public string UserId { get; set; } = "";
            public List<int> PaidCourseIds { get; set; } = new();

            // ✅ thêm voucherCode để confirm xong thì mark used
            public string? VoucherCode { get; set; }
        }

        // ===================== OTHERS =====================
        [Authorize]
        public async Task<IActionResult> CheckUserIdStorage()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserId = user?.Id;
            return View();
        }

        public IActionResult LocalStorageDebug()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> MyCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var freeCourses = _context.Courses.Where(c => c.IsFree).ToList();

            var paidCourseIds = _context.UserCourses
                .Where(e => e.UserId == user.Id)
                .Select(e => e.CourseId)
                .ToList();

            var paidCourses = _context.Courses
                .Where(c => paidCourseIds.Contains(c.CourseId))
                .ToList();

            var myCourses = freeCourses.Concat(paidCourses).ToList();
            return View(myCourses);
        }

        public async Task<IActionResult> LoginSuccess()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserId = user?.Id;
            return View();
        }

        [Authorize]
        public async Task<IActionResult> AccessCourse(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
            if (course == null) return NotFound("Khóa học không tồn tại.");

            if (course.IsFree)
            {
                return RedirectToAction("LessonList", "Lesson", new { courseId = course.CourseId });
            }

            var isEnrolled = _context.UserCourses
                .Any(uc => uc.UserId == user.Id && uc.CourseId == courseId);

            if (!isEnrolled)
            {
                TempData["Error"] = "Bạn cần đăng ký/đã thanh toán để truy cập khóa học này.";
                return RedirectToAction("RegisterCourse");
            }

            return RedirectToAction("LessonList", "Lesson", new { courseId = course.CourseId });
        }
    }
}
