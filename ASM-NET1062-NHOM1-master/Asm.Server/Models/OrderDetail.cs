using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Asm.Server.Models
{
	public class OrderDetail
	{
		public int Id { get; set; }

		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
		public int OrderId { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(OrderId))]
		public Order? Order { get; set; }

		public int ProductId { get; set; }
		[ForeignKey(nameof(ProductId))]
		public Product? Product { get; set; }
	}
}
