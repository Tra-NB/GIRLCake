namespace Asm.Server.Models
{
	public class Voucher
	{
		public int Id { get; set; }
		public string Code { get; set; }
		public string Description { get; set; }
		public DiscountType DiscountType { get; set; }
		public decimal DiscountValue { get; set; }
		public bool IsActive { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int UsageLimit { get; set; } // Số lần sử dụng tối đa
		public int UsedCount { get; set; } // Số lần đã sử dụng
	}

	public enum DiscountType
	{
		Percentage,   // Giảm theo %
		FixedAmount   // Giảm theo số tiền cố định
	}
}
