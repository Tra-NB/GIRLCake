using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Asm.Server.Models
{
	public class ComboFood
	{
		public int Id { get; set; }
		public int FoodId { get; set; }
		public int ComboId { get; set; }
		public int Quantity { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(ComboId))]
		public Combo? Combo { get; set; }

		[ForeignKey(nameof(FoodId))]
		public Food? Food { get; set; }
	}
}
