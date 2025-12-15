using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace Asm.Server.Models
{
	public class AppUser : IdentityUser
	{
		public string Address { get; set; }
		public string FullName{ get; set; }
		public UserStatus Status { get; set; } = UserStatus.Active;
		public DateTime CreateAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
	}

	public enum UserStatus
	{
		Active = 1, // người dùng hoạt động bình thường
		Inactive = 2, // khách tạm nghỉ hoặc bạn khóa tài khoản do yêu cầu của họ (xóa nhưng ko muốn mất dữ liệu
		Banned = 3, // khóa tài khoản vi phạm (bom hàng,...)
		Pending = 4 // tài khoản vừa đăng ký chưa xác thực số điện thoại/email
	}
}
