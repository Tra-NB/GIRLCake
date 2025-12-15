namespace Asm.Server.Dtos.CartDtos
{
	public class CartVoucherResponseDto
	{
		public decimal Discount { get; set; }
		public decimal NewTotal { get; set; }
		public int VoucherId { get; set; }
		public string Description { get; set; }
	}
}
