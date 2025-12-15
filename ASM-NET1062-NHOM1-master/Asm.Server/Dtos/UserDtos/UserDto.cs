using Asm.Server.Models;

namespace Asm.Server.Dtos.UserDtos
{
	public class UserDto
	{
		public string Id { get; set; }
		public string Email { get; set; }
		public string FullName { get; set; }
		public string Address { get; set; }
        public string PhoneNumber { get; set; }         
        public bool PhoneNumberConfirmed { get; set; }
        public UserStatus Status { get; set; }
		public string Role { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
	}
}
