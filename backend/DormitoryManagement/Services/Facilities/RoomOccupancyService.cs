using Dormitory.Models.DataContexts;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Services.Facilities;

public static class RoomOccupancyService
{
    public static async Task RecalculateRoomAsync(AppDbContext db, int roomId)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(x => x.Id == roomId);
        if (room is null)
        {
            return;
        }

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        room.CurrentOccupancy = await db.Students.CountAsync(x =>
            x.RoomId == roomId &&
            x.Contracts.Any(c =>
                c.Status == "Active" &&
                c.StartDate < tomorrow &&
                c.EndDate >= today));

        room.Status = room.CurrentOccupancy switch
        {
            0 => "Available",
            var occupied when occupied >= room.Capacity => "Full",
            _ => "Occupied"
        };
        room.UpdatedAt = DateTime.UtcNow;
    }

    public static async Task RecalculateAllRoomsAsync(AppDbContext db)
    {
        var roomIds = await db.Rooms.Select(x => x.Id).ToListAsync();
        foreach (var roomId in roomIds)
        {
            await RecalculateRoomAsync(db, roomId);
        }
    }
}
