using Dormitory.Models.DataContexts;
using Dormitory.Models.Entities;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Controllers.Catalog;

[ApiController]
[Route("api/catalog")]
public class CatalogController(AppDbContext db) : ControllerBase
{
    [HttpGet("room-categories")]
    public async Task<IActionResult> GetRoomCategories()
    {
        var data = await db.RoomCategories
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.BedLayout,
                x.DefaultCapacity,
                x.BaseMonthlyFee,
                x.DepositAmount,
                x.HygieneFee,
                x.ServiceFee,
                x.InternetFee,
                x.ElectricityUnitPrice,
                x.WaterUnitPrice,
                x.Description,
                x.IsActive,
                roomCount = x.Rooms.Count
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpPost("room-categories")]
    public async Task<IActionResult> CreateRoomCategory([FromBody] RoomCategoryRequest request)
    {
        var entity = new RoomCategory();
        ApplyRoomCategory(entity, request);
        db.RoomCategories.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("room-categories/{id:int}")]
    public async Task<IActionResult> UpdateRoomCategory(int id, [FromBody] RoomCategoryRequest request)
    {
        var entity = await db.RoomCategories.FindAsync(id);
        if (entity is null) return NotFound();

        ApplyRoomCategory(entity, request);
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("room-categories/{id:int}")]
    public async Task<IActionResult> DeleteRoomCategory(int id)
    {
        var hasRooms = await db.Rooms.AnyAsync(x => x.RoomCategoryId == id);
        if (hasRooms)
        {
            return BadRequest(new { message = "Không thể xóa loại phòng đang được sử dụng." });
        }

        var entity = await db.RoomCategories.FindAsync(id);
        if (entity is null) return NotFound();

        db.RoomCategories.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("room-zones")]
    public async Task<IActionResult> GetRoomZones()
    {
        var data = await db.RoomZones
            .Include(x => x.Building)
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.BuildingId,
                buildingName = x.Building == null ? "" : x.Building.Name,
                x.GenderPolicy,
                x.FloorFrom,
                x.FloorTo,
                x.ManagerName,
                x.Description,
                x.IsActive,
                roomCount = x.Rooms.Count
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpPost("room-zones")]
    public async Task<IActionResult> CreateRoomZone([FromBody] RoomZoneRequest request)
    {
        var entity = new RoomZone();
        ApplyRoomZone(entity, request);
        db.RoomZones.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("room-zones/{id:int}")]
    public async Task<IActionResult> UpdateRoomZone(int id, [FromBody] RoomZoneRequest request)
    {
        var entity = await db.RoomZones.FindAsync(id);
        if (entity is null) return NotFound();

        ApplyRoomZone(entity, request);
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("room-zones/{id:int}")]
    public async Task<IActionResult> DeleteRoomZone(int id)
    {
        var hasRooms = await db.Rooms.AnyAsync(x => x.RoomZoneId == id);
        if (hasRooms)
        {
            return BadRequest(new { message = "Không thể xóa phân khu đang có phòng." });
        }

        var entity = await db.RoomZones.FindAsync(id);
        if (entity is null) return NotFound();

        db.RoomZones.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("payment-methods")]
    public async Task<IActionResult> GetPaymentMethods()
    {
        var data = await db.PaymentMethodCatalogs
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.AccountName,
                x.AccountNumber,
                x.BankName,
                x.ProcessingFee,
                x.Description,
                x.IsActive
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpPost("payment-methods")]
    public async Task<IActionResult> CreatePaymentMethod([FromBody] PaymentMethodCatalogRequest request)
    {
        var entity = new PaymentMethodCatalog();
        ApplyPaymentMethod(entity, request);
        db.PaymentMethodCatalogs.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("payment-methods/{id:int}")]
    public async Task<IActionResult> UpdatePaymentMethod(int id, [FromBody] PaymentMethodCatalogRequest request)
    {
        var entity = await db.PaymentMethodCatalogs.FindAsync(id);
        if (entity is null) return NotFound();

        ApplyPaymentMethod(entity, request);
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("payment-methods/{id:int}")]
    public async Task<IActionResult> DeletePaymentMethod(int id)
    {
        var entity = await db.PaymentMethodCatalogs.FindAsync(id);
        if (entity is null) return NotFound();

        db.PaymentMethodCatalogs.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static void ApplyRoomCategory(RoomCategory entity, RoomCategoryRequest request)
    {
        entity.Code = request.Code.Trim();
        entity.Name = request.Name.Trim();
        entity.BedLayout = request.BedLayout.Trim();
        entity.DefaultCapacity = request.DefaultCapacity;
        entity.BaseMonthlyFee = request.BaseMonthlyFee;
        entity.DepositAmount = request.DepositAmount;
        entity.HygieneFee = request.HygieneFee;
        entity.ServiceFee = request.ServiceFee;
        entity.InternetFee = request.InternetFee;
        entity.ElectricityUnitPrice = request.ElectricityUnitPrice;
        entity.WaterUnitPrice = request.WaterUnitPrice;
        entity.Description = request.Description.Trim();
        entity.IsActive = request.IsActive;
    }

    private static void ApplyRoomZone(RoomZone entity, RoomZoneRequest request)
    {
        entity.Code = request.Code.Trim();
        entity.Name = request.Name.Trim();
        entity.BuildingId = request.BuildingId;
        entity.GenderPolicy = request.GenderPolicy.Trim();
        entity.FloorFrom = request.FloorFrom;
        entity.FloorTo = request.FloorTo;
        entity.ManagerName = request.ManagerName.Trim();
        entity.Description = request.Description.Trim();
        entity.IsActive = request.IsActive;
    }

    private static void ApplyPaymentMethod(PaymentMethodCatalog entity, PaymentMethodCatalogRequest request)
    {
        entity.Code = request.Code.Trim();
        entity.Name = request.Name.Trim();
        entity.AccountName = request.AccountName.Trim();
        entity.AccountNumber = request.AccountNumber.Trim();
        entity.BankName = request.BankName.Trim();
        entity.ProcessingFee = request.ProcessingFee;
        entity.Description = request.Description.Trim();
        entity.IsActive = request.IsActive;
    }
}
