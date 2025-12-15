using Asm.Server.Data;
using Asm.Server.Dtos.VoucherDtos;
using Asm.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asm.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VoucherController(AppDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // GET ALL VOUCHERS
        // ================================================================
        /// <summary>
        /// Lấy danh sách tất cả voucher.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VoucherResponseDto>>> GetAll()
        {
            var vouchers = await _context.Vouchers.ToListAsync();

            return Ok(vouchers.Select(v => new VoucherResponseDto
            {
                Id = v.Id,
                Code = v.Code,
                Description = v.Description,
                DiscountType = v.DiscountType,
                DiscountValue = v.DiscountValue,
                IsActive = v.IsActive,
                UsageLimit = v.UsageLimit,
                UsedCount = v.UsedCount,
                StartDate = v.StartDate,
                EndDate = v.EndDate
            }));
        }

		[HttpGet("active")]
		public async Task<ActionResult<IEnumerable<VoucherResponseDto>>> GetActiveVouchers()
		{
			var now = DateTime.UtcNow;

			var vouchers = await _context.Vouchers
				.Where(v => v.IsActive && v.StartDate <= now && v.EndDate >= now)
				.ToListAsync();

			var result = vouchers.Select(v => new VoucherResponseDto
			{
				Id = v.Id,
				Code = v.Code,
				Description = v.Description,
				DiscountType = v.DiscountType,
				DiscountValue = v.DiscountValue,
				IsActive = v.IsActive,
				UsageLimit = v.UsageLimit,
				UsedCount = v.UsedCount,
				StartDate = v.StartDate,
				EndDate = v.EndDate
			});

			return Ok(result);
		}


		// ================================================================
		// GET BY ID
		// ================================================================
		/// <summary>
		/// Lấy thông tin 1 voucher theo ID.
		/// </summary>
		[HttpGet("{id}")]
        public async Task<ActionResult<VoucherResponseDto>> Get(int id)
        {
            var v = await _context.Vouchers.FindAsync(id);
            if (v == null)
                return NotFound();

            return Ok(new VoucherResponseDto
            {
                Id = v.Id,
                Code = v.Code,
                Description = v.Description,
                DiscountType = v.DiscountType,
                DiscountValue = v.DiscountValue,
                IsActive = v.IsActive,
                UsageLimit = v.UsageLimit,
                UsedCount = v.UsedCount,
                StartDate = v.StartDate,
                EndDate = v.EndDate
            });
        }

        // ================================================================
        // CREATE VOUCHER
        // ================================================================
        /// <summary>
        /// Tạo voucher mới.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VoucherCreateDto dto)
        {
            var v = new Voucher
            {
                Code = dto.Code,
                Description = dto.Description,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                IsActive = dto.IsActive,
                UsageLimit = dto.UsageLimit,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                UsedCount = 0
            };

            _context.Vouchers.Add(v);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = v.Id }, v);
        }

        // ================================================================
        // UPDATE VOUCHER
        // ================================================================
        /// <summary>
        /// Cập nhật voucher.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VoucherUpdateDto dto)
        {
            var v = await _context.Vouchers.FindAsync(id);
            if (v == null)
                return NotFound();

            v.Code = dto.Code;
            v.Description = dto.Description;
            v.DiscountType = dto.DiscountType;
            v.DiscountValue = dto.DiscountValue;
            v.IsActive = dto.IsActive;
            v.UsageLimit = dto.UsageLimit;
            v.StartDate = dto.StartDate;
            v.EndDate = dto.EndDate;

            await _context.SaveChangesAsync();

            return NoContent();
        }

		// ================================================================
		// DELETE VOUCHER (HARD DELETE)
		// ================================================================
		/// <summary>
		/// Xóa voucher khỏi hệ thống nếu chưa có ai sử dụng.
		/// </summary>
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var voucher = await _context.Vouchers.FindAsync(id);
			if (voucher == null)
				return NotFound("Voucher not found");

			if (voucher.UsedCount > 0)
				return BadRequest("Cannot delete voucher: it has already been used.");

			_context.Vouchers.Remove(voucher);
			await _context.SaveChangesAsync();

			return NoContent();
		}


		// ================================================================
		// VALIDATE VOUCHER
		// ================================================================
		/// <summary>
		/// Kiểm tra voucher hợp lệ và tính tiền giảm.
		/// </summary>
		[HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] VoucherDto dto, [FromQuery] decimal cartTotal)
        {
            var response = new
            {
                isValid = false,
                message = "",
                discountAmount = 0m,
                newTotal = cartTotal,
                voucherId = (int?)null
            };

            var v = await _context.Vouchers.FirstOrDefaultAsync(x => x.Code == dto.Code);
            if (v == null)
                return BadRequest(response with { message = "Voucher không tồn tại" });

            if (!v.IsActive)
                return BadRequest(response with { message = "Voucher đã bị vô hiệu hóa" });

            if (DateTime.UtcNow < v.StartDate)
                return BadRequest(response with { message = "Voucher chưa bắt đầu" });

            if (DateTime.UtcNow > v.EndDate)
                return BadRequest(response with { message = "Voucher đã hết hạn" });

            if (v.UsageLimit > 0 && v.UsedCount >= v.UsageLimit)
                return BadRequest(response with { message = "Voucher đã dùng hết số lần cho phép" });

            decimal discount = (v.DiscountType == DiscountType.Percentage)
                ? cartTotal * (v.DiscountValue / 100)
                : v.DiscountValue;

            if (discount > cartTotal)
                discount = cartTotal;

            return Ok(new
            {
                isValid = true,
                message = "Voucher hợp lệ",
                discountAmount = discount,
                newTotal = cartTotal - discount,
                voucherId = v.Id
            });
        }

        // ================================================================
        // REDEEM VOUCHER
        // ================================================================
        /// <summary>
        /// Tăng số lần sử dụng của voucher sau khi thanh toán.
        /// </summary>
        [HttpPost("redeem/{voucherId}")]
        public async Task<IActionResult> Redeem(int voucherId)
        {
            var v = await _context.Vouchers.FindAsync(voucherId);
            if (v == null)
                return NotFound("Voucher không tồn tại");

            v.UsedCount++;
            await _context.SaveChangesAsync();

            return Ok("Redeemed");
        }
    }
}
