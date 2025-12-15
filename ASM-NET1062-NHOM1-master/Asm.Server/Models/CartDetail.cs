using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Asm.Server.Models
{
	public class CartDetail
	{
		public int Id { get; set; }
		public int Quantity { get; set; }

		public int CartId { get; set; }
		public int ProductId { get; set; }
		[ForeignKey(nameof(ProductId))]
		public Product? Product { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(CartId))]
		public Cart? Cart { get; set; }

	}
}
