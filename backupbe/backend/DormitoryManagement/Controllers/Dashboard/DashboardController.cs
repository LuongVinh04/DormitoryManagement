using Dormitory.Models.DataContexts;
using DormitoryManagement.Services.Facilities;
using DormitoryManagement.Services.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Controllers.Dashboard;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        await DormitoryWorkflowService.ApplyContractExpiryRulesAsync(db);
        await RoomOccupancyService.RecalculateAllRoomsAsync(db);
        await db.SaveChangesAsync();

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
        var activeContracts = contracts.Count(x => x.Status == "Active" && x.StartDate.Date <= DateTime.Today && x.EndDate.Date >= DateTime.Today);
        var expiringContracts = contracts.Count(x => x.EndDate <= DateTime.Today.AddDays(45) && x.EndDate >= DateTime.Today && x.Status == "Active");
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

        var pendingRegistrations = registrations.Where(x => x.Status == "Pending").ToList();
        var dueSoonEnd = DateTime.Today.AddDays(5);
        var dueSoonFinances = financeRecords
            .Where(x => x.Status != "Paid" && x.DueDate.Date >= DateTime.Today && x.DueDate.Date <= dueSoonEnd)
            .ToList();
        var overdueFinancesList = financeRecords
            .Where(x => x.Status != "Paid" && x.DueDate.Date < DateTime.Today)
            .ToList();

        var notifications = new List<object>();

        if (pendingRegistrations.Count > 0)
        {
            notifications.Add(new
            {
                id = "pending-registrations",
                type = "operations",
                severity = "warning",
                title = "Hồ sơ chưa duyệt",
                description = $"Có {pendingRegistrations.Count} hồ sơ đang chờ duyệt.",
                count = pendingRegistrations.Count,
                amount = (decimal?)null,
                route = "/operations",
                panelKey = "operations-registrations",
                panelId = "panel-operations-registrations",
                actionLabel = "Đi tới duyệt hồ sơ"
            });
        }

        if (dueSoonFinances.Count > 0)
        {
            notifications.Add(new
            {
                id = "due-soon-finances",
                type = "finance",
                severity = "warning",
                title = "Khoản đến hạn",
                description = $"Có {dueSoonFinances.Count} khoản tài chính sắp đến hạn (5 ngày).",
                count = dueSoonFinances.Count,
                amount = dueSoonFinances.Sum(x => x.Total - x.PaidAmount),
                route = "/finance",
                panelKey = "finance-room-finances",
                panelId = "panel-finance-room-finances",
                actionLabel = "Đi tới theo dõi tài chính"
            });
        }

        if (overdueFinancesList.Count > 0)
        {
            notifications.Add(new
            {
                id = "overdue-finances",
                type = "finance",
                severity = "danger",
                title = "Khoản quá hạn",
                description = $"Có {overdueFinancesList.Count} khoản đã quá hạn thanh toán.",
                count = overdueFinancesList.Count,
                amount = overdueFinancesList.Sum(x => x.Total - x.PaidAmount),
                route = "/finance",
                panelKey = "finance-room-finances",
                panelId = "panel-finance-room-finances",
                actionLabel = "Đi tới xử lý công nợ"
            });
        }

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
            roomSnapshots,
            notifications
        });
    }
}
