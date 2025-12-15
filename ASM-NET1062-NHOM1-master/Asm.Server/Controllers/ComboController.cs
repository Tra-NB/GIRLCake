using Asm.Server.Data;
using Asm.Server.Dtos;
using Asm.Server.Dtos.ComboDtos;
using Asm.Server.Helpers;
using Asm.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;

namespace Asm.Server.Controllerss
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComboController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ComboController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ================================================================
        // TÍNH GIÁ COMBO TỰ ĐỘNG (mới)
        // ================================================================
        private decimal CalculateComboPrice(IEnumerable<ComboFood> foods)
        {
            decimal total = 0;

            var foodIds = foods.Select(f => f.FoodId).ToList();
            var foodsInDb = _context.Foods.Where(f => foodIds.Contains(f.Id)).ToList();

            foreach (var cf in foods)
            {
                var food = foodsInDb.FirstOrDefault(f => f.Id == cf.FoodId);
                if (food != null)
                {
                    decimal discountedPrice = food.Price * 0.89m; // giảm 11%
                    total += discountedPrice * cf.Quantity;
                }
            }

            return total;
        }

        // ================================================================
        // GET ACTIVE COMBOS
        // ================================================================
        /// <summary>
        /// Lấy danh sách tất cả combo (chỉ combo chưa bị xóa) và không bao gồm luôn comboFood.
        /// </summary>
        /// <returns>Danh sách combo.</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<ComboResponDto>>> GetActive()
        {
            var result = await _context.Combos
                .Where(c => c.DeletedAt == null)
                .Select(c => new ComboResponDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    Description = c.Description,
                    IsAvailable = c.IsAvailable

                })
                .ToListAsync();

            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách tất cả combo (Cho admin) và không bao gồm luôn comboFood.
        /// </summary>
        /// <returns>Danh sách combo.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ComboResponDto>>> GetAll()
        {
            var result = await _context.Combos
                .Select(c => new ComboResponDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    DeletedAt = c.DeletedAt,
                    Description = c.Description,
                    IsAvailable = c.IsAvailable
                })
                .ToListAsync();

            return Ok(result);
        }

        // ================================================================
        // GET COMBO BY ID
        // ================================================================
        /// <summary>
        /// Lấy thông tin chi tiết của một combo theo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ComboResponDto>> Get(int id)
        {
            var result = await _context.Combos
                .Include(c => c.ComboFoods)
                .Where(c => c.DeletedAt == null && c.Id == id)
                .Select(c => new ComboResponDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    Description = c.Description,
                    IsAvailable = c.IsAvailable,
                    Foods = c.ComboFoods.Select(cf => new FoodInComboDto
                    {
                        FoodId = cf.FoodId,
                        Name = cf.Food.Name,
                        Quantity = cf.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ================================================================
        // CREATE COMBO
        // ================================================================
        /// <summary>
        /// Tạo combo mới (hỗ trợ upload hình ảnh).
        /// </summary>
        /// <remarks>
        /// Sử dụng <b>multipart/form-data</b> để upload ảnh.
        /// </remarks>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] ComboCreateDto request)
        {
            string? imageUrl = await ImageHelper.UploadImageAsync(
                env: _env,
                file: request.Image,
                folderName: "combo"
            );

            var combo = new Combo
            {
                Name = request.Name,
                Price = request.Price, // sẽ được tính lại nếu có foods
                Description = request.Description ?? "No description",
                ImageUrl = imageUrl,
                IsAvailable = request.IsAvailable,
            };

            _context.Combos.Add(combo);
            await _context.SaveChangesAsync();

            if (request.Foods != null)
            {
                foreach (var item in request.Foods)
                {
                    _context.ComboFoods.Add(new ComboFood
                    {
                        ComboId = combo.Id,
                        FoodId = item.FoodId,
                        Quantity = item.Quantity
                    });
                }

                await _context.SaveChangesAsync();

                // --- Tính giá combo tự động dựa trên foods ---
                var comboFoods = _context.ComboFoods.Where(cf => cf.ComboId == combo.Id).ToList();
                combo.Price = CalculateComboPrice(comboFoods);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(Get), new { id = combo.Id }, combo);
        }

        // ================================================================
        // UPDATE COMBO
        // ================================================================
        /// <summary>
        /// Cập nhật combo (cho phép đổi hình ảnh).
        /// </summary>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] ComboUpdateDto request)
        {
            var combo = await _context.Combos
                .Include(c => c.ComboFoods)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (combo == null)
                return NotFound();

            combo.Name = request.Name;
            combo.Price = request.Price; // sẽ được tính lại nếu có foods
            combo.Description = request.Description;
            combo.IsAvailable = request.IsAvailable;

            if (request.Image != null)
            {
                combo.ImageUrl = await ImageHelper.UploadImageAsync(
                    env: HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>(),
                    file: request.Image,
                    folderName: "combo"
                );
            }

            if (request.Foods != null)
            {
                _context.ComboFoods.RemoveRange(combo.ComboFoods);

                combo.ComboFoods = new List<ComboFood>();
                foreach (var f in request.Foods)
                {
                    combo.ComboFoods.Add(new ComboFood
                    {
                        ComboId = combo.Id,
                        FoodId = f.FoodId,
                        Quantity = f.Quantity
                    });
                }

                await _context.SaveChangesAsync();

                // --- Tính giá combo tự động dựa trên foods ---
                combo.Price = CalculateComboPrice(combo.ComboFoods);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ================================================================
        // DELETE COMBO (SOFT DELETE)
        // ================================================================
        /// <summary>
        /// Xóa combo (soft delete – không xóa khỏi DB).
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var combo = await _context.Combos.FindAsync(id);

            if (combo == null)
                return NotFound();

            combo.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ================================================================
        // RESTORE COMBO
        // ================================================================
        [HttpPut("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            var combo = await _context.Combos.FindAsync(id);

            if (combo == null)
                return NotFound();

            combo.DeletedAt = null;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
