namespace JapaneseLearningWeb.Models.MoMo
{
    public class MomoCreatePaymentResponseModel
    {
        public required string RequestId { get; set; }
        public int ErrorCode { get; set; }
        public required string OrderId { get; set; }
        public required string Message { get; set; }
        public required string LocalMessage { get; set; }
        public required string RequestType { get; set; }
        public required string PayUrl { get; set; }
        public required string Signature { get; set; }
        public required string QrCodeUrl { get; set; }
        public required string Deeplink { get; set; }
        public required string DeeplinkWebInApp { get; set; }
    }
}
