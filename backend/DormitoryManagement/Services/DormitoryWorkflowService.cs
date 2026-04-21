using Dormitory.Models.DataContexts;
using Dormitory.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Services;

public static class DormitoryWorkflowService
{
    public static async Task<(bool Success, string? Message, Students? Student, Rooms? Room)> AssignStudentToRoomAsync(
        AppDbContext db,
        int studentId,
        int roomId,
        string status,
        string note)
    {
        var student = await db.Students.FirstOrDefaultAsync(x => x.Id == studentId);
        var room = await db.Rooms.Include(x => x.Building).FirstOrDefaultAsync(x => x.Id == roomId);

        if (student is null || room is null)
        {
            return (false, "Không tìm thấy sinh viên hoặc phòng.", null, null);
        }

        var validation = await ValidateRoomAssignmentAsync(db, student, room);
        if (!validation.Success)
        {
            return (false, validation.Message, student, room);
        }

        var previousRoomId = student.RoomId;
        student.RoomId = roomId;
        student.Status = status;
        student.UpdatedAt = DateTime.UtcNow;

        if (previousRoomId.HasValue && previousRoomId.Value != roomId)
        {
            await RoomOccupancyService.RecalculateRoomAsync(db, previousRoomId.Value);
        }

        await RoomOccupancyService.RecalculateRoomAsync(db, roomId);

        if (!string.IsNullOrWhiteSpace(note))
        {
            var registration = await db.Registrations
                .Where(x => x.StudentId == studentId && x.RoomId == roomId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (registration is not null)
            {
                registration.Note = note.Trim();
                registration.UpdatedAt = DateTime.UtcNow;
            }
        }

        return (true, null, student, room);
    }

    public static async Task<(bool Success, string? Message)> TransferStudentAsync(
        AppDbContext db,
        int studentId,
        int toRoomId,
        string status,
        string note)
    {
        var student = await db.Students.FirstOrDefaultAsync(x => x.Id == studentId);
        if (student is null)
        {
            return (false, "Không tìm thấy sinh viên.");
        }

        if (student.RoomId == toRoomId)
        {
            return (false, "Sinh viên đã ở trong phòng này.");
        }

        var result = await AssignStudentToRoomAsync(db, studentId, toRoomId, status, note);
        return (result.Success, result.Message);
    }

    public static async Task<(bool Success, string? Message)> RemoveStudentFromRoomAsync(
        AppDbContext db,
        int roomId,
        int studentId,
        string status,
        string note)
    {
        var student = await db.Students.FirstOrDefaultAsync(x => x.Id == studentId && x.RoomId == roomId);
        if (student is null)
        {
            return (false, "Sinh viên không thuộc phòng được chọn.");
        }

        student.RoomId = null;
        student.Status = status;
        student.UpdatedAt = DateTime.UtcNow;

        await RoomOccupancyService.RecalculateRoomAsync(db, roomId);

        if (!string.IsNullOrWhiteSpace(note))
        {
            var registration = await db.Registrations
                .Where(x => x.StudentId == studentId && x.RoomId == roomId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (registration is not null)
            {
                registration.Note = note.Trim();
                registration.UpdatedAt = DateTime.UtcNow;
            }
        }

        return (true, null);
    }

    public static async Task<(bool Success, string? Message)> ApproveRegistrationAsync(
        AppDbContext db,
        int registrationId,
        DateTime decisionDate,
        string note)
    {
        var registration = await db.Registrations.FirstOrDefaultAsync(x => x.Id == registrationId);
        if (registration is null)
        {
            return (false, "Không tìm thấy đơn đăng ký.");
        }

        registration.Status = "Approved";
        registration.ApprovedDate = decisionDate;
        registration.Note = string.IsNullOrWhiteSpace(note) ? "Đã duyệt đăng ký nội trú." : note.Trim();
        registration.UpdatedAt = DateTime.UtcNow;

        var assignResult = await AssignStudentToRoomAsync(db, registration.StudentId, registration.RoomId, "Active", registration.Note);
        if (!assignResult.Success)
        {
            return (false, assignResult.Message);
        }

        var room = await db.Rooms.FirstAsync(x => x.Id == registration.RoomId);
        var contractExists = await db.Contracts.AnyAsync(x =>
            x.StudentId == registration.StudentId &&
            x.RoomId == registration.RoomId &&
            x.Status == "Active");

        if (!contractExists)
        {
            db.Contracts.Add(new Contract
            {
                ContractCode = $"HD-{DateTime.Today:yyyyMMdd}-{registration.StudentId:D3}",
                StudentId = registration.StudentId,
                RoomId = registration.RoomId,
                DepositAmount = room.PricePerMonth,
                MonthlyFee = room.PricePerMonth,
                StartDate = decisionDate.Date,
                EndDate = decisionDate.Date.AddMonths(12),
                Status = "Active"
            });
        }

        return (true, null);
    }

    public static async Task<(bool Success, string? Message)> RejectRegistrationAsync(
        AppDbContext db,
        int registrationId,
        DateTime decisionDate,
        string note)
    {
        var registration = await db.Registrations.FirstOrDefaultAsync(x => x.Id == registrationId);
        if (registration is null)
        {
            return (false, "Không tìm thấy đơn đăng ký.");
        }

        registration.Status = "Rejected";
        registration.ApprovedDate = decisionDate;
        registration.Note = string.IsNullOrWhiteSpace(note) ? "Đơn đăng ký đã bị từ chối." : note.Trim();
        registration.UpdatedAt = DateTime.UtcNow;

        var student = await db.Students.FirstOrDefaultAsync(x => x.Id == registration.StudentId);
        if (student is not null)
        {
            student.Status = "Waiting";
            student.UpdatedAt = DateTime.UtcNow;
        }

        return (true, null);
    }

    public static async Task<(bool Success, string? Message, List<Invoices>? Invoices)> GenerateInvoicesFromUtilityAsync(
        AppDbContext db,
        int utilityId)
    {
        var utility = await db.Utilities
            .Include(x => x.Room)
            .FirstOrDefaultAsync(x => x.Id == utilityId);

        if (utility is null)
        {
            return (false, "Không tìm thấy kỳ điện nước.", null);
        }

        var students = await db.Students
            .Where(x => x.RoomId == utility.RoomId && x.Status != "Inactive")
            .OrderBy(x => x.StudentCode)
            .ToListAsync();
        var profile = await db.RoomFeeProfiles.FirstOrDefaultAsync(x => x.RoomId == utility.RoomId);

        if (students.Count == 0)
        {
            return (false, "Phòng chưa có sinh viên để tạo hóa đơn.", null);
        }

        var existingInvoices = await db.Invoices
            .Where(x => x.UtilityId == utilityId)
            .Select(x => x.StudentId)
            .ToListAsync();

        var usageElectricity = Math.Max(0, utility.ElectricityNew - utility.ElectricityOld);
        var usageWater = Math.Max(0, utility.WaterNew - utility.WaterOld);
        var electricityPerStudent = (usageElectricity * utility.ElectricityUnitPrice) / students.Count;
        var waterPerStudent = (usageWater * utility.WaterUnitPrice) / students.Count;
        var roomFeePerStudent = (profile?.MonthlyRoomFee ?? utility.Room!.PricePerMonth) / students.Count;
        var recurringServiceFee = (profile?.HygieneFee ?? 0) + (profile?.ServiceFee ?? 100000) + (profile?.InternetFee ?? 0) + (profile?.OtherFee ?? 0);
        var recurringServicePerStudent = recurringServiceFee / students.Count;

        var invoices = new List<Invoices>();

        foreach (var student in students.Where(x => !existingInvoices.Contains(x.Id)))
        {
            invoices.Add(new Invoices
            {
                InvoiceCode = $"INV-{utility.BillingMonth:yyyyMM}-{student.StudentCode}",
                StudentId = student.Id,
                RoomId = utility.RoomId,
                UtilityId = utility.Id,
                RoomFee = Math.Round(roomFeePerStudent, 0),
                ElectricityFee = Math.Round(electricityPerStudent, 0),
                WaterFee = Math.Round(waterPerStudent, 0),
                ServiceFee = Math.Round(recurringServicePerStudent, 0),
                Total = Math.Round(roomFeePerStudent, 0) + Math.Round(electricityPerStudent, 0) + Math.Round(waterPerStudent, 0) + Math.Round(recurringServicePerStudent, 0),
                Status = "Unpaid",
                BillingMonth = utility.BillingMonth,
                DueDate = utility.BillingMonth.AddDays(10)
            });
        }

        if (invoices.Count == 0)
        {
            return (false, "Kỳ điện nước này đã được tạo hóa đơn cho toàn bộ sinh viên.", null);
        }

        db.Invoices.AddRange(invoices);
        return (true, null, invoices);
    }

    public static async Task<(bool Success, string? Message, RoomFinanceRecord? Record)> GenerateRoomFinanceFromUtilityAsync(
        AppDbContext db,
        int utilityId)
    {
        var utility = await db.Utilities
            .Include(x => x.Room)
            .FirstOrDefaultAsync(x => x.Id == utilityId);

        if (utility is null)
        {
            return (false, "KhÃ´ng tÃ¬m tháº¥y ká»³ Ä‘iá»‡n nÆ°á»›c.", null);
        }

        var profile = await db.RoomFeeProfiles.FirstOrDefaultAsync(x => x.RoomId == utility.RoomId);
        if (profile is null)
        {
            return (false, "PhÃ²ng chÆ°a cÃ³ cáº¥u hÃ¬nh phÃ­ táº¡i chÃ­nh.", null);
        }

        var billingMonth = new DateTime(utility.BillingMonth.Year, utility.BillingMonth.Month, 1);
        var existingRecord = await db.RoomFinanceRecords
            .FirstOrDefaultAsync(x => x.RoomId == utility.RoomId && x.BillingMonth == billingMonth);

        var electricityFee = Math.Max(0, utility.ElectricityNew - utility.ElectricityOld) * utility.ElectricityUnitPrice;
        var waterFee = Math.Max(0, utility.WaterNew - utility.WaterOld) * utility.WaterUnitPrice;
        var total = profile.MonthlyRoomFee + electricityFee + waterFee + profile.HygieneFee + profile.ServiceFee + profile.InternetFee + profile.OtherFee;

        if (existingRecord is null)
        {
            existingRecord = new RoomFinanceRecord
            {
                RoomId = utility.RoomId,
                UtilityId = utility.Id,
                BillingMonth = billingMonth,
                MonthlyRoomFee = profile.MonthlyRoomFee,
                ElectricityFee = electricityFee,
                WaterFee = waterFee,
                HygieneFee = profile.HygieneFee,
                ServiceFee = profile.ServiceFee,
                InternetFee = profile.InternetFee,
                OtherFee = profile.OtherFee,
                Total = total,
                PaidAmount = 0,
                Status = "Unpaid",
                DueDate = billingMonth.AddDays(profile.BillingCycleDay),
                PaymentMethod = string.Empty,
                PaymentNote = "Táº¡o tá»± Ä‘á»™ng tá»« ká»³ Ä‘iá»‡n nÆ°á»›c",
                RecordedBy = "system"
            };

            db.RoomFinanceRecords.Add(existingRecord);
        }
        else
        {
            existingRecord.UtilityId = utility.Id;
            existingRecord.MonthlyRoomFee = profile.MonthlyRoomFee;
            existingRecord.ElectricityFee = electricityFee;
            existingRecord.WaterFee = waterFee;
            existingRecord.HygieneFee = profile.HygieneFee;
            existingRecord.ServiceFee = profile.ServiceFee;
            existingRecord.InternetFee = profile.InternetFee;
            existingRecord.OtherFee = profile.OtherFee;
            existingRecord.Total = total;
            existingRecord.DueDate = billingMonth.AddDays(profile.BillingCycleDay);
            existingRecord.Status = ResolveFinanceStatus(existingRecord.PaidAmount, total, existingRecord.DueDate, existingRecord.PaidDate);
            existingRecord.UpdatedAt = DateTime.UtcNow;
        }

        return (true, null, existingRecord);
    }

    public static async Task<(bool Success, string? Message, RoomFinanceRecord? Record)> MarkRoomFinancePaidAsync(
        AppDbContext db,
        int financeRecordId,
        decimal paidAmount,
        DateTime paidDate,
        string paymentMethod,
        string paymentNote,
        string recordedBy)
    {
        var record = await db.RoomFinanceRecords.FirstOrDefaultAsync(x => x.Id == financeRecordId);
        if (record is null)
        {
            return (false, "KhÃ´ng tÃ¬m tháº¥y cÃ´ng ná»£ phÃ²ng.", null);
        }

        if (paidAmount <= 0)
        {
            return (false, "Sá»‘ tiá»n thu pháº£i lá»›n hÆ¡n 0.", null);
        }

        record.PaidAmount = Math.Min(record.Total, record.PaidAmount + paidAmount);
        record.PaidDate = paidDate;
        record.PaymentMethod = paymentMethod.Trim();
        record.PaymentNote = paymentNote.Trim();
        record.RecordedBy = recordedBy.Trim();
        record.Status = ResolveFinanceStatus(record.PaidAmount, record.Total, record.DueDate, record.PaidDate);
        record.UpdatedAt = DateTime.UtcNow;

        return (true, null, record);
    }

    public static string ResolveFinanceStatus(decimal paidAmount, decimal totalAmount, DateTime dueDate, DateTime? paidDate)
    {
        if (paidAmount >= totalAmount && totalAmount > 0)
        {
            return "Paid";
        }

        if (paidAmount > 0)
        {
            return "PartiallyPaid";
        }

        var comparisonDate = paidDate?.Date ?? DateTime.Today;
        return dueDate.Date < comparisonDate ? "Late" : "Unpaid";
    }

    private static async Task<(bool Success, string? Message)> ValidateRoomAssignmentAsync(AppDbContext db, Students student, Rooms room)
    {
        if (student.RoomId == room.Id)
        {
            return (true, null);
        }

        var currentOccupancy = await db.Students.CountAsync(x => x.RoomId == room.Id && x.Id != student.Id);
        if (currentOccupancy >= room.Capacity)
        {
            return (false, "Phòng đã đầy, không thể xếp thêm sinh viên.");
        }

        if (room.Building is not null)
        {
            var genderPolicy = room.Building.GenderPolicy;
            if (genderPolicy == "Male" && student.Gender != "Male")
            {
                return (false, "Phòng thuộc khu nam, không thể xếp sinh viên nữ.");
            }

            if (genderPolicy == "Female" && student.Gender != "Female")
            {
                return (false, "Phòng thuộc khu nữ, không thể xếp sinh viên nam.");
            }
        }

        return (true, null);
    }
}
