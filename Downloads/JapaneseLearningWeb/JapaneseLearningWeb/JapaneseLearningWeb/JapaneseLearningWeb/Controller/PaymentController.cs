
using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using JapaneseLearningWeb.Services.Momo;
using Microsoft.AspNetCore.Mvc;

namespace JapaneseLearningWeb.Controllers
{
    public class PaymentController : Controller
    {
        private IMomoService _momoService;
        private readonly AppDbContext _dbContext;

        public PaymentController(IMomoService momoService, AppDbContext dbContext)
        {
            _momoService = momoService;
            _dbContext = dbContext;
        }
        [HttpPost]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentMomo(model);
            return Redirect(response.PayUrl);
        }
        [HttpGet]
        public IActionResult PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
                
            return View(response);
        }
    }
}