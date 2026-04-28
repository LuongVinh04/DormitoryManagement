using Dormitory.Models.DataContexts;
using Dormitory.Models.Entities;
using DormitoryManagement.Services.Facilities;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Services.Operations;

public static class DormitoryWorkflowService
{
    public static async Task ApplyContractExpiryRulesAsync(AppDbContext db)
    {
        var today = DateTime.Today;
        var cancelCutoff = today.AddDays(-3);

        var contractsToCancel = await db.Contracts
            .Include(x => x.Student)
            .Where(x =>
                x.Status == "Active" &&
                x.EndDate <= cancelCutoff)
            .ToListAsync();

        if (contractsToCancel.Count == 0)
        {
            return;
        }

        var affectedRoomIds = contractsToCancel
            .Where(x => x.Student?.RoomId != null)
            .Select(x => x.Student!.RoomId!.Value)
            .Distinct()
            .ToList();

        foreach (var contract in contractsToCancel)
        {
            contract.Status = "Cancelled";
            contract.UpdatedAt = DateTime.UtcNow;

            if (contract.Student is not null)
            {
                contract.Student.RoomId = null;
                contract.Student.Status = "Waiting";
                contract.Student.UpdatedAt = DateTime.UtcNow;
            }
        }

        foreach (var roomId in affectedRoomIds)
        {
            await RoomOccupancyService.RecalculateRoomAsync(db, roomId);
        }
    }

    public static async Task<(bool Success, string? Message, Students? Student, Rooms? Room)> AssignStudentToRoomAsync(
        AppDbContext db,
        int studentId,
        int roomId,
        string status,
        string note)
    {
        await ApplyContractExpiryRulesAsync(db);

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

        var room = await db.Rooms.FirstAsync(x => x.Id == registration.RoomId);
        var contractExists = await db.Contracts.AnyAsync(x =>
            x.StudentId == registration.StudentId &&
            x.RoomId == registration.RoomId &&
            x.Status == "Active" &&
            x.EndDate >= decisionDate.Date);

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
            await db.SaveChangesAsync();
        }

        var assignResult = await AssignStudentToRoomAsync(db, registration.StudentId, registration.RoomId, "Active", registration.Note);
        if (!assignResult.Success)
        {
            return (false, assignResult.Message);
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

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var students = await db.Students
            .Where(x =>
                x.RoomId == utility.RoomId &&
                x.Status != "Inactive" &&
                x.Contracts.Any(c =>
                    c.Status == "Active" &&
                    c.StartDate < tomorrow &&
                    c.EndDate >= today))
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
            return (false, "Không tìm thấy kỳ điện nước.", null);
        }

        var profile = await db.RoomFeeProfiles.FirstOrDefaultAsync(x => x.RoomId == utility.RoomId);
        if (profile is null)
        {
            return (false, "Phòng chưa có cấu hình phí tài chính.", null);
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
                PaymentNote = "Tạo tự động từ kỳ điện nước",
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
            return (false, "Không tìm thấy công nợ phòng.", null);
        }

        if (paidAmount <= 0)
        {
            return (false, "Số tiền thu phải lớn hơn 0.", null);
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

    public static async Task UpdateRoomFinanceFromSharesAsync(AppDbContext db, int roomFinanceRecordId, DateTime? paidDate = null)
    {
        var record = await db.RoomFinanceRecords.FirstOrDefaultAsync(x => x.Id == roomFinanceRecordId);
        if (record is null)
        {
            return;
        }

        var shares = await db.RoomFinanceStudentShares
            .Where(x => x.RoomFinanceRecordId == roomFinanceRecordId)
            .ToListAsync();

        if (shares.Count == 0)
        {
            return;
        }

        var totalPaid = shares.Sum(x => x.PaidAmount);
        record.PaidAmount = Math.Min(record.Total, totalPaid);
        var lastSharePaidDate = shares
            .Where(x => x.PaidDate.HasValue)
            .Select(x => x.PaidDate)
            .DefaultIfEmpty(null)
            .Max();
        record.PaidDate = record.PaidAmount >= record.Total && record.Total > 0
            ? paidDate ?? lastSharePaidDate ?? DateTime.Today
            : null;
        record.Status = ResolveFinanceStatus(record.PaidAmount, record.Total, record.DueDate, record.PaidDate ?? paidDate);
        record.UpdatedAt = DateTime.UtcNow;
    }

    private static async Task<(bool Success, string? Message)> ValidateRoomAssignmentAsync(AppDbContext db, Students student, Rooms room)
    {
        if (student.RoomId == room.Id)
        {
            return (true, null);
        }

        // Kiểm tra sinh viên có hợp đồng hợp lệ
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var hasValidContract = await db.Contracts.AnyAsync(x =>
            x.StudentId == student.Id &&
            x.RoomId == room.Id &&
            x.Status == "Active" &&
            x.StartDate < tomorrow &&
            x.EndDate >= today);

        if (!hasValidContract)
        {
            return (false, "Sinh viên cần có hợp đồng lưu trú hiệu lực cho đúng phòng trước khi xếp phòng.");
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

