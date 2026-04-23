using Dormitory.Models.DataContexts;
using Dormitory.Models.Entities;
using DormitoryManagement.Models;
using DormitoryManagement.Services.Facilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Controllers.People;

[ApiController]
[Route("api/people")]
public class PeopleController(AppDbContext db) : ControllerBase
{
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        var data = await db.Students
            .Include(x => x.Room)
            .ThenInclude(x => x!.Building)
            .OrderBy(x => x.StudentCode)
            .Select(x => new
            {
                x.Id,
                x.StudentCode,
                x.Name,
                x.Gender,
                x.DateOfBirth,
                x.Phone,
                x.Email,
                x.Faculty,
                x.ClassName,
                x.Address,
                x.EmergencyContact,
                x.Status,
                x.RoomId,
                roomNumber = x.Room != null ? x.Room.RoomNumber : null,
                buildingName = x.Room != null ? x.Room.Building!.Name : null,
                createdAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("students/{id:int}")]
    public async Task<IActionResult> GetStudentById(int id)
    {
        var student = await db.Students
            .Include(x => x.Room)
            .ThenInclude(x => x!.Building)
            .Include(x => x.Contracts.OrderByDescending(c => c.StartDate))
            .Include(x => x.Invoices.OrderByDescending(i => i.BillingMonth))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (student is null) return NotFound();

        return Ok(new
        {
            student = new
            {
                student.Id,
                student.StudentCode,
                student.Name,
                student.Gender,
                student.DateOfBirth,
                student.Phone,
                student.Email,
                student.Faculty,
                student.ClassName,
                student.Address,
                student.EmergencyContact,
                student.Status,
                student.RoomId,
                roomNumber = student.Room?.RoomNumber,
                buildingName = student.Room?.Building?.Name
            },
            contracts = student.Contracts.Take(6).Select(x => new
            {
                x.Id,
                x.ContractCode,
                x.MonthlyFee,
                x.StartDate,
                x.EndDate,
                x.Status
            }),
            invoices = student.Invoices.Take(8).Select(x => new
            {
                x.Id,
                x.InvoiceCode,
                x.Total,
                x.Status,
                x.DueDate,
                x.PaidDate
            })
        });
    }

    [HttpPost("students")]
    public async Task<IActionResult> CreateStudent([FromBody] StudentRequest request)
    {
        var entity = new Students
        {
            StudentCode = request.StudentCode.Trim(),
            Name = request.Name.Trim(),
            Gender = request.Gender.Trim(),
            DateOfBirth = request.DateOfBirth,
            Phone = request.Phone.Trim(),
            Email = request.Email.Trim(),
            Faculty = request.Faculty.Trim(),
            ClassName = request.ClassName.Trim(),
            Address = request.Address.Trim(),
            EmergencyContact = request.EmergencyContact.Trim(),
            Status = request.Status.Trim(),
            RoomId = request.RoomId
        };

        db.Students.Add(entity);
        await db.SaveChangesAsync();

        if (entity.RoomId.HasValue)
        {
            await RoomOccupancyService.RecalculateRoomAsync(db, entity.RoomId.Value);
            await db.SaveChangesAsync();
        }

        return Ok(entity);
    }

    [HttpPut("students/{id:int}")]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] StudentRequest request)
    {
        var entity = await db.Students.FindAsync(id);
        if (entity is null) return NotFound();

        var oldRoomId = entity.RoomId;
        entity.StudentCode = request.StudentCode.Trim();
        entity.Name = request.Name.Trim();
        entity.Gender = request.Gender.Trim();
        entity.DateOfBirth = request.DateOfBirth;
        entity.Phone = request.Phone.Trim();
        entity.Email = request.Email.Trim();
        entity.Faculty = request.Faculty.Trim();
        entity.ClassName = request.ClassName.Trim();
        entity.Address = request.Address.Trim();
        entity.EmergencyContact = request.EmergencyContact.Trim();
        entity.Status = request.Status.Trim();
        entity.RoomId = request.RoomId;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        if (oldRoomId.HasValue)
        {
            await RoomOccupancyService.RecalculateRoomAsync(db, oldRoomId.Value);
        }

        if (entity.RoomId.HasValue)
        {
            await RoomOccupancyService.RecalculateRoomAsync(db, entity.RoomId.Value);
        }

        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPatch("students/{id:int}/status")]
    public async Task<IActionResult> UpdateStudentStatus(int id, [FromBody] StudentStatusRequest request)
    {
        var entity = await db.Students.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Status = request.Status.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(new { message = "Đã cập nhật trạng thái sinh viên." });
    }

    [HttpDelete("students/{id:int}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var entity = await db.Students.FindAsync(id);
        if (entity is null) return NotFound();

        var roomId = entity.RoomId;
        db.Students.Remove(entity);
        await db.SaveChangesAsync();

        if (roomId.HasValue)
        {
            await RoomOccupancyService.RecalculateRoomAsync(db, roomId.Value);
            await db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        return Ok(await db.Roles.OrderBy(x => x.Name).ToListAsync());
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] RoleRequest request)
    {
        var entity = new Roles
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim()
        };

        db.Roles.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("roles/{id:int}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleRequest request)
    {
        var entity = await db.Roles.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Name = request.Name.Trim();
        entity.Description = request.Description.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("roles/{id:int}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var inUse = await db.Users.AnyAsync(x => x.RoleId == id);
        if (inUse)
        {
            return BadRequest(new { message = "Vai trò đang được sử dụng." });
        }

        var entity = await db.Roles.FindAsync(id);
        if (entity is null) return NotFound();

        db.Roles.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var data = await db.Users
            .Include(x => x.Role)
            .OrderBy(x => x.Username)
            .Select(x => new
            {
                x.Id,
                x.Username,
                x.FullName,
                x.Email,
                x.RoleId,
                roleName = x.Role!.Name,
                x.IsActive,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("users/{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await db.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user is null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.FullName,
            user.Email,
            user.RoleId,
            roleName = user.Role!.Name,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt
        });
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] UserRequest request)
    {
        var entity = new Users
        {
            Username = request.Username.Trim(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = request.PasswordHash.Trim(),
            RoleId = request.RoleId,
            IsActive = request.IsActive
        };

        db.Users.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserRequest request)
    {
        var entity = await db.Users.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Username = request.Username.Trim();
        entity.FullName = request.FullName.Trim();
        entity.Email = request.Email.Trim();
        entity.PasswordHash = request.PasswordHash.Trim();
        entity.RoleId = request.RoleId;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("users/{id:int}/profile")]
    public async Task<IActionResult> UpdateUserProfile(int id, [FromBody] UserProfileRequest request)
    {
        var entity = await db.Users.FindAsync(id);
        if (entity is null) return NotFound();

        entity.FullName = request.FullName.Trim();
        entity.Email = request.Email.Trim();
        if (!string.IsNullOrWhiteSpace(request.PasswordHash))
        {
            entity.PasswordHash = request.PasswordHash.Trim();
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(new { message = "Đã cập nhật hồ sơ người dùng." });
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var entity = await db.Users.FindAsync(id);
        if (entity is null) return NotFound();

        db.Users.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("roles/{id:int}/permissions")]
    public async Task<IActionResult> GetRolePermissions(int id)
    {
        var role = await db.Roles.FindAsync(id);
        if (role == null) return NotFound();

        var permissionIds = await db.RolePermissions
            .Where(x => x.RoleId == id)
            .Select(x => x.PermissionId)
            .ToListAsync();

        return Ok(permissionIds);
    }

    [HttpPut("roles/{id:int}/permissions")]
    public async Task<IActionResult> UpdateRolePermissions(int id, [FromBody] UpdateRolePermissionsRequest request)
    {
        var role = await db.Roles.FindAsync(id);
        if (role == null) return NotFound();

        var existing = await db.RolePermissions.Where(x => x.RoleId == id).ToListAsync();
        db.RolePermissions.RemoveRange(existing);

        var newPerms = request.PermissionIds.Distinct().Select(pid => new RolePermissions
        {
            RoleId = id,
            PermissionId = pid
        });

        db.RolePermissions.AddRange(newPerms);
        await db.SaveChangesAsync();

        return Ok(new { message = "Cập nhật quyền của vai trò thành công." });
    }

    [HttpGet("users/{id:int}/permissions")]
    public async Task<IActionResult> GetUserPermissions(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();

        var granted = await db.UserPermissions
            .Where(x => x.UserId == id && x.IsGranted)
            .Select(x => x.PermissionId)
            .ToListAsync();

        var denied = await db.UserPermissions
            .Where(x => x.UserId == id && !x.IsGranted)
            .Select(x => x.PermissionId)
            .ToListAsync();

        return Ok(new { allowedPermissionIds = granted, deniedPermissionIds = denied });
    }

    [HttpPut("users/{id:int}/permissions")]
    public async Task<IActionResult> UpdateUserPermissions(int id, [FromBody] UpdateUserPermissionsRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();

        var existing = await db.UserPermissions.Where(x => x.UserId == id).ToListAsync();
        db.UserPermissions.RemoveRange(existing);

        var addedList = new List<UserPermissions>();

        foreach (var pid in request.AllowedPermissionIds.Distinct())
        {
            addedList.Add(new UserPermissions { UserId = id, PermissionId = pid, IsGranted = true });
        }

        foreach (var pid in request.DeniedPermissionIds.Distinct())
        {
            // If already allowed, skip denying to prevent conflict
            if (!request.AllowedPermissionIds.Contains(pid))
            {
                addedList.Add(new UserPermissions { UserId = id, PermissionId = pid, IsGranted = false });
            }
        }

        db.UserPermissions.AddRange(addedList);
        await db.SaveChangesAsync();

        return Ok(new { message = "Cập nhật quyền đặc biệt của người dùng thành công." });
    }

    [HttpPut("users/{id:int}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();

        var role = await db.Roles.FindAsync(request.RoleId);
        if (role == null) return BadRequest(new { message = "Vai trò không hợp lệ." });

        user.RoleId = role.Id;
        user.UpdatedAt = DateTime.UtcNow;
        
        // When changing roles, might want to clear user-specific overrides.
        var existingOverrides = await db.UserPermissions.Where(x => x.UserId == id).ToListAsync();
        db.UserPermissions.RemoveRange(existingOverrides);

        await db.SaveChangesAsync();

        return Ok(new { message = "Cập nhật vai trò người dùng thành công." });
    }
}
