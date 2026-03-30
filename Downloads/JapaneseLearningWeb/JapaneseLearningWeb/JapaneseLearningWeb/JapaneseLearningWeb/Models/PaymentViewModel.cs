namespace JapaneseLearningWeb.Models
{
    public class PaymentViewModel
    {
        public List<Course> SelectedCourses { get; set; } = new();
        public decimal GrandTotal => SelectedCourses?.Where(c => !c.IsFree).Sum(c => c.Price) ?? 0;
    }

}
