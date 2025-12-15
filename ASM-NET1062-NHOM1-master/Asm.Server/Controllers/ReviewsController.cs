using Asm.Server.Data;
using Asm.Server.Dtos.ReviewDtos;
using Asm.Server.Helpers;
using Asm.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Asm.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

		// GET: api/Reviews
		/// <summary>
		/// Lấy danh sách tất cả các đánh giá.
		/// </summary>
		/// <returns>
		/// Danh sách các đánh giá, bao gồm thông tin người dùng và thời gian tạo.
		/// </returns>
		/// <response code="200">Trả về danh sách đánh giá</response>
		[HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews()
        {
            var reviews = await _context.Reviews.Include(r => r.User)
                .Include(r => r.User)     // include user
        .       Include(r => r.Product)
                .Select(r => new ReviewDto(r)).ToListAsync();
			return Ok(reviews);
        }

		// GET: api/Reviews/5
		/// <summary>
		/// Lấy chi tiết một đánh giá theo ID.
		/// </summary>
		/// <param name="id">ID của đánh giá cần lấy</param>
		/// <returns>Chi tiết đánh giá tương ứng với ID.</returns>
		/// <response code="200">Trả về thông tin đánh giá</response>
		/// <response code="404">Không tìm thấy đánh giá</response>
		[HttpGet("{id}")]
        public async Task<ActionResult<ReviewDto>> GetReview(int id)
        {
            var existingReview = await _context.Reviews.FindAsync(id);

            if (existingReview == null)
            {
                return NotFound();
            }

            return Ok(new ReviewDto(existingReview));
        }

		/// <summary>
		/// Lấy danh sách đánh giá theo sản phẩm.
		/// </summary>
		/// <param name="productId">ID sản phẩm cần lấy danh sách đánh giá</param>
		/// <returns>Danh sách các đánh giá thuộc sản phẩm.</returns>
		/// <response code="200">Trả về danh sách đánh giá</response>
		[HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByProduct(int productId)
        {
            var reviews = await _context.Reviews
                 .Include(r => r.User)
                .Where(r => r.ProductId == productId && r.DeletedAt == null)
                .Select(r => new ReviewDto(r))
				.ToListAsync();
            return Ok(reviews);
		}

		/// <summary>
		/// Lấy danh sách đánh giá của người dùng đang đăng nhập.
		/// </summary>
		/// <returns>Danh sách các đánh giá mà người dùng đã tạo. 
        /// Chỉ trả về đánh giá chưa bị xóa (DeletedAt = null)</returns>
		/// <response code="200">Trả về danh sách đánh giá</response>
		[HttpGet("me")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetMyReviews()
        {
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
				return Unauthorized("User not logged in");

			var reviews = await _context.Reviews
                .Where(r => r.UserId == userId && r.DeletedAt == null)
                .Select(r => new ReviewDto(r))
				.ToListAsync();
            return Ok(reviews);
		}

		// PUT: api/Reviews/5
		/// <summary>
		/// Cập nhật nội dung của một đánh giá.
		/// </summary>
		/// <param name="id">ID của đánh giá cần cập nhật</param>
		/// <param name="review">Dữ liệu mới để cập nhật đánh giá</param>
		/// <returns>Trả về đánh giá sau khi cập nhật.</returns>
		/// <response code="200">Cập nhật thành công và trả về dữ liệu mới</response>
		/// <response code="400">Không thể cập nhật vì đánh giá đã bị xóa</response>
		/// <response code="404">Không tìm thấy đánh giá</response>
		[HttpPut("{id}")]
        public async Task<IActionResult> PutReview(int id, ReviewUpdateDto review)
        {
            var existingReview = await _context.Reviews.FindAsync(id);
            if (existingReview == null)
                return NotFound();

            if (existingReview.DeletedAt != null)
                return BadRequest("Cannot update a deleted review.");

            existingReview.Rating = review.Rating;
            existingReview.Comment = review.Comment;
            existingReview.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok ( new ReviewDto(existingReview));
        }

        [HttpPut("{id}/toggle-visibility")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            // Nếu đã bị ẩn → hiện, ngược lại → ẩn
            review.DeletedAt = review.DeletedAt == null ? DateTime.Now : null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thành công", deletedAt = review.DeletedAt });
        }


        // POST: api/Reviews
        /// <summary>
        /// Tạo một đánh giá mới.
        /// </summary>
        /// <param name="review">Thông tin đánh giá mới</param>
        /// <returns>Đánh giá vừa tạo</returns>
        /// <response code="201">Đã tạo thành công</response>
        [HttpPost]
        public async Task<ActionResult<ReviewDto>> PostReview(ReviewCreateDto review)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not logged in"); // trả 401 nếu token không hợp lệ

            if (review == null || review.ProductId <= 0 || review.Rating <= 0 || string.IsNullOrEmpty(review.Comment))
                return BadRequest("Dữ liệu review không hợp lệ");

            var newReview = new Review
            {
                ProductId = review.ProductId,
                UserId = userId,        // lấy từ token
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(newReview);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReview", new { id = newReview.Id }, new ReviewDto(newReview));
        }


        // DELETE: api/Reviews/5
        /// <summary>
        /// Xóa một đánh giá theo ID.
        /// </summary>
        /// <remarks>
        /// Hành động này chỉ đánh dấu đánh giá là đã bị xóa (soft delete),
        /// không xóa hoàn toàn khỏi cơ sở dữ liệu.
        /// </remarks>
        /// <param name="id">ID của đánh giá cần xóa</param>
        /// <returns>Không có nội dung.</returns>
        /// <response code="204">Xóa thành công</response>
        /// <response code="404">Không tìm thấy đánh giá</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

			review.DeletedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;

            _context.Reviews.Update(review);
			await _context.SaveChangesAsync();

            return NoContent();
        }



        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
	}
}
