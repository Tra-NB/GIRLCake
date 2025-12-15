using Asm.Server.Models;

namespace Asm.Server.Dtos.UserDtos
{
	public class UserUpdateDto
	{
		public string? Id { get; set; }
		public string? FullName { get; set; }
		public string? Address { get; set; }
		public UserStatus? Status { get; set; }
		public string? Role { get; set; }
        public string? PhoneNumber { get; set; }
        public string Email { get; set; }


    }
}
