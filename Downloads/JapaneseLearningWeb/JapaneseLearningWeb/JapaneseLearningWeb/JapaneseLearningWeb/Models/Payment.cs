using System.ComponentModel.DataAnnotations.Schema;

namespace JapaneseLearningWeb.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public User User { get; set; } = null!;
        public Course Course { get; set; } = null!;
    }
}