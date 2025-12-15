namespace Asm.Server.Dtos.ReviewDtos
{
	public class ReviewUpdateDto
	{
		public int Rating { get; set; }
		public string Comment { get; set; } = string.Empty;
	}
}
