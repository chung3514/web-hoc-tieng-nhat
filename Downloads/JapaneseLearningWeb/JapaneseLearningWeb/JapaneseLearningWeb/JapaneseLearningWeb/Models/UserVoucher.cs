using System;
using System.ComponentModel.DataAnnotations;

namespace JapaneseLearningWeb.Models
{
    public class UserVoucher
    {
        public int UserVoucherId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int VoucherId { get; set; }
        public Voucher Voucher { get; set; } = null!;

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }
    }
}
