using Asm.Server.Dtos.UserDtos;
using Asm.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Asm.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly UserManager<AppUser> _userManager;

		public UsersController(UserManager<AppUser> userManager)
		{
			_userManager = userManager;
		}

		/// <summary>
		/// Lấy danh sách tất cả người dùng (dành cho admin).
		/// </summary>
		/// <returns>Trả về danh sách user kèm thông tin role và DeletedAt.</returns>
		/// <response code="200">Danh sách người dùng được trả về thành công.</response>
		/// <response code="403">Không có quyền thực hiện hành động này.</response>
		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
		{
			var users = await _userManager.Users.ToListAsync();


			var usersWithRoles = new List<UserDto>();
			foreach (var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);
				usersWithRoles.Add(new UserDto
				{
					Id = user.Id,
					Email = user.Email,
					FullName = user.FullName,
					Address = user.Address,
					Status = user.Status,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    Role = roles.FirstOrDefault(),
					CreatedAt = user.CreateAt,
					UpdatedAt = user.UpdatedAt,
					DeletedAt = user.DeletedAt,
				});
			}



			return Ok(usersWithRoles);
		}

		/// <summary>
		/// Lấy thông tin một user theo ID (dành cho admin).
		/// </summary>
		/// <param name="id">ID của người dùng cần lấy thông tin.</param>
		/// <returns>Trả về thông tin user nếu tồn tại.</returns>
		/// <response code="200">Trả về thông tin user thành công.</response>
		/// <response code="404">Không tìm thấy user với ID tương ứng.</response>
		/// <response code="403">Không có quyền thực hiện hành động này.</response>
		[HttpGet("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<UserDto>> GetUser(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
				return NotFound();

			var roles = await _userManager.GetRolesAsync(user);

			var userWithRole = new UserDto()
			{
				Id = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				Address = user.Address,
				Status = user.Status,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                Role = roles.FirstOrDefault(),
				CreatedAt = user.CreateAt,
				UpdatedAt = user.UpdatedAt,
				DeletedAt = user.DeletedAt,
			};

			return Ok(userWithRole);
		}

		/// <summary>
		/// Lấy thông tin profile của người dùng hiện tại.
		/// </summary>
		/// <returns>Trả về thông tin người dùng hiện tại.</returns>
		/// <response code="200">Trả về thông tin user thành công.</response>
		/// <response code="404">Không tìm thấy người dùng tương ứng với JWT hoặc người dùng đã bị xóa.</response>
		[HttpGet("me")]
		public async Task<ActionResult<UserDto>> GetProfile()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
				return Unauthorized("User not logged in");

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null || user.DeletedAt != null)
				return NotFound();

			var roles = await _userManager.GetRolesAsync(user);

			var userWithRole = new UserDto()
			{
				Id = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				Address = user.Address,
				Status = user.Status,
				Role = roles.FirstOrDefault(),
				CreatedAt = user.CreateAt,
				UpdatedAt = user.UpdatedAt,
				DeletedAt = user.DeletedAt,
                PhoneNumber = user.PhoneNumber
            };

			return Ok(userWithRole);
		}

		/// <summary>
		/// Tạo một người dùng mới (dành cho admin).
		/// </summary>
		/// <param name="model">Dữ liệu tạo user mới.</param>
		/// <returns>Trả về thông báo tạo user thành công hoặc lỗi.</returns>
		/// <response code="200">Tạo user thành công.</response>
		/// <response code="400">Có lỗi xảy ra trong quá trình tạo user.</response>
		/// <response code="403">Không có quyền thực hiện hành động này.</response>
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<string>> PostUser([FromBody] UserCreateDto model)
		{
			var user = new AppUser
			{
				UserName = model.Email,
				Email = model.Email,
				FullName = model.FullName,
				Address = model.Address,
				PhoneNumber = model.PhoneNumber,

                Status = model.Status,
			};

			var result = await _userManager.CreateAsync(user, model.Password);
			if (!result.Succeeded)
			{
				return BadRequest(result.Errors);
			}

			if (!string.IsNullOrEmpty(model.Role))
			{
				await _userManager.AddToRoleAsync(user, model.Role);
			}

			return Ok(new { message = "User created successfully by admin." });
		}

		/// <summary>
		/// Cập nhật thông tin của một người dùng (dành cho admin).
		/// </summary>
		/// <param name="id">ID của người dùng cần cập nhật.</param>
		/// <param name="model">Dữ liệu cập nhật.</param>
		/// <returns>Trả về thông báo thành công</returns>
		/// <response code="200">Cập nhật thành công.</response>
		/// <response code="400">Lỗi trong quá trình cập nhật thông tin hoặc role.</response>
		/// <response code="404">Không tìm thấy người dùng với ID tương ứng.</response>
		/// <response code="403">Không có quyền thực hiện hành động này.</response>
		[HttpPatch("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<string>> PatchUser(string id, [FromBody] UserUpdateDto model)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
				return NotFound();

			if (!string.IsNullOrEmpty(model.FullName))
				user.FullName = model.FullName;

			if (!string.IsNullOrEmpty(model.Address))
				user.Address = model.Address;

            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                // Cập nhật số điện thoại
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    return BadRequest(setPhoneResult.Errors);
                }
                // Khi đổi số mới, mặc định PhoneNumberConfirmed = false
            }

            if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                    return BadRequest(setEmailResult.Errors);

            }
            user.UpdatedAt = DateTime.Now;

            if (model.Status.HasValue)
				user.Status = model.Status.Value;

			var updateResult = await _userManager.UpdateAsync(user);
			if (!updateResult.Succeeded)
				return BadRequest(updateResult.Errors);

			if (!string.IsNullOrEmpty(model.Role)) {
				var currentRoles = await _userManager.GetRolesAsync(user);
				var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
				if (!removeResult.Succeeded)
					return BadRequest(removeResult.Errors);

				var addResult = await _userManager.AddToRoleAsync(user, model.Role);
				if (!addResult.Succeeded)
					return BadRequest(addResult.Errors);
			}

			return Ok(new { message = "User updated successfully." });
		}

		/// <summary>
		/// Cập nhật thông tin cá nhân của chính người dùng.
		/// </summary>
		/// <remarks>
		/// Đối với việc đổi mật khẩu:
		/// - Nếu người dùng đã có mật khẩu, phải cung cấp `PasswordOld`.
		/// - Nếu người dùng chưa có mật khẩu, chỉ cần `PasswordNew`.
		/// </remarks>
		/// <returns>Trả về thông báo thành công và có thể trả về thông tin người dùng cập nhật.</returns>
		/// <response code="200">Cập nhật thành công.</response>
		/// <response code="400">
		/// Một số trường hợp lỗi xảy ra:
		/// - Dữ liệu gửi lên không hợp lệ.
		/// - PasswordOld không đúng khi đổi mật khẩu.
		/// - Lỗi trong quá trình update user hoặc password.
		/// </response>
		/// <response code="404">Không tìm thấy người dùng tương ứng với JWT hoặc người dùng đã bị xóa.</response>
		[HttpPatch("me")]
		public async Task<ActionResult<string>> PatchProfile([FromBody] ProfileUpdateDto model)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null || user.DeletedAt != null)
				return NotFound(new { message = "User not found." });

			if (!string.IsNullOrEmpty(model.FullName))
				user.FullName = model.FullName;

			if (!string.IsNullOrEmpty(model.Address))
				user.Address = model.Address;

            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    return BadRequest(setPhoneResult.Errors);
                }
                // PhoneNumberConfirmed = false, cần xác thực bằng OTP
            }
            user.UpdatedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
				return BadRequest(result.Errors);

			if (!string.IsNullOrEmpty(model.PasswordNew))
			{
				var hasPasword = await _userManager.HasPasswordAsync(user);
				IdentityResult passwordResult;

				if (hasPasword)
				{
					if (string.IsNullOrEmpty(model.PasswordOld))
					{
						return BadRequest(new { message = "Old password is required to change the password." });
					}
					passwordResult = await _userManager.ChangePasswordAsync(user, model.PasswordOld, model.PasswordNew);
				}
				else
				{
					passwordResult = await _userManager.AddPasswordAsync(user, model.PasswordNew);
				}

				if (!passwordResult.Succeeded)
				{
					return BadRequest(passwordResult.Errors);
				}
			}

			return Ok(new { message = "Profile updated successfully." });
		}

		/// <summary>
		/// Xóa tạm thời một người dùng (dành cho admin).
		/// </summary>
		/// <remarks>
		/// Hành động này chỉ đánh dấu đánh giá là đã bị xóa (soft delete),
		/// không xóa hoàn toàn khỏi cơ sở dữ liệu.
		/// </remarks>
		/// <param name="id">ID của người dùng cần xóa.</param>
		/// <returns>Trả về thông báo thành công hoặc lỗi.</returns>
		/// <response code="200">User bị xóa thành công.</response>
		/// <response code="400">Có lỗi xảy ra trong quá trình xóa.</response>
		/// <response code="404">Không tìm thấy user với ID tương ứng.</response>
		/// <response code="403">Không có quyền thực hiện hành động này.</response>
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<string>> DeleteUser(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			if (user.DeletedAt != null)
				return BadRequest(new { message = "User is already deleted." });

			user.DeletedAt = DateTime.UtcNow;

			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				return BadRequest(result.Errors);
			}
			return Ok(new { message = "User deleted successfully." });
		}

		/// <summary>
		/// Khôi phục một user đã bị xóa mềm - soft delete (dành cho admin).
		/// </summary>
		/// <param name="id">ID của người dùng cần khôi phục.</param>
		/// <returns>Trả về thông báo thành công hoặc lỗi.</returns>
		/// <response code="200">User được khôi phục thành công.</response>
		/// <response code="400">User chưa bị xóa hoặc có lỗi trong quá trình khôi phục.</response>
		/// <response code="404">Không tìm thấy user với ID tương ứng.</response>
		/// <response code="403">Không có quyền thực hiện hành động này.</response>
		[Authorize(Roles = "Admin")]
		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<string>> RestoreUser(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
				return NotFound();

			if (user.DeletedAt == null)
				return BadRequest(new { message = "User is not deleted." });

			user.DeletedAt = null;

			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
				return BadRequest(result.Errors);

			return Ok(new { message = "User restored successfully." });
		}
	}
}
