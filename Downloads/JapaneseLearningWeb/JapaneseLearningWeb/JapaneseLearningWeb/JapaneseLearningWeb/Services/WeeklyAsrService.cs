using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace JapaneseLearningWeb.Services
{
    public class WeeklyAsrService
    {
        private readonly AppDbContext _ctx;

        public WeeklyAsrService(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        private (int year, int week) GetCurrentYearWeek()
        {
            var now = DateTime.UtcNow;
            var cal = CultureInfo.CurrentCulture.Calendar;
            int week = cal.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return (now.Year, week);
        }

        /// <summary>Tăng 1 lần ASR pass cho tuần hiện tại.</summary>
        public async Task<AsrWeeklyRecord> IncreaseAsync(string userId)
        {
            var (year, week) = GetCurrentYearWeek();
            var record = await _ctx.AsrWeeklyRecords
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Year == year && x.Week == week);

            if (record == null)
            {
                record = new AsrWeeklyRecord
                {
                    UserId = userId,
                    Year = year,
                    Week = week,
                    TargetCount = 1,
                    DoneCount = 0,
                    RewardUnlocked = false, // ✅ chỉ dùng cho “đã quay/đã nhận”
                    VoucherId = null
                };
                _ctx.AsrWeeklyRecords.Add(record);
            }

            record.DoneCount += 1;
            record.LastUpdated = DateTime.UtcNow;

            // ❌ BỎ: không set RewardUnlocked ở đây nữa
            // RewardUnlocked chỉ set khi user quay vòng (SpinWeekly)

            await _ctx.SaveChangesAsync();
            return record;
        }

        public async Task<List<AsrWeeklySummaryDto>> GetLastWeeksAsync(string userId, int weekCount = 6)
        {
            var now = DateTime.UtcNow;
            var fromDate = now.AddDays(-7 * weekCount);

            var list = await _ctx.AsrWeeklyRecords
                .Where(x => x.UserId == userId && x.LastUpdated >= fromDate)
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Week)
                .Take(weekCount)
                .Select(x => new AsrWeeklySummaryDto
                {
                    Year = x.Year,
                    Week = x.Week,
                    Target = x.TargetCount,
                    Done = x.DoneCount,
                    RewardUnlocked = x.RewardUnlocked
                })
                .ToListAsync();

            return list;
        }

        /// <summary>Lấy record tuần hiện tại (dùng chung cách tính week với IncreaseAsync).</summary>
        public async Task<AsrWeeklyRecord?> GetCurrentAsync(string userId)
        {
            var (year, week) = GetCurrentYearWeek();
            return await _ctx.AsrWeeklyRecords
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Year == year && x.Week == week);
        }
    }

    public class AsrWeeklySummaryDto
    {
        public int Year { get; set; }
        public int Week { get; set; }
        public int Target { get; set; }
        public int Done { get; set; }
        public bool RewardUnlocked { get; set; }
    }
}
