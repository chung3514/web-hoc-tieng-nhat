namespace JapaneseLearningWeb.Models
{
    public class OrderInfoModel
    {
        public string FullName { get; set; }
        public string OrderId { get; set; }
        public string OrderInformation { get; set; }
        public double Amount { get; set; }
        //public List<int> SelectedCourseIds { get; set; } = new List<int>();
    }
}
