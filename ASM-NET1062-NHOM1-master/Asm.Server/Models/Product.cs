using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Asm.Server.Models
{
	public abstract class Product
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public string? ImageUrl { get; set; }
		public bool IsAvailable { get; set; } = true;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }

		public List<Review>? reviews { get; set; }
	}
}
