// Controllers/AsrController.cs
using JapaneseLearningWeb.Data;
using JapaneseLearningWeb.Models;
using JapaneseLearningWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace JapaneseLearningWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AsrController : ControllerBase
    {
        private readonly IAsrTranscriber? _transcriber;

        private readonly WeeklyAsrService _weeklyAsrService;
        private readonly UserManager<IdentityUser> _userManager;

        // ✅ thêm DbContext
        private readonly AppDbContext _ctx;

        public AsrController(
            AppDbContext ctx,                     // ✅ thêm
            WeeklyAsrService weeklyAsrService,
            UserManager<IdentityUser> userManager,
            IAsrTranscriber? transcriber = null)
        {
            _ctx = ctx;                            // ✅ thêm
            _weeklyAsrService = weeklyAsrService;
            _userManager = userManager;
            _transcriber = transcriber;
        }

        public record AsrResult(
            string transcript,
            int moraCount,
            string[] moraSeq,
            bool longVowel,
            int matchPercent
        );

        [HttpPost("Assess")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Assess([FromForm] IFormFile audio, [FromForm] string? target, CancellationToken ct)
        {
            if (audio == null || audio.Length == 0)
                return BadRequest("No audio");

            await using var ms = new MemoryStream();
            await audio.CopyToAsync(ms, ct);
            ms.Position = 0;

            string mime = string.IsNullOrWhiteSpace(audio.ContentType) ? "audio/webm" : audio.ContentType;

            string transcript;
            if (_transcriber != null)
            {
                try
                {
                    transcript = await _transcriber.TranscribeAsync(ms, mime, ct);
                }
                catch (OperationCanceledException) { return StatusCode(504, "ASR timeout"); }
                catch (Exception ex) { return StatusCode(502, $"ASR engine error: {ex.Message}"); }
            }
            else
            {
                transcript = "おはようございます";
            }

            var normRec = Ja.NormalizeJa(transcript);
            var hiraRec = Ja.ToHira(normRec);
            var moraSeg = Ja.MoraSegment(hiraRec);
            int moraCount = moraSeg.Count;
            bool longVowel = Ja.HasChoon(hiraRec);

            int matchPercent = 0;
            if (!string.IsNullOrWhiteSpace(target))
            {
                matchPercent = Ja.PercentMatch(normRec, target!);
            }

            var result = new AsrResult(
                transcript: transcript,
                moraCount: moraCount,
                moraSeq: moraSeg.ToArray(),
                longVowel: longVowel,
                matchPercent: matchPercent
            );

            return Ok(result);
        }

        // ================== WEEKLY SUMMARY ==================
        [HttpGet("GetWeeklySummary")]
        [Authorize]
        public async Task<IActionResult> GetWeeklySummary()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized();

            var data = await _weeklyAsrService.GetLastWeeksAsync(userId, 6);
            return Ok(data);
        }

        [HttpPost("IncreaseWeekly")]
        [Authorize]
        public async Task<IActionResult> IncreaseWeekly(CancellationToken ct)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized();

            var rec = await _weeklyAsrService.IncreaseAsync(userId);

            var dto = new AsrWeeklySummaryDto
            {
                Year = rec.Year,
                Week = rec.Week,
                Target = rec.TargetCount,
                Done = rec.DoneCount,
                RewardUnlocked = rec.RewardUnlocked
            };

            return Ok(dto);
        }

        // ================== 🎡 SPIN WEEKLY (mới) ==================
        // JS đang gửi header RequestVerificationToken => antiforgery OK nếu bạn đã cấu hình antiforgery cho app.
        [HttpPost("SpinWeekly")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SpinWeekly()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            // lấy record tuần hiện tại (không tăng done)
            var rec = await _weeklyAsrService.GetCurrentAsync(userId);
            if (rec == null) return BadRequest("Không có dữ liệu tuần hiện tại.");

            // đã quay rồi
            if (rec.RewardUnlocked)
            {
                // nếu bạn có lưu VoucherId thì trả thêm code cho UI
                Voucher? v = null;
                if (rec.VoucherId != null)
                    v = await _ctx.Vouchers.FirstOrDefaultAsync(x => x.VoucherId == rec.VoucherId);

                return Ok(new
                {
                    already = true,
                    rewardKey = (v == null ? "LOSE" : "VOUCHER"),
                    rewardTitle = (v == null ? "Chúc bạn may mắn lần sau 😊" : $"Voucher {v.DiscountAmount:N0}đ"),
                    voucherCode = v?.Code
                });
            }

            // chưa đạt chỉ tiêu thì không cho quay
            if (rec.DoneCount < rec.TargetCount)
                return BadRequest("Bạn chưa đạt chỉ tiêu tuần để quay.");

            // ===== danh sách giải (voucher số tiền) =====
            var prizes = new[]
            {
                new Prize("LOSE",    0m,       40),   // trượt
                new Prize("VOUCHER", 50000m,   30),
                new Prize("VOUCHER", 100000m,  20),
                new Prize("VOUCHER", 200000m,  10),
            };

            var pick = PickWeighted(prizes);

            // khóa luôn: 1 tuần chỉ quay 1 lần
            rec.RewardUnlocked = true;

            Voucher? voucher = null;

            if (pick.Type == "VOUCHER" && pick.Amount > 0)
            {
                voucher = new Voucher
                {
                    Code = "ASR" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    DiscountAmount = pick.Amount,
                    OnlyPaidCourse = true,
                    ExpireAt = DateTime.UtcNow.AddDays(30),
                    CreatedAt = DateTime.UtcNow
                };

                _ctx.Vouchers.Add(voucher);
                await _ctx.SaveChangesAsync(); // để có VoucherId

                // gắn voucher cho user (1 lần dùng)
                _ctx.UserVouchers.Add(new UserVoucher
                {
                    UserId = userId,
                    VoucherId = voucher.VoucherId,
                    IsUsed = false
                });

                // lưu vào tuần
                rec.VoucherId = voucher.VoucherId;
            }

            await _ctx.SaveChangesAsync();

            return Ok(new
            {
                already = false,
                rewardKey = (voucher == null ? "LOSE" : "VOUCHER"),
                rewardTitle = (voucher == null ? "Chúc bạn may mắn lần sau 😊" : $"Voucher {voucher.DiscountAmount:N0}đ"),
                voucherCode = voucher?.Code
            });
        }

        private record Prize(string Type, decimal Amount, int Weight);

        private static Prize PickWeighted(Prize[] items)
        {
            var total = items.Sum(x => x.Weight);
            var r = Random.Shared.Next(1, total + 1);
            var sum = 0;
            foreach (var it in items)
            {
                sum += it.Weight;
                if (r <= sum) return it;
            }
            return items[^1];
        }

        // ===================== Japanese helpers =====================
        private static class Ja
        {
            public static string KataToHira(string s)
            {
                if (string.IsNullOrEmpty(s)) return s;
                var sb = new StringBuilder(s.Length);
                foreach (var ch in s)
                {
                    if (ch >= 'ァ' && ch <= 'ン')
                        sb.Append((char)(ch - 0x60));
                    else
                        sb.Append(ch);
                }
                return sb.ToString();
            }

            public static string NormalizeJa(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                var tmp = s.Normalize(NormalizationForm.FormC);
                tmp = Regex.Replace(tmp, "[！!。．\\.、，,？?・「」『』\\s]", "");
                return tmp;
            }

            public static string ToHira(string s) => KataToHira(s);

            private static readonly HashSet<char> SmallHira =
                new(new[] { 'ゃ', 'ゅ', 'ょ', 'ぁ', 'ぃ', 'ぅ', 'ぇ', 'ぉ', 'ゎ', 'ゕ', 'ゖ' });

            public static List<string> MoraSegment(string hiraStr)
            {
                var chars = hiraStr.ToCharArray();
                var segs = new List<string>();
                for (int i = 0; i < chars.Length; i++)
                {
                    var c = chars[i];
                    if (c == 'っ' || c == 'ん' || c == 'ー')
                    {
                        segs.Add(c.ToString());
                        continue;
                    }
                    if (SmallHira.Contains(c))
                    {
                        if (segs.Count > 0) segs[^1] += c;
                        else segs.Add(c.ToString());
                        continue;
                    }
                    segs.Add(c.ToString());
                }
                return segs;
            }

            public static bool HasChoon(string hiraStr)
            {
                if (string.IsNullOrEmpty(hiraStr)) return false;
                if (hiraStr.Contains('ー')) return true;
                return Regex.IsMatch(hiraStr, "(おう|えい)");
            }

            public static int PercentMatch(string recognizedNorm, string targetRaw)
            {
                var a = NormalizeJa(recognizedNorm);
                var b = NormalizeJa(targetRaw);
                if (a.Length == 0 || b.Length == 0) return 0;
                int same = 0, len = Math.Max(a.Length, b.Length);
                var m = Math.Min(a.Length, b.Length);
                for (int i = 0; i < m; i++)
                {
                    if (a[i] == b[i]) same++;
                }
                return (int)Math.Round(100.0 * same / len);
            }
        }
    }

    public interface IAsrTranscriber
    {
        Task<string> TranscribeAsync(Stream audioStream, string mimeType, CancellationToken ct);
    }
}
