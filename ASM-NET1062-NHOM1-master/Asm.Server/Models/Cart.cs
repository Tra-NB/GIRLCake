using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.Server.Models
{
	public class Cart
	{
		public int Id { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public string UserId { get; set; }
		[ForeignKey(nameof(UserId))]
		public AppUser? User { get; set; }

		public ICollection<CartDetail>? CartDetails { get; set; }
	}

}
