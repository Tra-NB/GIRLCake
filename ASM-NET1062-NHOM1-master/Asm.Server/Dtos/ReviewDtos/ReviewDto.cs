using Asm.Server.Models;

namespace Asm.Server.Dtos.ReviewDtos
{
	public class ReviewDto
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string ProductName { get; set; }
        public int Rating { get; set; }
		public string Comment { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }

		public ReviewDto(Review review)
		{
			Id = review.Id;
			ProductId = review.ProductId;
			UserId = review.UserId;
			UserName = review.User != null ? review.User.UserName : string.Empty;
            FullName = review.User?.FullName ?? "Ẩn danh";
            ProductName = review.Product != null ? review.Product.Name : string.Empty;
            Rating = review.Rating;
			Comment = review.Comment;
			CreatedAt = review.CreatedAt;
			UpdatedAt = review.UpdatedAt;
			DeletedAt = review.DeletedAt;
		}

		public ReviewDto() { }
	}
}
