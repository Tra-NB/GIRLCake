using Asm.Server.Data;
using Asm.Server.Dtos.OrderDtos;
using Asm.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asm.Server.Controllers
{
    /// <summary>
    /// API quản lý đơn hàng
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _db;

        public OrderController(AppDbContext db)
        {
            _db = db;
        }

        // =====================================================================
        // GET: api/Order
        // =====================================================================

        /// <summary>
        /// Lấy danh sách tất cả đơn hàng
        /// </summary>
        /// <remarks>
        /// Trả về toàn bộ danh sách đơn hàng, kèm thông tin chi tiết, user và voucher
        /// </remarks>
        /// <returns>Danh sách các đơn hàng</returns>
        /// <response code="200">Thành công, trả về danh sách đơn hàng</response>
        /// <response code="404">Không tìm thấy đơn hàng</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
        {
            var orders = await _db.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    ShippingAddress = o.ShippingAddress,
                    Status = o.Status,
                    DiscountAmount = o.DiscountAmount,
                    TotalAmount = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    UserId = o.UserId,
                    UserName = o.User != null ? o.User.FullName : null,
                    VoucherCode = o.Voucher != null ? o.Voucher.Code : null,

                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto
                    {
                        ProductId = od.ProductId,
                        ProductName = od.Product != null ? od.Product.Name : null,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    }).ToList()
                }).ToListAsync();

            return Ok(orders);
        }

        // =====================================================================
        // GET: api/Order/{id}
        // =====================================================================

        /// <summary>
        /// Lấy thông tin chi tiết đơn hàng theo Id
        /// </summary>
        /// <param name="id">ID đơn hàng</param>
        /// <returns>Thông tin đơn hàng</returns>
        /// <response code="200">Thành công, trả về đơn hàng</response>
        /// <response code="404">Không tìm thấy đơn hàng</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetById(int id)
        {
            var order = await _db.Orders
                 .Where(o => o.Id == id)
                 .Include(o => o.OrderDetails)
                 .Include(o => o.User)
                 .Include(o => o.Voucher)
                 .Select(o => new OrderDto
                 {
                     Id = o.Id,
                     ShippingAddress = o.ShippingAddress,
                     Status = o.Status,
                     DiscountAmount = o.DiscountAmount,
                     TotalAmount = o.TotalAmount,
                     PaymentMethod = o.PaymentMethod,
                     CreatedAt = o.CreatedAt,
                     UpdatedAt = o.UpdatedAt,

                     UserId = o.UserId,
                     UserName = o.User != null ? o.User.FullName : null,
                     VoucherCode = o.Voucher != null ? o.Voucher.Code : null,

                     OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto
                     {
                         ProductId = od.ProductId,
                         ProductName = od.Product != null ? od.Product.Name : null,
                         Quantity = od.Quantity,
                         UnitPrice = od.UnitPrice
                     }).ToList()
                 }).FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // =====================================================================
        // POST: api/Order
        // =====================================================================

        /// <summary>
        /// Tạo mới đơn hàng
        /// </summary>
        /// <param name="dto">Thông tin đơn hàng cần tạo</param>
        /// <remarks>
        /// Khi tạo đơn hàng mới, trạng thái mặc định là Pending.
        /// Có thể thêm chi tiết đơn hàng kèm theo
        /// </remarks>
        /// <returns>Thông tin đơn hàng vừa tạo</returns>
        /// <response code="201">Tạo đơn hàng thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        [HttpPost]
        public async Task<ActionResult<OrderCreateDto>> Post([FromBody] OrderCreateDto dto)
        {
            if (dto.OrderDetails == null || dto.OrderDetails.Count == 0)
                return BadRequest("Không có sản phẩm trong đơn hàng.");

            decimal subtotal = dto.OrderDetails.Sum(od => od.UnitPrice * od.Quantity);
            decimal discount = 0;
            decimal shippingFee = 15000; 
            Voucher? voucher = null;

            // --- Xử lý voucher ---
            if (!string.IsNullOrEmpty(dto.VoucherCode))
            {
                voucher = await _db.Vouchers
                    .Where(v => v.Code == dto.VoucherCode && v.IsActive)
                    .FirstOrDefaultAsync();

                if (voucher != null)
                {
                    if (voucher.Code == "FREESHIP15")
                    {
                        // Voucher free ship: chỉ giảm phí vận chuyển, không giảm sản phẩm
                        shippingFee = 0;
                        discount = 0; // không giảm giá sản phẩm
                    }
                    else
                    {
                        // Voucher giảm sản phẩm
                        if (voucher.DiscountType == 0) // % giảm
                            discount = Math.Round(subtotal * voucher.DiscountValue / 100);
                        else
                            discount = voucher.DiscountValue;
                    }

                    // Cập nhật số lần dùng voucher
                    voucher.UsedCount += 1;
                    if (voucher.UsageLimit > 0 && voucher.UsedCount >= voucher.UsageLimit)
                        voucher.IsActive = false;

                    _db.Vouchers.Update(voucher);
                }
            }
            //Nơi bạn đặt Breakpoint và thấy TotalAmount = 78000
            decimal totalAmount = subtotal - discount + shippingFee;
            totalAmount = Math.Max(totalAmount, 0);

            // Tạo đơn hàng
            var order = new Order
            {
                ShippingAddress = dto.ShippingAddress,
                Status = OrderStatus.Pending,
                PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod),
                DiscountAmount = discount,
                TotalAmount = totalAmount,
                UserId = dto.UserId,
                VoucherId = voucher?.Id,
                CreatedAt = DateTime.UtcNow
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Thêm chi tiết đơn hàng
            foreach (var detailDto in dto.OrderDetails)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = detailDto.ProductId,
                    Quantity = detailDto.Quantity,
                    UnitPrice = detailDto.UnitPrice
                };
                _db.OrderDetails.Add(orderDetail);
            }
            await _db.SaveChangesAsync();

            // Trả về đơn hàng vừa tạo
            var result = await GetById(order.Id);
            return result.Result;
        }



        // =====================================================================
        // PUT: api/Order/{id}
        // =====================================================================

        /// <summary>
        /// Cập nhật trạng thái đơn hàng
        /// </summary>
        /// <param name="id">ID đơn hàng cần cập nhật</param>
        /// <param name="dto">Thông tin cập nhật (chỉ cập nhật trạng thái)</param>
        /// <returns>Thông tin đơn hàng sau khi cập nhật</returns>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="404">Không tìm thấy đơn hàng</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] OrderUpdateDto dto)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            order.Status = dto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var result = await GetById(order.Id);
            return Ok(result.Value);
        }


        // GET: api/Order/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId không hợp lệ");

            var orders = await _db.Orders
                .Where(o => o.UserId == userId) // UserId trong DB là string
                .Include(o => o.OrderDetails)
                .Include(o => o.Voucher)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    ShippingAddress = o.ShippingAddress,
                    Status = o.Status,
                    DiscountAmount = o.DiscountAmount,
                    TotalAmount = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    UserId = o.UserId,
                    VoucherId = o.Voucher != null ? (int?)o.Voucher.Id : null,
                    VoucherCode = o.Voucher != null ? o.Voucher.Code : null,
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto
                    {
                        ProductId = od.ProductId,
                        ProductName = od.Product != null ? od.Product.Name : null,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    }).ToList()
                }).ToListAsync();

            if (orders == null || orders.Count == 0)
                return NotFound();

            return Ok(orders);
        }


    }
}
