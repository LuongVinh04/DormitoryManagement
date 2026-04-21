using Dormitory.Models.DataContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Controllers;

[ApiController]
[Route("api/lookups")]
public class LookupsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var buildings = await db.Buildings
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Code })
            .ToListAsync();

        var rooms = await db.Rooms
            .Include(x => x.Building)
            .OrderBy(x => x.RoomNumber)
            .Select(x => new
            {
                x.Id,
                x.RoomNumber,
                x.BuildingId,
                buildingName = x.Building!.Name,
                x.Capacity,
                x.CurrentOccupancy
            })
            .ToListAsync();

        var students = await db.Students
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.StudentCode, x.RoomId })
            .ToListAsync();

        var roles = await db.Roles
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();

        var utilities = await db.Utilities
            .OrderByDescending(x => x.BillingMonth)
            .Select(x => new { x.Id, x.RoomId, x.BillingMonth })
            .ToListAsync();

        return Ok(new { buildings, rooms, students, roles, utilities });
    }
}
