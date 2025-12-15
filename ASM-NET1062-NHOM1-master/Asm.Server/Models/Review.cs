using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.Server.Models
{
	public class Review
	{
		public int Id { get; set; }
		public int Rating { get; set; }
		public string Comment { get; set; } = string.Empty;
		
		public string UserId { get; set; } = string.Empty;
		[ForeignKey("UserId")]
		public AppUser? User { get; set; }

		public int ProductId { get; set; }
		[ForeignKey("ProductId")]
		public Product? Product { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
	}
}
