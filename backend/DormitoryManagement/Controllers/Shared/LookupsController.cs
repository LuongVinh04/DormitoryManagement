using Dormitory.Models.DataContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Controllers.Shared;

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
            .Include(x => x.RoomCategory)
            .Include(x => x.RoomZone)
            .OrderBy(x => x.RoomNumber)
            .Select(x => new
            {
                x.Id,
                x.RoomNumber,
                x.BuildingId,
                buildingName = x.Building!.Name,
                x.RoomCategoryId,
                roomCategoryName = x.RoomCategory == null ? "" : x.RoomCategory.Name,
                x.RoomZoneId,
                roomZoneName = x.RoomZone == null ? "" : x.RoomZone.Name,
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

        var roomCategories = await db.RoomCategories
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.DefaultCapacity,
                x.BaseMonthlyFee,
                x.DepositAmount,
                x.HygieneFee,
                x.ServiceFee,
                x.InternetFee,
                x.ElectricityUnitPrice,
                x.WaterUnitPrice
            })
            .ToListAsync();

        var roomZones = await db.RoomZones
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new { x.Id, x.Code, x.Name, x.BuildingId, x.GenderPolicy })
            .ToListAsync();

        var paymentMethods = await db.PaymentMethodCatalogs
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Code, x.Name, x.ProcessingFee })
            .ToListAsync();

        return Ok(new { buildings, rooms, students, roles, utilities, roomCategories, roomZones, paymentMethods });
    }
}
