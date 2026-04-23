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

        room.CurrentOccupancy = await db.Students.CountAsync(x => x.RoomId == roomId);
        room.Status = room.CurrentOccupancy switch
        {
            0 => "Available",
            var occupied when occupied >= room.Capacity => "Full",
            _ => "Occupied"
        };
        room.UpdatedAt = DateTime.UtcNow;
    }
}
