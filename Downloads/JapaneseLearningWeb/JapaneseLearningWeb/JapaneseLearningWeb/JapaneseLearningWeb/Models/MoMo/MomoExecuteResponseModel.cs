namespace JapaneseLearningWeb.Models.MoMo
{
    public class MomoExecuteResponseModel
    {
        public string FullName { get; set; }
        public string OrderId { get; set; }
        public string Amount { get; set; }
        public string OrderInfo { get; set; }

        public List<int> PaidCourseIds { get; set; }
        public int ErrorCode { get; set; }   // thêm ErrorCode vào đây

    }
}
