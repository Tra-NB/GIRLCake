using Asm.Server.Dtos.Cart;

namespace Asm.Server.Dtos.CartDtos
{
    public class CartResponseDto
    {
		public List<CartDetailResponseDto> Items { get; set; } = new();
		public decimal Total { get; set; }
	}
}
