namespace Asm.Server.Dtos.AuthDtos
{
	public class LoginResponse
	{
		public string Token { get; set; }
		public UserLoginResponse User { get; set; }
	}

	public class UserLoginResponse {
		public string Id { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
		public string Role { get; set; }
	}
}
