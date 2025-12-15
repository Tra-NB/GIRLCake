namespace Asm.Server.Dtos.OrderDtos
{
    public class OrderCreateDto
    {
        public int Id { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; 
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; 
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; } 

        public int? VoucherId { get; set; }
        public string? VoucherCode { get; set; }

        public List<OrderDetailDto>? OrderDetails { get; set; }
    }
}
