using Dormitory.Models.DataContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var allInvoices = await db.Invoices.AsNoTracking().ToListAsync();
        var financeRecords = await db.RoomFinanceRecords.AsNoTracking().ToListAsync();
        var rooms = await db.Rooms.Include(x => x.Building).AsNoTracking().ToListAsync();
        var students = await db.Students.AsNoTracking().ToListAsync();
        var contracts = await db.Contracts.AsNoTracking().ToListAsync();
        var registrations = await db.Registrations
            .Include(x => x.Student)
            .Include(x => x.Room)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        var users = await db.Users.AsNoTracking().ToListAsync();

        var currentMonthRecords = financeRecords
            .Where(x => x.BillingMonth.Year == DateTime.Today.Year && x.BillingMonth.Month == DateTime.Today.Month)
            .ToList();

        var totalRooms = rooms.Count;
        var occupiedRooms = rooms.Count(x => x.CurrentOccupancy > 0);
        var fullRooms = rooms.Count(x => x.CurrentOccupancy >= x.Capacity);
        var availableBeds = rooms.Sum(x => x.Capacity - x.CurrentOccupancy);
        var totalStudents = students.Count;
        var waitingStudents = students.Count(x => x.Status is "Waiting" or "Pending");
        var activeContracts = contracts.Count(x => x.Status == "Active");
        var expiringContracts = contracts.Count(x => x.EndDate <= DateTime.Today.AddDays(45) && x.Status == "Active");
        var unpaidInvoices = financeRecords.Count > 0
            ? financeRecords.Count(x => x.Status != "Paid")
            : allInvoices.Count(x => x.Status != "Paid");
        var overdueInvoices = financeRecords.Count > 0
            ? financeRecords.Count(x => x.Status != "Paid" && x.DueDate.Date < DateTime.Today)
            : allInvoices.Count(x => x.Status != "Paid" && x.DueDate.Date < DateTime.Today);
        var revenueThisMonth = financeRecords.Count > 0
            ? currentMonthRecords.Sum(x => x.Total)
            : allInvoices
                .Where(x => x.BillingMonth.Year == DateTime.Today.Year && x.BillingMonth.Month == DateTime.Today.Month)
                .Sum(x => x.Total);
        var activeUsers = users.Count(x => x.IsActive);

        var occupancyByBuilding = rooms
            .GroupBy(x => x.Building!.Name)
            .Select(g => new
            {
                building = g.Key,
                occupied = g.Sum(x => x.CurrentOccupancy),
                capacity = g.Sum(x => x.Capacity),
                available = g.Sum(x => x.Capacity - x.CurrentOccupancy)
            })
            .ToList();

        var roomStatus = rooms
            .GroupBy(x => x.Status)
            .Select(g => new
            {
                status = g.Key,
                count = g.Count()
            })
            .ToList();

        var invoiceStatus = (financeRecords.Count > 0 ? financeRecords.Select(x => new { x.Status, x.Total }) : allInvoices.Select(x => new { x.Status, x.Total }))
            .GroupBy(x => x.Status)
            .Select(g => new
            {
                status = g.Key,
                count = g.Count(),
                total = g.Sum(x => x.Total)
            })
            .ToList();

        var monthlyRevenueData = (financeRecords.Count > 0
                ? financeRecords.Select(x => new { x.BillingMonth, x.Total, x.Status })
                : allInvoices.Select(x => new { x.BillingMonth, x.Total, x.Status }))
            .OrderBy(x => x.BillingMonth)
            .GroupBy(x => new { x.BillingMonth.Year, x.BillingMonth.Month })
            .Select(g => new
            {
                month = $"{g.Key.Month:00}/{g.Key.Year}",
                total = g.Sum(x => x.Total),
                paid = g.Where(x => x.Status == "Paid").Sum(x => x.Total),
                unpaid = g.Where(x => x.Status != "Paid").Sum(x => x.Total)
            })
            .ToList();

        var recentActivities = registrations
            .Take(6)
            .Select(x => new
            {
                id = x.Id,
                student = x.Student!.Name,
                room = x.Room!.RoomNumber,
                status = x.Status,
                createdAt = x.CreatedAt
            })
            .ToList();

        var alerts = new List<object>
        {
            new
            {
                level = overdueInvoices > 0 ? "high" : "medium",
                title = "Công nợ đến hạn",
                value = overdueInvoices,
                description = "Hóa đơn quá hạn cần xử lý ngay."
            },
            new
            {
                level = waitingStudents > 0 ? "medium" : "low",
                title = "Sinh viên chờ xếp phòng",
                value = waitingStudents,
                description = "Danh sách cần duyệt hoặc bố trí chỗ ở."
            },
            new
            {
                level = expiringContracts > 0 ? "medium" : "low",
                title = "Hợp đồng sắp hết hạn",
                value = expiringContracts,
                description = "Theo dõi gia hạn hoặc thanh lý hợp đồng."
            }
        };

        var roomSnapshots = rooms
            .OrderBy(x => x.Building!.Code)
            .ThenBy(x => x.RoomNumber)
            .Take(8)
            .Select(x => new
            {
                x.Id,
                x.RoomNumber,
                building = x.Building!.Name,
                x.CurrentOccupancy,
                x.Capacity,
                x.Status,
                availableSlots = x.Capacity - x.CurrentOccupancy
            })
            .ToList();

        return Ok(new
        {
            summary = new
            {
                totalRooms,
                occupiedRooms,
                fullRooms,
                availableBeds,
                totalStudents,
                waitingStudents,
                activeContracts,
                expiringContracts,
                unpaidInvoices,
                overdueInvoices,
                activeUsers,
                revenueThisMonth
            },
            occupancyByBuilding,
            roomStatus,
            invoiceStatus,
            monthlyRevenue = monthlyRevenueData,
            recentActivities,
            alerts,
            roomSnapshots
        });
    }
}
