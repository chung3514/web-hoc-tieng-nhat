using JapaneseLearningWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JapaneseLearningWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VoucherController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        private readonly UserManager<IdentityUser> _userManager;

        public VoucherController(AppDbContext ctx, UserManager<IdentityUser> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
        }

        public class ValidateVoucherRequest
        {
            public string? Code { get; set; }
            public List<int>? SelectedCourseIds { get; set; }
        }

        [HttpPost("Validate")]
        [Authorize] // bắt login (vì voucher user-specific / dùng 1 lần)
        public async Task<IActionResult> Validate([FromBody] ValidateVoucherRequest req)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var code = (req.Code ?? "").Trim();
            var selected = req.SelectedCourseIds ?? new List<int>();
            if (string.IsNullOrWhiteSpace(code)) return BadRequest(new { ok = false, message = "Thiếu mã voucher." });
            if (!selected.Any()) return BadRequest(new { ok = false, message = "Chưa chọn khóa học." });

            var voucher = await _ctx.Vouchers.FirstOrDefaultAsync(v => v.Code == code);
            if (voucher == null) return BadRequest(new { ok = false, message = "Voucher không tồn tại." });

            if (voucher.ExpireAt != null && voucher.ExpireAt < DateTime.UtcNow)
                return BadRequest(new { ok = false, message = "Voucher đã hết hạn." });

            // Chỉ áp cho khóa trả phí
            if (voucher.OnlyPaidCourse)
            {
                var paidCount = await _ctx.Courses.CountAsync(c => selected.Contains(c.CourseId) && !c.IsFree);
                if (paidCount == 0)
                    return BadRequest(new { ok = false, message = "Voucher chỉ áp dụng cho khóa trả phí." });
            }

            // 1 lần / user
            var userVoucher = await _ctx.UserVouchers
                .Include(uv => uv.Voucher)
                .FirstOrDefaultAsync(uv => uv.UserId == userId && uv.Voucher.Code == code);

            if (userVoucher == null)
                return BadRequest(new { ok = false, message = "Voucher này không thuộc về bạn (hoặc chưa được cấp)." });

            if (userVoucher.IsUsed)
                return BadRequest(new { ok = false, message = "Voucher đã được sử dụng." });

            // OK
            return Ok(new
            {
                ok = true,
                code = voucher.Code,
                discountAmount = voucher.DiscountAmount
            });
        }
    }
}
