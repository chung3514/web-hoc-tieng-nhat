using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace JapaneseLearningWeb.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string OrderId { get; set; } = Guid.NewGuid().ToString(); // Default fallback

        public string FullName { get; set; }

        public string OrderInformation { get; set; }

        public double Amount { get; set; }

        public string SelectedCourseIdsJson { get; set; } = "[]"; // Avoid null

        [NotMapped]
        public List<int> SelectedCourseIds
        {
            get
            {
                try
                {
                    return JsonSerializer.Deserialize<List<int>>(SelectedCourseIdsJson) ?? new List<int>();
                }
                catch
                {
                    return new List<int>(); // fallback nếu JSON lỗi
                }
            }
            set
            {
                SelectedCourseIdsJson = JsonSerializer.Serialize(value ?? new List<int>());
            }
        }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Optional: để liên kết user nếu cần
    }
}
