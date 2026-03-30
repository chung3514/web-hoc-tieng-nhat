using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JapaneseLearningWeb.Models
{
    public class Voucher
    {
        public int VoucherId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        // Số tiền giảm (VD: 50000 = 50k)
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999999)]
        public decimal DiscountAmount { get; set; }

        // Chỉ áp dụng cho khóa có phí
        public bool OnlyPaidCourse { get; set; } = true;

        public DateTime ExpireAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // navigation
        public ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
    }
}
