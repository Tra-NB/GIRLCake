using Asm.Server.Data;
using Asm.Server.Dtos.FoodDtos;
using Asm.Server.Helpers;
using Asm.Server.Models;
using Azure.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asm.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FoodController : ControllerBase
	{
		private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public FoodController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env; 
        }


        // GET: api/<FoodController>
        /// <summary>
        /// Danh sách tất cả món 
        /// </summary>
        //// <returns>
        /// Danh sách các món.
        /// </returns>
        /// <response code="200">Trả về danh sách tất cá món</response>

        [HttpGet]
		public async Task<ActionResult<IEnumerable<FoodDto>>> GetAll()
		{
			var foods = await _db.Foods
				.Include(f => f.Category)
                .Select(f => new FoodDto
				{
					Id = f.Id,
					Name = f.Name,
					Description = f.Description,
					Price = f.Price,
					ImageUrl = f.ImageUrl,
					IsAvailable = f.IsAvailable,
					CategoryId = f.CategoryId,
					CategoryName = f.Category != null ? f.Category.Name : null,
					CreatedAt = f.CreatedAt,
					UpdatedAt = f.UpdatedAt,
                    DeletedAt = f.DeletedAt
                }).ToListAsync();

			return Ok(foods);
		}

        // GET: api/Food/active
        /// <summary>
        /// Lấy danh sách các món chưa bị xóa (DeletedAt == null)
        /// </summary>
        /// <returns>Danh sách món ăn dạng FoodDto</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<FoodDto>>> GetActive()
        {
            var foods = await _db.Foods
                .Include(f => f.Category)
                .Where(f => f.DeletedAt == null)
                .Select(f => new FoodDto
                {
                    Id = f.Id,                     
                    Name = f.Name,                 
                    Description = f.Description,   
                    Price = f.Price,               
                    ImageUrl = f.ImageUrl,         
                    IsAvailable = f.IsAvailable,   
                    CategoryId = f.CategoryId,     
                    CategoryName = f.Category != null ? f.Category.Name : null, 
                    CreatedAt = f.CreatedAt,       
                    UpdatedAt = f.UpdatedAt        
                }).ToListAsync();

            // Trả về danh sách món ăn dạng JSON với status 200 OK
            return Ok(foods);
        }


        // GET api/<FoodController>/5

        /// <summary>
        /// Danh sách món theo danh mục
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<FoodDto>> GetById(int id)
        {
            var food = await _db.Foods
                .Include(f => f.Category)
                  .Where(f => f.Id == id && f.DeletedAt == null)
                .Select(f => new FoodDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Price = f.Price,
                    ImageUrl = f.ImageUrl,
                    IsAvailable = f.IsAvailable,
                    CategoryId = f.CategoryId,
                    CategoryName = f.Category != null ? f.Category.Name : null,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                }).ToListAsync();

            if (food == null) return NotFound();


            return Ok(food);
        }

        // POST api/<FoodController>

        /// <summary>
        /// Them moi mon 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// 

        [HttpPost]
        public async Task<ActionResult<FoodDto>> Post([FromForm] FoodCreateDto dto) 
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool categoryExists = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return BadRequest($"Category with ID {dto.CategoryId} does not exist.");

            // ======================================
            // LOGIC THÊM 14% VÀO GIÁ BÁN
            // ======================================
            // Lấy giá gốc từ DTO, cộng 14%
            decimal originalPrice = dto.Price;
            decimal calculatedPrice = originalPrice * 1.14m; // 14% là 0.14, cộng vào giá gốc là * 1.14

            dto.Price = calculatedPrice;

            if (dto.FImageFile != null && dto.FImageFile.Length > 0)
            {
				dto.ImageUrl = await ImageHelper.UploadImageAsync(
				     env: _env,
				     file: dto.FImageFile,
				     folderName: "food"
				    ); 
            }


            var food = new Food
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl, 
                IsAvailable = dto.IsAvailable,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Foods.Add(food);
            await _db.SaveChangesAsync();

            await _db.Entry(food).Reference(f => f.Category).LoadAsync();

            var createdFoodDto = new FoodDto
            {
                Id = food.Id,
                Name = food.Name,
                Description = food.Description,
                Price = food.Price,
                ImageUrl = food.ImageUrl,
                IsAvailable = food.IsAvailable,
                CategoryId = food.CategoryId,
                CategoryName = food.Category != null ? food.Category.Name : null,
                CreatedAt = food.CreatedAt,
                UpdatedAt = food.UpdatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = food.Id }, createdFoodDto);
        }




        // PUT api/<FoodController>/5
        /// <summary>
        /// Cap nhat mon 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] FoodUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var food = await _db.Foods.FindAsync(id);
            if (food == null)
                return NotFound();

            bool categoryExists = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return BadRequest($"Category with ID {dto.CategoryId} does not exist.");

            // ======================================
            // LOGIC THÊM 14% VÀO GIÁ BÁN KHI UPDATE
            // ======================================
            // Tính toán giá mới (giá gốc từ DTO + 14%)
            decimal originalPrice = dto.Price;
            decimal calculatedPrice = originalPrice * 1.14m;

            // ============================
            // XỬ LÝ FILE ẢNH KHI UPDATE
            // ============================
            if (dto.FImageFile != null && dto.FImageFile.Length > 0)
            {
                food.ImageUrl = await ImageHelper.UploadImageAsync(
					 env: _env,
					 file: dto.FImageFile,
					 folderName: "food"
					);
			}

            // ============================
            // CẬP NHẬT THÔNG TIN KHÁC
            // ============================
            food.Name = dto.Name;
            food.Description = dto.Description;
            food.Price = calculatedPrice;
            food.IsAvailable = dto.IsAvailable;
            food.CategoryId = dto.CategoryId;
            food.UpdatedAt = DateTime.UtcNow;

            _db.Foods.Update(food);
            await _db.SaveChangesAsync();

            return NoContent();
        }


        // DELETE api/<FoodController>/5

        /// <summary>
        /// Xoa mon 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var food = await _db.Foods.FindAsync(id);
			if (food == null)
			{
				return NotFound();
			}
            food.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
			return NoContent();
		}


        // <summary>
        /// Kho phuc mon 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            var food = await _db.Foods.FindAsync(id);
            if (food == null)
                return NotFound();

            food.DeletedAt = null;
            await _db.SaveChangesAsync();
            return Ok();
        }

    }
}