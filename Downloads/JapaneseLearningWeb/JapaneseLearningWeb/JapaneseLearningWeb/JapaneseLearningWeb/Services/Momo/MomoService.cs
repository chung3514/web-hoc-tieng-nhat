//using System.Security.Cryptography;
//using System.Text;
//using JapaneseLearningWeb.Models;
//using JapaneseLearningWeb.Models.MoMo;
//using JapaneseLearningWeb.Services.Momo;
//using Microsoft.Extensions.Options;
//using Newtonsoft.Json;
//using RestSharp;

//namespace JapaneseLearningWeb.Services.Momo
//{   
//    public class MomoService : IMomoService
//    {
//        private readonly IOptions<MomoOptionModel> _options;
//        public MomoService(IOptions<MomoOptionModel> options)
//        {
//            _options = options;
//        }
//        public async Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model)
//        {
//            model.OrderId = DateTime.UtcNow.Ticks.ToString();
//            model.OrderInformation = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInformation;
//            var rawData =
//                $"partnerCode={_options.Value.PartnerCode}" +
//                $"&accessKey={_options.Value.AccessKey}" +
//                $"&requestId={model.OrderId}" +
//                $"&amount={model.Amount}" +
//                $"&orderId={model.OrderId}" +
//                $"&orderInfo={model.OrderInformation}" +
//                $"&returnUrl={_options.Value.ReturnUrl}" +
//                $"&notifyUrl={_options.Value.NotifyUrl}" +
//                $"&extraData=";
//            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);
//            var client = new RestClient(_options.Value.MomoApiUrl);
//            var request = new RestRequest() { Method = Method.Post };
//            request.AddHeader("Content-Type", "application/json; charset=UTF-8");
//            var requestData = new
//            {
//                accessKey = _options.Value.AccessKey,
//                partnerCode = _options.Value.PartnerCode,
//                requestType = _options.Value.RequestType,
//                notifyUrl = _options.Value.NotifyUrl,
//                returnUrl = _options.Value.ReturnUrl,
//                orderId = model.OrderId,
//                amount = model.Amount.ToString(),
//                orderInfo = model.OrderInformation,
//                requestId = model.OrderId,
//                extraData = "",
//                signature = signature
//            };
//            request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);
//            var response = await client.ExecuteAsync(request);

//            return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
//        }

//        public async Task<MomoExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection)
//        {
//            var amount = collection.First(s => s.Key == "amount").Value;
//            var orderInfo = collection.First(s => s.Key == "orderInfo").Value;  
//            var orderId = collection.First(s => s.Key == "orderId").Value;
//            // Không có fullname trong query string nên lấy từ orderInfo nếu có, hoặc để trống
//            string fullname = "";

//            // Cách đơn giản để tách fullname nếu bạn đã nhúng trong orderInfo kiểu: "Khách hàng: admin. Nội dung: ..."
//            if (orderInfo.ToString().StartsWith("Khách hàng:"))
//            {
//                var parts = orderInfo.ToString().Split('.');
//                if (parts.Length > 0)
//                {
//                    fullname = parts[0].Replace("Khách hàng:", "").Trim();
//                }
//            }
//            return new MomoExecuteResponseModel()
//            {
//                Amount = amount,
//                OrderId = orderId,
//                OrderInfo = orderInfo,
//                FullName = fullname,
//            };
//        }

//        private string ComputeHmacSha256(string message, string secretKey)
//        {
//            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
//            var messageBytes = Encoding.UTF8.GetBytes(message);

//            byte[] hashBytes;

//            using (var hmac = new HMACSHA256(keyBytes))
//            {
//                hashBytes = hmac.ComputeHash(messageBytes);
//            }

//            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

//            return hashString;
//        }
//    }
//}


using System.Security.Cryptography;
using System.Text;
using JapaneseLearningWeb.Models;
using JapaneseLearningWeb.Models.MoMo;
using JapaneseLearningWeb.Services.Momo;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace JapaneseLearningWeb.Services.Momo
{
    public class MomoService : IMomoService 
    {
        private readonly IOptions<MomoOptionModel> _options;
        public MomoService(IOptions<MomoOptionModel> options)
        {
            _options = options;
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model)
        {
            model.OrderId = DateTime.UtcNow.Ticks.ToString();
            model.OrderInformation = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInformation;
            var rawData =
                $"partnerCode={_options.Value.PartnerCode}" +
                $"&accessKey={_options.Value.AccessKey}" +
                $"&requestId={model.OrderId}" +
                $"&amount={model.Amount}" +
                $"&orderId={model.OrderId}" +
                $"&orderInfo={model.OrderInformation}" +
                $"&returnUrl={_options.Value.ReturnUrl}" +
                $"&notifyUrl={_options.Value.NotifyUrl}" +
                $"&extraData=";
            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest() { Method = Method.Post };
            request.AddHeader("Content-Type", "application/json; charset=UTF-8");
            var requestData = new
            {
                accessKey = _options.Value.AccessKey,
                partnerCode = _options.Value.PartnerCode,
                requestType = _options.Value.RequestType,
                notifyUrl = _options.Value.NotifyUrl,
                returnUrl = _options.Value.ReturnUrl,
                orderId = model.OrderId,
                amount = model.Amount.ToString(),
                orderInfo = model.OrderInformation,
                requestId = model.OrderId,
                extraData = "",
                signature = signature
            };
            request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);
            var response = await client.ExecuteAsync(request);

            return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
        }

        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection.First(s => s.Key == "amount").Value;
            var orderId = collection.First(s => s.Key == "orderId").Value;
            var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
            //var fullName = collection.First(s => s.Key == "fullName").Value;

            return new MomoExecuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo
                //FullName = fullName
            };
        }


        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            byte[] hashBytes;

            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return hashString;
        }

    }
}
