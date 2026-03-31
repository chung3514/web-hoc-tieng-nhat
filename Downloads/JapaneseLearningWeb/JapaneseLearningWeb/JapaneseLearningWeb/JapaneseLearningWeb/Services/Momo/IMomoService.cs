using JapaneseLearningWeb.Models;
using JapaneseLearningWeb.Models.MoMo;

namespace JapaneseLearningWeb.Services.Momo
{
    public interface IMomoService
    {
        //Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model);
        //Task<MomoExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection);
        Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);

    }
}
