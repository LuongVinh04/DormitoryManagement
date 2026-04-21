using Dormitory.Models.DataContexts;
using Dormitory.Models.Entities;
using DormitoryManagement.Models;
using DormitoryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Controllers;

[ApiController]
[Route("api/facilities")]
public class FacilitiesController(AppDbContext db) : ControllerBase
{
    [HttpGet("buildings")]
    public async Task<IActionResult> GetBuildings()
    {
        var data = await db.Buildings
            .Include(x => x.Rooms)
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.GenderPolicy,
                x.NumberOfFloors,
                x.ManagerName,
                x.Description,
                roomCount = x.Rooms.Count,
                totalCapacity = x.Rooms.Sum(r => r.Capacity),
                currentOccupancy = x.Rooms.Sum(r => r.CurrentOccupancy),
                createdAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpPost("buildings")]
    public async Task<IActionResult> CreateBuilding([FromBody] BuildingRequest request)
    {
        var entity = new Buildings
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            GenderPolicy = request.GenderPolicy.Trim(),
            NumberOfFloors = request.NumberOfFloors,
            ManagerName = request.ManagerName.Trim(),
            Description = request.Description.Trim()
        };

        db.Buildings.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("buildings/{id:int}")]
    public async Task<IActionResult> UpdateBuilding(int id, [FromBody] BuildingRequest request)
    {
        var entity = await db.Buildings.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Code = request.Code.Trim();
        entity.Name = request.Name.Trim();
        entity.GenderPolicy = request.GenderPolicy.Trim();
        entity.NumberOfFloors = request.NumberOfFloors;
        entity.ManagerName = request.ManagerName.Trim();
        entity.Description = request.Description.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("buildings/{id:int}")]
    public async Task<IActionResult> DeleteBuilding(int id)
    {
        var hasRooms = await db.Rooms.AnyAsync(x => x.BuildingId == id);
        if (hasRooms)
        {
            return BadRequest(new { message = "Không thể xóa tòa nhà khi vẫn còn phòng." });
        }

        var entity = await db.Buildings.FindAsync(id);
        if (entity is null) return NotFound();

        db.Buildings.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms()
    {
        var data = await db.Rooms
            .Include(x => x.Building)
            .OrderBy(x => x.RoomNumber)
            .Select(x => new
            {
                x.Id,
                x.RoomNumber,
                x.BuildingId,
                buildingName = x.Building!.Name,
                buildingPolicy = x.Building.GenderPolicy,
                x.FloorNumber,
                x.RoomType,
                x.Capacity,
                x.CurrentOccupancy,
                x.PricePerMonth,
                x.Status,
                availableSlots = x.Capacity - x.CurrentOccupancy,
                createdAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("rooms/{id:int}/overview")]
    public async Task<IActionResult> GetRoomOverview(int id)
    {
        var room = await db.Rooms
            .Include(x => x.Building)
            .Include(x => x.Students.OrderBy(s => s.StudentCode))
            .Include(x => x.Utilities.OrderByDescending(u => u.BillingMonth))
            .Include(x => x.Invoices.OrderByDescending(i => i.BillingMonth))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (room is null) return NotFound();

        return Ok(new
        {
            room = new
            {
                room.Id,
                room.RoomNumber,
                room.BuildingId,
                buildingName = room.Building!.Name,
                buildingPolicy = room.Building.GenderPolicy,
                room.FloorNumber,
                room.RoomType,
                room.Capacity,
                room.CurrentOccupancy,
                room.PricePerMonth,
                room.Status,
                availableSlots = room.Capacity - room.CurrentOccupancy
            },
            students = room.Students.Select(x => new
            {
                x.Id,
                x.StudentCode,
                x.Name,
                x.Gender,
                x.Phone,
                x.Email,
                x.Faculty,
                x.ClassName,
                x.Status
            }),
            utilities = room.Utilities.Take(6).Select(x => new
            {
                x.Id,
                x.BillingMonth,
                electricityUsage = x.ElectricityNew - x.ElectricityOld,
                waterUsage = x.WaterNew - x.WaterOld
            }),
            invoices = room.Invoices.Take(8).Select(x => new
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

    [HttpGet("rooms/{id:int}/students")]
    public async Task<IActionResult> GetRoomStudents(int id)
    {
        var roomExists = await db.Rooms.AnyAsync(x => x.Id == id);
        if (!roomExists) return NotFound();

        var students = await db.Students
            .Where(x => x.RoomId == id)
            .OrderBy(x => x.StudentCode)
            .Select(x => new
            {
                x.Id,
                x.StudentCode,
                x.Name,
                x.Gender,
                x.Faculty,
                x.ClassName,
                x.Phone,
                x.Email,
                x.Status
            })
            .ToListAsync();

        return Ok(students);
    }

    [HttpPost("rooms")]
    public async Task<IActionResult> CreateRoom([FromBody] RoomRequest request)
    {
        var entity = new Rooms
        {
            RoomNumber = request.RoomNumber.Trim(),
            BuildingId = request.BuildingId,
            FloorNumber = request.FloorNumber,
            RoomType = request.RoomType.Trim(),
            Capacity = request.Capacity,
            PricePerMonth = request.PricePerMonth,
            Status = request.Status.Trim()
        };

        db.Rooms.Add(entity);
        await db.SaveChangesAsync();
        await RoomOccupancyService.RecalculateRoomAsync(db, entity.Id);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("rooms/{id:int}")]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] RoomRequest request)
    {
        var entity = await db.Rooms.FindAsync(id);
        if (entity is null) return NotFound();

        entity.RoomNumber = request.RoomNumber.Trim();
        entity.BuildingId = request.BuildingId;
        entity.FloorNumber = request.FloorNumber;
        entity.RoomType = request.RoomType.Trim();
        entity.Capacity = request.Capacity;
        entity.PricePerMonth = request.PricePerMonth;
        entity.Status = request.Status.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await RoomOccupancyService.RecalculateRoomAsync(db, entity.Id);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPost("rooms/{roomId:int}/assign-student")]
    public async Task<IActionResult> AssignStudentToRoom(int roomId, [FromBody] AssignStudentRequest request)
    {
        var result = await DormitoryWorkflowService.AssignStudentToRoomAsync(
            db,
            request.StudentId,
            roomId,
            request.Status.Trim(),
            request.Note);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        await db.SaveChangesAsync();
        return Ok(new { message = "Đã xếp sinh viên vào phòng thành công." });
    }

    [HttpPost("rooms/transfer-student")]
    public async Task<IActionResult> TransferStudent([FromBody] TransferStudentRequest request)
    {
        var result = await DormitoryWorkflowService.TransferStudentAsync(
            db,
            request.StudentId,
            request.ToRoomId,
            request.Status.Trim(),
            request.Note);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        await db.SaveChangesAsync();
        return Ok(new { message = "Đã chuyển phòng cho sinh viên." });
    }

    [HttpPost("rooms/{roomId:int}/remove-student")]
    public async Task<IActionResult> RemoveStudentFromRoom(int roomId, [FromBody] RemoveStudentRequest request)
    {
        var result = await DormitoryWorkflowService.RemoveStudentFromRoomAsync(
            db,
            roomId,
            request.StudentId,
            request.Status.Trim(),
            request.Note);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        await db.SaveChangesAsync();
        return Ok(new { message = "Đã trả phòng cho sinh viên." });
    }

    [HttpDelete("rooms/{id:int}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var hasStudents = await db.Students.AnyAsync(x => x.RoomId == id);
        if (hasStudents)
        {
            return BadRequest(new { message = "Không thể xóa phòng đang có sinh viên." });
        }

        var entity = await db.Rooms.FindAsync(id);
        if (entity is null) return NotFound();

        db.Rooms.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
