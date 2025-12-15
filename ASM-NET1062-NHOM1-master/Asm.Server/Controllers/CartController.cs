using Asm.Server.Data;
using Asm.Server.Dtos;
using Asm.Server.Dtos.Cart;
using Asm.Server.Dtos.CartDtos;
using Asm.Server.Dtos.VoucherDtos;
using Asm.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Asm.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================================
        // ADD PRODUCT TO CART
        // =====================================================================
        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng.
        /// </summary>
        /// <remarks>
        /// - Nếu chưa có giỏ hàng → tạo mới
        /// - Nếu sản phẩm đã có trong giỏ → tăng số lượng  
        /// - Nếu sản phẩm chưa có → thêm mới  
        /// </remarks>
        /// <param name="request">Thông tin sản phẩm và số lượng.</param>
        /// <returns>Giỏ hàng sau khi cập nhật.</returns>
        /// <response code="200">Thêm sản phẩm thành công</response>
        /// <response code="404">Không tìm thấy sản phẩm</response>
        [HttpPost("product")]
        public async Task<IActionResult> AddProductToCart([FromBody] CartDetailDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User not logged in");

            var cart = await _context.Carts
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
                return NotFound("Product not found");

            var existingCartDetail = await _context.CartDetails
                .FirstOrDefaultAsync(c => c.CartId == cart.Id && c.ProductId == request.ProductId);

            if (existingCartDetail != null)
            {
                existingCartDetail.Quantity = request.Quantity;
                _context.CartDetails.Update(existingCartDetail);
            }
            else
            {
                var newCartDetail = new CartDetail
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };
                await _context.CartDetails.AddAsync(newCartDetail);
            }

            await _context.SaveChangesAsync();

            var cartItems = await _context.CartDetails
                .Where(c => c.CartId == cart.Id)
                .Include(c => c.Product)
                .ToListAsync();

            var response = cartItems.Select(c => new CartDetailResponseDto
            {
                ProductId = c.ProductId,
                ProductName = c.Product.Name,
                Price = c.Product.Price,
                Quantity = c.Quantity
            });

            return Ok(response);
        }




        // =====================================================================
        // APPLY VOUCHER
        // =====================================================================
        /// <summary>
        /// Áp dụng mã giảm giá vào tổng giỏ hàng.
        /// </summary>
        /// <remarks>
        /// Voucher được kiểm tra theo các điều kiện:  
        /// - Còn hiệu lực thời gian  
        /// - Đang active  
        /// - Chưa dùng quá số lần cho phép  
        /// - Loại giảm giá: phần trăm / số tiền  
        /// </remarks>
        /// <param name="request">Mã voucher.</param>
        /// <returns>Tổng tiền sau khi áp dụng voucher.</returns>
        /// <response code="200">Áp dụng voucher thành công</response>
        /// <response code="400">Voucher không hợp lệ</response>
        /// <response code="404">Không tìm thấy voucher</response>
        [HttpPost("voucher")]
        public async Task<ActionResult<CartVoucherResponseDto>> ApplyVoucher([FromBody] VoucherDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User not logged in");

            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == request.Code);

            if (voucher == null)
                return NotFound("Voucher not found");

            if (!voucher.IsActive ||
                DateTime.UtcNow < voucher.StartDate ||
                DateTime.UtcNow > voucher.EndDate)
                return BadRequest("Voucher is not valid");

            if (voucher.UsageLimit > 0 && voucher.UsedCount >= voucher.UsageLimit)
                return BadRequest("Voucher usage limit exceeded");

            var cart = await _context.Carts
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (cart == null)
                return BadRequest("Cart not found");

            var cartDetails = await _context.CartDetails
                .Where(cd => cd.CartId == cart.Id)
                .Include(cd => cd.Product)
                .ToListAsync();

            decimal cartTotal = cartDetails.Sum(cd => cd.Product.Price * cd.Quantity);

            decimal discount = 0;

            if (voucher.DiscountType == DiscountType.Percentage)
                discount = cartTotal * (voucher.DiscountValue / 100);
            else
                discount = voucher.DiscountValue;

            if (discount > cartTotal) discount = cartTotal;

            return Ok(new CartVoucherResponseDto
            {
                Description = voucher.Description,
                VoucherId = voucher.Id,
                Discount = discount,
                NewTotal = cartTotal - discount
            });
        }


        // =====================================================================
        // GET CART
        // =====================================================================
        /// <summary>
        /// Lấy toàn bộ giỏ hàng.
        /// </summary>
        /// <remarks>
        /// Trả về:  
        /// - Danh sách sản phẩm trong giỏ hàng 
        /// - Tổng tiền  
        /// </remarks>
        /// <returns>Giỏ hàng hiện tại.</returns>
        /// <response code="200">Lấy giỏ hàng thành công</response>
        [HttpGet]
        public async Task<ActionResult<CartResponseDto>> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User not logged in");

            var cart = await _context.Carts
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItems = await _context.CartDetails
                .Where(cd => cd.CartId == cart.Id)
                .Include(cd => cd.Product)
                .Select(cd => new CartDetailResponseDto
                {
                    ProductId = cd.ProductId,
                    ProductName = cd.Product.Name,
                    Price = cd.Product.Price,
                    Quantity = cd.Quantity,
                    ImageUrl = cd.Product.ImageUrl
                })
                .ToListAsync();

            decimal total = cartItems.Sum(c => c.Total);

            var response = new CartResponseDto
            {
                Items = cartItems,
                Total = total
            };

            return Ok(response);
        }

        // =====================================================================
        // DELETE ITEM
        // =====================================================================

        /// <summary>
        /// Xóa product khỏi giỏ hàng (xóa vĩnh viễn).
        /// </summary>
        /// <param name="id">ID cart detail cần xóa.</param>
        /// <returns>Giỏ hàng sau khi xóa.</returns>
        /// <response code="200">Xóa thành công</response>
        /// <response code="404">Product không tồn tại trong giỏ</response>
        [HttpDelete("product/{productId}")]
        public async Task<IActionResult> RemoveProduct(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User not logged in");

            var cart = await _context.Carts
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (cart == null)
                return NotFound("Cart not found");

            var cartDetail = await _context.CartDetails
                .FirstOrDefaultAsync(cd => cd.CartId == cart.Id && cd.ProductId == productId);

            if (cartDetail == null)
                return NotFound("Product not found in cart");

            _context.CartDetails.Remove(cartDetail);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("cart")]
        public async Task<IActionResult> DeleteCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User not logged in");

            var cart = await _context.Carts
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (cart == null)
                return NotFound("Cart not found");

            var cartItems = await _context.CartDetails
                .Where(cd => cd.CartId == cart.Id)
                .ToListAsync();

            if (cartItems.Any())
                _context.CartDetails.RemoveRange(cartItems);

            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cart deleted successfully" });
        }
    }
}
