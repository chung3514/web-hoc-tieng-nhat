//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using JapaneseLearningWeb.Models;
//using System.Diagnostics;
//using JapaneseLearningWeb.Data;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Authorization;

//namespace JapaneseLearningWeb.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly UserManager<IdentityUser> _userManager;
//        private readonly SignInManager<IdentityUser> _signInManager;
//        private readonly AppDbContext _context;

//        public HomeController(UserManager<IdentityUser> userManager,
//                              SignInManager<IdentityUser> signInManager,
//                              AppDbContext context)
//        {
//            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
//            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
//            _context = context ?? throw new ArgumentNullException(nameof(context));
//        }

//        public IActionResult Index()
//        {
//            return View();
//        }

//        // GET: /Home/Login
//        [HttpGet]
//        public IActionResult Login()
//        {
//            return View();
//        }

//        // POST: /Home/Login
//        [HttpPost]
//        public async Task<IActionResult> Login(string email, string password)
//        {
//            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
//            {
//                ViewBag.Error = "Email và mật khẩu không được để trống.";
//                return View();
//            }

//            var user = await _userManager.FindByEmailAsync(email);
//            if (user != null)
//            {
//                var result = await _signInManager.PasswordSignInAsync(user.UserName ?? email, password, false, lockoutOnFailure: false);
//                if (result.Succeeded)
//                {
//                    return RedirectToAction("Index", "Home");
//                }
//            }

//            ViewBag.Error = "Email hoặc mật khẩu không đúng.";
//            return View();
//        }

//        // GET: /Home/ForgotPassword
//        [HttpGet]
//        public IActionResult ForgotPassword()
//        {
//            return View();
//        }

//        // POST: /Home/ForgotPassword
//        [HttpPost]
//        public async Task<IActionResult> ForgotPassword(string email)
//        {
//            if (string.IsNullOrEmpty(email))
//            {
//                ViewBag.Error = "Vui lòng nhập email.";
//                return View();
//            }

//            var user = await _userManager.FindByEmailAsync(email);
//            if (user == null)
//            {
//                ViewBag.Message = "Nếu email hợp lệ, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu.";
//                return View();
//            }

//            // TODO: Tạo token reset mật khẩu và gửi email
//            // Ví dụ: tạo token
//            // var token = await _userManager.GeneratePasswordResetTokenAsync(user);
//            // Tạo link reset rồi gọi service gửi email
//            // await _emailSender.SendEmailAsync(email, "Đặt lại mật khẩu", ...);

//            ViewBag.Message = "Hướng dẫn đặt lại mật khẩu đã được gửi vào email của bạn.";
//            return View();
//        }

//        public IActionResult Register()
//        {
//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> Register(string email, string username, string password)
//        {
//            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
//            {
//                ViewBag.Error = "Email, username và mật khẩu không được để trống.";
//                return View();
//            }

//            var existingUser = await _userManager.FindByEmailAsync(email);
//            if (existingUser != null)
//            {
//                ViewBag.Error = "Email đã được sử dụng.";
//                return View();
//            }

//            var user = new IdentityUser
//            {
//                Email = email,
//                UserName = username
//            };

//            var result = await _userManager.CreateAsync(user, password);
//            if (result.Succeeded)
//            {
//                return RedirectToAction("Login");
//            }

//            ViewBag.Error = string.Join("<br>", result.Errors.Select(e => e.Description));
//            return View();
//        }

//        public IActionResult Vocabulary()
//        {
//            var vocabularies = _context.Vocabularies.ToList();
//            return View(vocabularies);
//        }

//        public IActionResult Quiz()
//        {
//            var userId = HttpContext.Session.GetString("UserId");
//            if (string.IsNullOrEmpty(userId))
//            {
//                return RedirectToAction("Login");
//            }

//            var randomVocab = _context.Vocabularies.OrderBy(x => Guid.NewGuid()).Take(3).ToList();
//            return View(randomVocab);
//        }

//        [HttpPost]
//        public IActionResult SubmitQuiz(int[] answers)
//        {
//            var userId = HttpContext.Session.GetString("UserId");
//            if (string.IsNullOrEmpty(userId))
//            {
//                return RedirectToAction("Login");
//            }

//            int score = answers?.Sum() ?? 0;
//            var result = new QuizResult
//            {
//                Score = score,
//                DateTaken = DateTime.Now,
//                UserId = userId
//            };
//            _context.QuizResults.Add(result);
//            _context.SaveChanges();

//            string aiSuggestion = score < 2 ? "Hãy ôn lại từ vựng cơ bản!" : "Tốt lắm! Tiếp tục luyện tập nhé!";
//            ViewBag.Score = score;
//            ViewBag.AISuggestion = aiSuggestion;
//            return View("QuizResult");
//        }

