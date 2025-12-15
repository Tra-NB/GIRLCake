using Asm.Server.Models;

namespace Asm.Server.Dtos.OrderDtos
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string ShippingAddress { get; set; }
        public OrderStatus Status { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        // Thêm thông tin user
        public string UserId { get; set; }
        public string UserName { get; set; }

        // Thông tin voucher
        public int? VoucherId { get; set; }
        public string VoucherCode { get; set; }

        // Thông tin chi tiết đơn hàng (nếu cần)
        public List<OrderDetailDto> OrderDetails { get; set; } = new List<OrderDetailDto>();


    }
}
