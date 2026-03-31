// Models/AsrWeeklyRecord.cs
using System;

namespace JapaneseLearningWeb.Models
{
    public class AsrWeeklyRecord
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public int Year { get; set; }
        public int Week { get; set; }

        /// <summary>Số lần ASR pass cần đạt trong tuần</summary>
        public int TargetCount { get; set; } = 20;

        /// <summary>Số lần ASR pass thực tế trong tuần</summary>
        public int DoneCount { get; set; }

        /// <summary>Đã mở thưởng cho tuần này hay chưa</summary>
        public bool RewardUnlocked { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public int? VoucherId { get; set; }
        public Voucher? Voucher { get; set; }

    }
}
