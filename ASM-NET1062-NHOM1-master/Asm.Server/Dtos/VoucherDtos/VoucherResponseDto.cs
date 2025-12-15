using Asm.Server.Models;

namespace Asm.Server.Dtos.VoucherDtos
{
    public class VoucherResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public bool IsActive { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
