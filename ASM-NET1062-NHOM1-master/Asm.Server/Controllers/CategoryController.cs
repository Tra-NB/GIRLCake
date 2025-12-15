using Asm.Server.Data;
using Asm.Server.Dtos.CategoryDtos;
using Asm.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asm.Server.Controllers
{
    /// <summary>
    /// API quản lý loại món ăn (Category)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CategoryController(AppDbContext db)
        {
            _db = db;
        }

        // =====================================================================
        // GET: api/Category
        // =====================================================================
        /// <summary>
        /// Lấy danh sách tất cả loại món
        /// </summary>
        /// <returns>Danh sách các loại món</returns>
        /// <response code="200">Thành công</response>
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
        {
            var categories = await _db.Categories
                .Where(c => c.DeletedAt == null) // chỉ lấy category chưa xóa
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToListAsync();

            return Ok(categories);
        }

        // =====================================================================
        // GET: api/Category/{id}
        // =====================================================================
        /// <summary>
        /// Lấy thông tin loại món theo ID
        /// </summary>
        /// <param name="id">ID của loại món</param>
        /// <returns>Thông tin loại món</returns>
        /// <response code="200">Thành công</response>
        /// <response code="404">Không tìm thấy loại món</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id)
        {
            var category = await _db.Categories
                .Where(c => c.Id == id && c.DeletedAt == null)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound($"Không tìm thấy loại món với ID {id}");

            return Ok(category);
        }

        // =====================================================================
        // POST: api/Category
        // =====================================================================
        /// <summary>
        /// Tạo mới loại món
        /// </summary>
        /// <param name="dto">Thông tin loại món</param>
        /// <returns>Thông tin loại món vừa tạo</returns>
        /// <response code="201">Tạo thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDto>> Post([FromBody] CategoryCreateDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var result = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                CreatedAt = category.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, result);
        }

        // =====================================================================
        // PUT: api/Category/{id}
        // =====================================================================
        /// <summary>
        /// Cập nhật loại món
        /// </summary>
        /// <param name="id">ID loại món cần cập nhật</param>
        /// <param name="dto">Thông tin cập nhật</param>
        /// <returns>Thông tin loại món sau khi cập nhật</returns>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="404">Không tìm thấy loại món</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CategoryUpdateDto dto)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound($"Không tìm thấy loại món với ID {id}");

            category.Name = dto.Name;
            category.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var result = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return Ok(result);
        }

        // =====================================================================
        // DELETE: api/Category/{id}
        // =====================================================================
        /// <summary>
        /// Xóa mềm loại món
        /// </summary>
        /// <param name="id">ID loại món cần xóa</param>
        /// <returns></returns>
        /// <response code="204">Xóa thành công (soft delete)</response>
        /// <response code="404">Không tìm thấy loại món</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound($"Không tìm thấy loại món với ID {id}");

            bool hasFoods = await _db.Foods.AnyAsync(f => f.CategoryId == id && f.DeletedAt == null);

            if(hasFoods)
            {
                return BadRequest("Không thể xóa loại món này vì vẫn còn món ăn thuộc loại này.");
            }

            category.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Category/restore/5
        [HttpPost("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound($"Không tìm thấy loại món với ID {id}");

            category.DeletedAt = null;
            await _db.SaveChangesAsync();

            return Ok(category);
        }

    }
}
