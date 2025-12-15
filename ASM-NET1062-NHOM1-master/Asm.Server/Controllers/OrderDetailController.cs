using Asm.Server.Data;
using Asm.Server.Dtos.OrderDtos;
using Asm.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asm.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly AppDbContext _db;

        public OrderDetailController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/OrderDetail

        /// <summary>
        /// Danh sách tất cả chi tiết đơn hàng
        /// </summary>
        /// <remarks>
        /// API này trả về toàn bộ danh sách chi tiết hóa đơn.
        /// </remarks>
        /// <returns>Danh sách các món ăn.</returns>
        /// <response code="200">Thành công, trả về danh sách chi tiết hóa đơn.</response>
        /// <response code="404">Không tìm thấy dữ liệu.</response>

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDetailDto>>> GetAll()
        {
            var orderDetails = await _db.OrderDetails
                .Include(od => od.Product)
                .Select(od => new OrderDetailDto
                {
                    Id = od.Id,
                    ProductId = od.ProductId,
                    ProductName = od.Product != null ? od.Product.Name : null,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice
                })
                .ToListAsync();

            return Ok(orderDetails);
        }

        // GET: api/OrderDetail/5

        /// <summary>
        /// Danh sách chi tiết đơn hàng theo Id
        /// </summary>
        /// <param name="id"></param>
        /// <remarks>
        /// Trả về toàn bộ món ăn có OrderId = id.
        /// </remarks>
        /// <returns>Danh sách hóa đơn.</returns>
        /// <response code="200">Thành công.</response>
        /// <response code="404">Không tìm thấy hóa đơn.</response>

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailDto>> GetById(int id)
        {
            var od = await _db.OrderDetails
                .Include(o => o.Product)
                .Where(o => o.Id == id)
                .Select(o => new OrderDetailDto
                {
                    Id = o.Id,
                    ProductId = o.ProductId,
                    ProductName = o.Product != null ? o.Product.Name : null,
                    Quantity = o.Quantity,
                    UnitPrice = o.UnitPrice

                })
                .FirstOrDefaultAsync();

            if (od == null)
                return NotFound();

            return Ok(od);
        }
    }
}