//        public IActionResult Progress()
//        {
//            var userId = HttpContext.Session.GetString("UserId");
//            if (string.IsNullOrEmpty(userId))
//            {
//                return RedirectToAction("Login");
//            }

//            var results = _context.QuizResults.Where(r => r.UserId == userId).ToList();
//            return View(results);
//        }

//        public async Task<IActionResult> Logout()
//        {
//            await _signInManager.SignOutAsync();
//            HttpContext.Session.Clear();
//            return RedirectToAction("Index");
//        }

//        public IActionResult PracticeKanji()
//        {
//            return View();
//        }

//        public async Task<IActionResult> LessonDetail()
//        {
//            var n5Course = await _context.Courses.FirstOrDefaultAsync(c => c.Title == "Khóa học Tiếng Nhật N5");

//            if (n5Course == null)
//            {
//                return NotFound();
//            }

//            var lessons = await _context.Lessons.Where(l => l.CourseId == n5Course.CourseId)
//                .OrderBy(l => l.OrderInCourse).ToListAsync();

//            return View(lessons);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetLessonContent(int id)
//        {
//            var lesson = await _context.Lessons
//                .Include(l => l.Course)
//                .Include(l => l.Kanjis)
//                .Include(l => l.Vocabularies)
//                .Include(l => l.Grammars)
//                .FirstOrDefaultAsync(l => l.LessonId == id);

//            if (lesson == null)
//            {
//                return NotFound();
//            }

//            return PartialView("_LessonContent", lesson);
//        }

//        [Authorize]
//        public async Task<IActionResult> Profile()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null) return RedirectToAction("Login");

//            var roles = await _userManager.GetRolesAsync(user);

//            var model = new UserProfileViewModel
//            {
//                UserName = user.UserName,
//                Email = user.Email,
//                Roles = roles
//            };

//            return View(model);
//        }

//        public IActionResult Privacy()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }
//    }
//}
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace JapaneseLearningWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppDbContext _context;
        

        public HomeController(UserManager<IdentityUser> userManager,
                              SignInManager<IdentityUser> signInManager,
                              AppDbContext context
                             )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
           
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: /Home/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Home/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email và mật khẩu không được để trống.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.UserName ?? email, password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Email hoặc mật khẩu không đúng.";
            return View();
        }

       

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string username, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email, username và mật khẩu không được để trống.";
                return View();
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ViewBag.Error = "Email đã được sử dụng.";
                return View();
            }

            var user = new IdentityUser
            {
                Email = email,
                UserName = username
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Error = string.Join("<br>", result.Errors.Select(e => e.Description));
            return View();
        }

        public IActionResult Vocabulary()
        {
            var vocabularies = _context.Vocabularies.ToList();
            return View(vocabularies);
        }

        public IActionResult Quiz()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var randomVocab = _context.Vocabularies.OrderBy(x => Guid.NewGuid()).Take(3).ToList();
            return View(randomVocab);
        }

        [HttpPost]
        public IActionResult SubmitQuiz(int[] answers)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            int score = answers?.Sum() ?? 0;
            var result = new QuizResult
            {
                Score = score,
                DateTaken = DateTime.Now,
                UserId = userId
            };
            _context.QuizResults.Add(result);
            _context.SaveChanges();

            string aiSuggestion = score < 2 ? "Hãy ôn lại từ vựng cơ bản!" : "Tốt lắm! Tiếp tục luyện tập nhé!";
            ViewBag.Score = score;
            ViewBag.AISuggestion = aiSuggestion;
            return View("QuizResult");
        }

        public IActionResult Progress()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var results = _context.QuizResults.Where(r => r.UserId == userId).ToList();
            return View(results);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult PracticeKanji()
        {
            return View();
        }

        public async Task<IActionResult> LessonDetail(int? courseId)
        {
            if (courseId == null)
                return NotFound();

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                return NotFound();

            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.OrderInCourse)
                .ToListAsync();

            // Có thể truyền cả Course và danh sách Lesson nếu muốn hiển thị thông tin khóa học
            ViewBag.CourseTitle = course.Title;

            return View(lessons);
        }
        [HttpGet]
        public async Task<IActionResult> GetLessonContent(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .Include(l => l.Kanjis)
                .Include(l => l.Vocabularies)
                .Include(l => l.Grammars)
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null)
            {
                return NotFound();
            }

            return PartialView("_LessonContent", lesson);
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> CourseLesson()
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .ToListAsync();

            return View(courses);
        }

        public IActionResult CheckUserId()
        {
            return View();
        }
    }
}
