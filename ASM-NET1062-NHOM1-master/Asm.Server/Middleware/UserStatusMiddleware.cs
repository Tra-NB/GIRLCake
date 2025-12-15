using Asm.Server.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Asm.Server.Middleware
{
    public class UserStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public UserStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<AppUser> userManager)
        {
            // 1. Chỉ kiểm tra nếu user đã đăng nhập (có Token hợp lệ)
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                // 2. Lấy ID user từ Token
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userId))
                {
                    // 3. Tra cứu user trong Database (để lấy trạng thái mới nhất)
                    var user = await userManager.FindByIdAsync(userId);

                    // 4. ĐIỀU KIỆN CHẶN:
                    // - Không tìm thấy user
                    // - User đã bị xóa mềm (DeletedAt != null)
                    // - Status KHÔNG PHẢI LÀ ACTIVE (tức là Inactive, Banned, Pending đều chặn)
                    if (user == null || user.DeletedAt != null || user.Status != UserStatus.Active)
                    {
                        // -> Trả về lỗi 401 Unauthorized ngay lập tức
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Account is locked or inactive.");
                        return; // ⛔ Dừng lại, không cho đi tiếp vào Controller
                    }
                }
            }

            // Nếu ổn thì cho đi tiếp
            await _next(context);
        }
    }
}
