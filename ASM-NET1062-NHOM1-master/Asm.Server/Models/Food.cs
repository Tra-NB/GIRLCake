using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Asm.Server.Models
{
	public class Food : Product
	{
		public int CategoryId { get; set; }
		[ForeignKey(nameof(CategoryId))]
		public Category? Category { get; set; }

		[JsonIgnore]
		public ICollection<ComboFood>? ComboFoods { get; set; }
	}
}
