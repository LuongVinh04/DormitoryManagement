using Dormitory.Models.DataContexts;
using Dormitory.Models.DTOS.DataRequest;
using Dormitory.Models.DTOS.DataResponse;
using DormitoryManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

namespace DormitoryManagement.Controllers.Shared
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext db, JwtService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _db.Users
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                .FirstOrDefaultAsync(x => x.Username == request.Username);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });
            }

            bool isPasswordValid = false;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            }
            catch
            {
                // To support legacy unhashed passwords during initial tests (fallback)
                isPasswordValid = request.Password == user.PasswordHash;
                if (!isPasswordValid) return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });
            }

            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });
            }

            // Aggregate permissions
            var rolePerms = user.Role?.RolePermissions.Select(rp => rp.Permission.Code) ?? Enumerable.Empty<string>();
            
            // Apply overrides (if IsGranted == false, remove. If true, add)
            var userPermsGranted = user.UserPermissions.Where(up => up.IsGranted).Select(up => up.Permission.Code);
            var userPermsRevoked = user.UserPermissions.Where(up => !up.IsGranted).Select(up => up.Permission.Code);

            var finalPermissions = rolePerms.Except(userPermsRevoked).Union(userPermsGranted).Distinct().ToList();

            var token = _jwtService.GenerateToken(user, finalPermissions);

            return Ok(new LoginResponse
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    RoleName = user.Role?.Name ?? ""
                },
                Permissions = finalPermissions
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var user = await _db.Users
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || !user.IsActive) return Unauthorized();

            var rolePerms = user.Role?.RolePermissions.Select(rp => rp.Permission.Code) ?? Enumerable.Empty<string>();
            var userPermsGranted = user.UserPermissions.Where(up => up.IsGranted).Select(up => up.Permission.Code);
            var userPermsRevoked = user.UserPermissions.Where(up => !up.IsGranted).Select(up => up.Permission.Code);

            var finalPermissions = rolePerms.Except(userPermsRevoked).Union(userPermsGranted).Distinct().ToList();

            return Ok(new
            {
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    RoleName = user.Role?.Name ?? ""
                },
                Permissions = finalPermissions
            });
        }
    }
}
