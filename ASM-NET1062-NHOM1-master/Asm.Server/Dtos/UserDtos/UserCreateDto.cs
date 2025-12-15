using Asm.Server.Models;

namespace Asm.Server.Dtos.UserDtos
{
	public class UserCreateDto
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string FullName { get; set; }
		public string Address { get; set; }

        public string? PhoneNumber { get; set; }
        public string Role { get; set; }
		public UserStatus Status { get; set; }
	}
}
