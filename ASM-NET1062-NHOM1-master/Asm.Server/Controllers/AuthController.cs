using Asm.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Asm.Server.Dtos.AuthDtos;
namespace Asm.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IConfiguration _config;

		private readonly UserManager<AppUser> _userManager;

		public AuthController(IConfiguration configuration, UserManager<AppUser> userManager)
		{
			_config = configuration;
			_userManager = userManager;
		}

        /// <summary>
        ﻿/// Đăng nhập bằng Google và trả về JWT token cùng thông tin người dùng.
        /// </summary>
        /// <param name="googleLoginRequest">IdToken được gửi từ phía client sau khi đăng nhập Google.</param>
        /// <returns>Trả về token và thông tin người dùng.</returns>
        /// <response code="200">Đăng nhập thành công, trả về JWT và dữ liệu người dùng.</response>
        /// <response code="400">IdToken không hợp lệ hoặc tạo tài khoản thất bại.</response>
        /// <response code="401">Tài khoản tồn tại nhưng đã bị xóa.</response>
        [HttpPost("google-login")]
        public async Task<ActionResult<LoginResponse>> GoogleLogin([FromBody] GoogleLoginRequest googleLoginRequest)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(googleLoginRequest.IdToken);
                var user = await _userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    // TẠO TÀI KHOẢN MỚI QUA GOOGLE
                    user = new AppUser
                    {
                        UserName = payload.Email,
                        Email = payload.Email,
                        FullName = payload.Name,
                        Address = "",
                        // 🔥 Đảm bảo Status là ACTIVE (1) khi tạo mới
                        Status = UserStatus.Active,
                    };
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        return BadRequest(new { message = "User creation failed.", errors = result.Errors });
                    }

                    await _userManager.AddToRoleAsync(user, "User");
                }
                else
                {
                    // XỬ LÝ TÀI KHOẢN ĐÃ TỒN TẠI
                    if (user.DeletedAt != null)
                    {
                        return Unauthorized(new { message = "Tài khoản này đã bị xóa" });
                    }

                    if (user.Status != UserStatus.Active)
                    {
                        string msg = user.Status == UserStatus.Banned ? "Tài khoản bị cấm do vi phạm." :
                                     user.Status == UserStatus.Pending ? "Tài khoản chưa kích hoạt." :
                                     "Tài khoản đang tạm khóa.";

                        // 🔥 Trả về thông báo lỗi cụ thể dưới dạng JSON object
                        return Unauthorized(new { message = msg });
                    }
                }

                // Nếu tài khoản hợp lệ (không bị xóa và Status == Active)
                var loginResponse = await BuildLoginResponseAsync(user);

                return Ok(loginResponse);
            }
            catch (Exception error)
            {
                return BadRequest(new { message = "Invalid Google token", error = error.Message });
            }
        }

        /// <summary>
        ﻿/// Đăng ký tài khoản mới.
        /// </summary>
        /// <returns>Trả về thông tin người dùng sau khi đăng ký thành công.</returns>
        /// <response code="200">Đăng ký thành công và trả về thông tin người dùng.</response>
        /// <response code="400">
        /// Email đã tồn tại, hoặc dữ liệu đăng ký không hợp lệ,
        /// hoặc quá trình tạo tài khoản thất bại.
        /// </response>
        /// <response code="500">Lỗi hệ thống trong quá trình đăng ký.</response>
        [HttpPost("register")]
		public async Task<ActionResult<UserLoginResponse>> Register([FromBody] RegisterDto register)
		{
			try
			{
				var user = await _userManager.FindByEmailAsync(register.Email);
				if (user != null)
				{
					return BadRequest(new { message = "User already exists." });
				}

				user = new AppUser
				{
					UserName = register.Email,
					Email = register.Email,
					FullName = register.FullName,
					Address = register.Address ?? ""
				};

				var result = await _userManager.CreateAsync(user, register.Password);
				if (!result.Succeeded)
				{
					return BadRequest(new { message = "User creation failed.", errors = result.Errors });
				}

				await _userManager.AddToRoleAsync(user, "User");

				var userRoles = await _userManager.GetRolesAsync(user);

				return Ok(new UserLoginResponse
					{
						Id = user.Id,
						Email = user.Email,
						FullName = user.FullName,
						Role = userRoles.FirstOrDefault()
					}
				);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = "An error occurred during registration.", error = ex.Message });
			}
		}

		/// <summary>
		/// Đăng nhập bằng email và mật khẩu.
		/// </summary>
		/// <param name="loginDto">Chứa email và mật khẩu người dùng.</param>
		/// <returns>Trả về token JWT và thông tin người dùng.</returns>
		/// <response code="200">Đăng nhập thành công, trả về token và thông tin người dùng.</response>
		/// <response code="400">Có lỗi xảy ra trong quá trình đăng nhập.</response>
		/// <response code="401">Email hoặc mật khẩu không hợp lệ, hoặc tài khoản đã bị xóa.</response>
		[HttpPost("login")]
		public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginDto loginDto)
		{
			try
			{
				var user = await _userManager.FindByEmailAsync(loginDto.Email);
				if (user == null)
					return Unauthorized(new { message = "Invalid email or password." });

				if (user.DeletedAt != null)
					return Unauthorized(new { message = "Tài khoản này đã bị xóa" });

				var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
				if (!passwordValid)
					return Unauthorized(new { message = "Invalid email or password." });


                if (user.Status != UserStatus.Active)
                {
                    string msg = user.Status == UserStatus.Banned ? "Tài khoản bị cấm do vi phạm." :
                                 // ...
                                 "Tài khoản đang tạm khóa.";

                    // 🔥 TRẢ VỀ JSON OBJECT { message: msg }
                    return Unauthorized(new { message = msg });
                }

                var loginResponse = await BuildLoginResponseAsync(user);

				return Ok(loginResponse);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = "An error occurred during login.", error = ex.Message });
			}
		}

		private async Task<LoginResponse> BuildLoginResponseAsync(AppUser user)
		{
			var userRoles = await _userManager.GetRolesAsync(user);

			var claims = new List<Claim>
				{
					new Claim(ClaimTypes.NameIdentifier, user.Id),
					new Claim(ClaimTypes.Email, user.Email),
					new Claim(ClaimTypes.Name, user.FullName),
				};

			foreach (var role in userRoles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddHours(1),
				signingCredentials: creds
			);

			var jwt = new JwtSecurityTokenHandler().WriteToken(token);

			return (new LoginResponse
			{
				Token = jwt,
				User = new UserLoginResponse
				{
					Id = user.Id,
					Email = user.Email,
					FullName = user.FullName,
					Role = userRoles.FirstOrDefault()
				}
			});
		}
	}
}
