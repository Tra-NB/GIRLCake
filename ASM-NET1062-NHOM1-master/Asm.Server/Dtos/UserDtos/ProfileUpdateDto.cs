using Asm.Server.Models;

namespace Asm.Server.Dtos.UserDtos
{
	public class ProfileUpdateDto
	{
		public string? Id { get; set; }
		public string? FullName { get; set; }
		public string? Address { get; set; }
		public string? PasswordOld { get; set; }
		public string? PasswordNew { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
