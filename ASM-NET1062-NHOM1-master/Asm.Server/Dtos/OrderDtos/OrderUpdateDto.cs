using Asm.Server.Models;

namespace Asm.Server.Dtos.OrderDtos
{
    public class OrderUpdateDto
    {
        public OrderStatus Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
