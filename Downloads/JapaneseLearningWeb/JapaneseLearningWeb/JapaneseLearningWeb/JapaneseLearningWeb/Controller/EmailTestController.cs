using Microsoft.AspNetCore.Mvc;
using JapaneseLearningWeb.Services;
using System.Threading.Tasks;

namespace SendGridEmailDemo.Controllers
{
    public class EmailTestController : Controller
    {
        private readonly SendGridEmailSender _emailSender;

        public EmailTestController(SendGridEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendTestEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập email";
                return View("Index");
            }

            string subject = "Test gửi mail SendGrid";
            string content = "<h3>Chào bạn, đây là email test gửi từ SendGrid qua ASP.NET Core</h3>";

            await _emailSender.SendEmailAsync(email, subject, content);

            ViewBag.Message = "Email đã được gửi thành công!";
            return View("Index");
        }
    }
}
