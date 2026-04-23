using Dormitory.Models.DataContexts;
using Dormitory.Models.Entities;
using DormitoryManagement.Models;
using DormitoryManagement.Services.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Controllers.Operations;

[ApiController]
[Route("api/operations")]
public class OperationsController(AppDbContext db) : ControllerBase
{
    [HttpGet("registrations")]
    public async Task<IActionResult> GetRegistrations()
    {
        var data = await db.Registrations
            .Include(x => x.Student)
            .Include(x => x.Room)
            .OrderByDescending(x => x.RegistrationDate)
            .Select(x => new
            {
                x.Id,
                x.StudentId,
                studentName = x.Student!.Name,
                studentCode = x.Student.StudentCode,
                x.RoomId,
                roomNumber = x.Room!.RoomNumber,
                x.RegistrationDate,
                x.ApprovedDate,
                x.Note,
                x.Status,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpPost("registrations")]
    public async Task<IActionResult> CreateRegistration([FromBody] RegistrationRequest request)
    {
        var entity = new Registrations
        {
            StudentId = request.StudentId,
            RoomId = request.RoomId,
            RegistrationDate = request.RegistrationDate,
            ApprovedDate = request.ApprovedDate,
            Note = request.Note.Trim(),
            Status = request.Status.Trim()
        };

        db.Registrations.Add(entity);

        var student = await db.Students.FindAsync(request.StudentId);
        if (student is not null && student.Status == "Waiting")
        {
            student.Status = "Pending";
            student.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("registrations/{id:int}")]
    public async Task<IActionResult> UpdateRegistration(int id, [FromBody] RegistrationRequest request)
    {
        var entity = await db.Registrations.FindAsync(id);
        if (entity is null) return NotFound();

        entity.StudentId = request.StudentId;
        entity.RoomId = request.RoomId;
        entity.RegistrationDate = request.RegistrationDate;
        entity.ApprovedDate = request.ApprovedDate;
        entity.Note = request.Note.Trim();
        entity.Status = request.Status.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPost("registrations/{id:int}/approve")]
    public async Task<IActionResult> ApproveRegistration(int id, [FromBody] RegistrationDecisionRequest request)
    {
        var result = await DormitoryWorkflowService.ApproveRegistrationAsync(
            db,
            id,
            request.DecisionDate ?? DateTime.Today,
            request.Note);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        await db.SaveChangesAsync();
        return Ok(new { message = "Đã duyệt đăng ký và xếp phòng cho sinh viên." });
    }

    [HttpPost("registrations/{id:int}/reject")]
    public async Task<IActionResult> RejectRegistration(int id, [FromBody] RegistrationDecisionRequest request)
    {
        var result = await DormitoryWorkflowService.RejectRegistrationAsync(
            db,
            id,
            request.DecisionDate ?? DateTime.Today,
            request.Note);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        await db.SaveChangesAsync();
        return Ok(new { message = "Đã từ chối đơn đăng ký." });
    }

    [HttpDelete("registrations/{id:int}")]
    public async Task<IActionResult> DeleteRegistration(int id)
    {
        var entity = await db.Registrations.FindAsync(id);
        if (entity is null) return NotFound();

        db.Registrations.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("contracts")]
    public async Task<IActionResult> GetContracts()
    {
        var data = await db.Contracts
            .Include(x => x.Student)
            .Include(x => x.Room)
            .OrderByDescending(x => x.StartDate)
            .Select(x => new
            {
                x.Id,
                x.ContractCode,
                x.StudentId,
                studentName = x.Student!.Name,
                x.RoomId,
                roomNumber = x.Room!.RoomNumber,
                x.DepositAmount,
                x.MonthlyFee,
                x.StartDate,
                x.EndDate,
                x.Status,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpPost("contracts")]
    public async Task<IActionResult> CreateContract([FromBody] ContractRequest request)
    {
        var entity = new Contract
        {
            ContractCode = request.ContractCode.Trim(),
            StudentId = request.StudentId,
            RoomId = request.RoomId,
            DepositAmount = request.DepositAmount,
            MonthlyFee = request.MonthlyFee,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status.Trim()
        };

        db.Contracts.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("contracts/{id:int}")]
    public async Task<IActionResult> UpdateContract(int id, [FromBody] ContractRequest request)
    {
        var entity = await db.Contracts.FindAsync(id);
        if (entity is null) return NotFound();

        entity.ContractCode = request.ContractCode.Trim();
        entity.StudentId = request.StudentId;
        entity.RoomId = request.RoomId;
        entity.DepositAmount = request.DepositAmount;
        entity.MonthlyFee = request.MonthlyFee;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Status = request.Status.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("contracts/{id:int}")]
    public async Task<IActionResult> DeleteContract(int id)
    {
        var entity = await db.Contracts.FindAsync(id);
        if (entity is null) return NotFound();

        db.Contracts.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("utilities")]
    public async Task<IActionResult> GetUtilities()
    {
        var data = await db.Utilities
            .Include(x => x.Room)
            .ThenInclude(x => x!.Building)
            .OrderByDescending(x => x.BillingMonth)
            .Select(x => new
            {
                x.Id,
                x.RoomId,
                roomNumber = x.Room!.RoomNumber,
                buildingName = x.Room.Building!.Name,
                x.ElectricityOld,
                x.ElectricityNew,
                electricityUsage = x.ElectricityNew - x.ElectricityOld,
                x.WaterOld,
                x.WaterNew,
                waterUsage = x.WaterNew - x.WaterOld,
                x.ElectricityUnitPrice,
                x.WaterUnitPrice,
                x.BillingMonth,
                electricityFee = (x.ElectricityNew - x.ElectricityOld) * x.ElectricityUnitPrice,
                waterFee = (x.WaterNew - x.WaterOld) * x.WaterUnitPrice,
                generatedInvoiceCount = db.Invoices.Count(i => i.UtilityId == x.Id),
                generatedRoomFinanceCount = db.RoomFinanceRecords.Count(i => i.UtilityId == x.Id),
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpPost("utilities")]
    public async Task<IActionResult> CreateUtility([FromBody] UtilityRequest request)
    {
        var entity = new Utilities
        {
            RoomId = request.RoomId,
            ElectricityOld = request.ElectricityOld,
            ElectricityNew = request.ElectricityNew,
            WaterOld = request.WaterOld,
            WaterNew = request.WaterNew,
            ElectricityUnitPrice = request.ElectricityUnitPrice,
            WaterUnitPrice = request.WaterUnitPrice,
            BillingMonth = request.BillingMonth
        };

        db.Utilities.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("utilities/{id:int}")]
    public async Task<IActionResult> UpdateUtility(int id, [FromBody] UtilityRequest request)
    {
        var entity = await db.Utilities.FindAsync(id);
        if (entity is null) return NotFound();

        entity.RoomId = request.RoomId;
        entity.ElectricityOld = request.ElectricityOld;
        entity.ElectricityNew = request.ElectricityNew;
        entity.WaterOld = request.WaterOld;
        entity.WaterNew = request.WaterNew;
        entity.ElectricityUnitPrice = request.ElectricityUnitPrice;
        entity.WaterUnitPrice = request.WaterUnitPrice;
        entity.BillingMonth = request.BillingMonth;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPost("utilities/{id:int}/generate-invoices")]
    public async Task<IActionResult> GenerateInvoicesFromUtility(int id)
    {
        var result = await DormitoryWorkflowService.GenerateInvoicesFromUtilityAsync(db, id);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        await db.SaveChangesAsync();
        return Ok(new
        {
            message = "Đã tạo hóa đơn từ kỳ điện nước.",
            count = result.Invoices!.Count
        });
    }

    [HttpDelete("utilities/{id:int}")]
    public async Task<IActionResult> DeleteUtility(int id)
    {
        var entity = await db.Utilities.FindAsync(id);
        if (entity is null) return NotFound();

        db.Utilities.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("room-fee-profiles")]
    public async Task<IActionResult> GetRoomFeeProfiles()
    {
        var data = await db.RoomFeeProfiles
            .Include(x => x.Room)
            .ThenInclude(x => x!.Building)
            .OrderBy(x => x.Room!.Building!.Code)
            .ThenBy(x => x.Room!.RoomNumber)
            .Select(x => new
            {
                x.Id,
                x.RoomId,
                roomNumber = x.Room!.RoomNumber,
                buildingName = x.Room.Building!.Name,
                x.MonthlyRoomFee,
                x.ElectricityUnitPrice,
                x.WaterUnitPrice,
                x.HygieneFee,
                x.ServiceFee,
                x.InternetFee,
                x.OtherFee,
                x.OtherFeeName,
                x.BillingCycleDay,
                x.Notes,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpPost("room-fee-profiles")]
    public async Task<IActionResult> CreateRoomFeeProfile([FromBody] RoomFeeProfileRequest request)
    {
        var entity = new RoomFeeProfile
        {
            RoomId = request.RoomId,
            MonthlyRoomFee = request.MonthlyRoomFee,
            ElectricityUnitPrice = request.ElectricityUnitPrice,
            WaterUnitPrice = request.WaterUnitPrice,
            HygieneFee = request.HygieneFee,
            ServiceFee = request.ServiceFee,
            InternetFee = request.InternetFee,
            OtherFee = request.OtherFee,
            OtherFeeName = request.OtherFeeName.Trim(),
            BillingCycleDay = request.BillingCycleDay,
            Notes = request.Notes.Trim()
        };

        db.RoomFeeProfiles.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("room-fee-profiles/{id:int}")]
    public async Task<IActionResult> UpdateRoomFeeProfile(int id, [FromBody] RoomFeeProfileRequest request)
    {
        var entity = await db.RoomFeeProfiles.FindAsync(id);
        if (entity is null) return NotFound();

        entity.RoomId = request.RoomId;
        entity.MonthlyRoomFee = request.MonthlyRoomFee;
        entity.ElectricityUnitPrice = request.ElectricityUnitPrice;
        entity.WaterUnitPrice = request.WaterUnitPrice;
        entity.HygieneFee = request.HygieneFee;
        entity.ServiceFee = request.ServiceFee;
        entity.InternetFee = request.InternetFee;
        entity.OtherFee = request.OtherFee;
        entity.OtherFeeName = request.OtherFeeName.Trim();
        entity.BillingCycleDay = request.BillingCycleDay;
        entity.Notes = request.Notes.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("room-fee-profiles/{id:int}")]
    public async Task<IActionResult> DeleteRoomFeeProfile(int id)
    {
        var entity = await db.RoomFeeProfiles.FindAsync(id);
        if (entity is null) return NotFound();

        db.RoomFeeProfiles.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("room-finances")]
    public async Task<IActionResult> GetRoomFinanceRecords()
    {
        var data = await db.RoomFinanceRecords
            .Include(x => x.Room)
            .ThenInclude(x => x!.Building)
            .Include(x => x.Utility)
            .OrderByDescending(x => x.BillingMonth)
            .ThenBy(x => x.Room!.RoomNumber)
            .Select(x => new
            {
                x.Id,
                x.RoomId,
                roomNumber = x.Room!.RoomNumber,
                buildingName = x.Room.Building!.Name,
                x.UtilityId,
                x.BillingMonth,
                electricityUsage = x.Utility != null ? x.Utility.ElectricityNew - x.Utility.ElectricityOld : 0,
                waterUsage = x.Utility != null ? x.Utility.WaterNew - x.Utility.WaterOld : 0,
                x.MonthlyRoomFee,
                x.ElectricityFee,
                x.WaterFee,
                x.HygieneFee,
                x.ServiceFee,
                x.InternetFee,
                x.OtherFee,
                x.Total,
                x.PaidAmount,
                remainingAmount = x.Total - x.PaidAmount,
                x.Status,
                x.DueDate,
                x.PaidDate,
                x.PaymentMethod,
                x.PaymentNote,
                x.RecordedBy,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("room-finances/summary")]
    public async Task<IActionResult> GetRoomFinanceSummary()
    {
        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var records = await db.RoomFinanceRecords
            .Include(x => x.Room)
            .Where(x => x.BillingMonth.Year == currentMonth.Year && x.BillingMonth.Month == currentMonth.Month)
            .OrderBy(x => x.Room!.RoomNumber)
            .ToListAsync();

        var totalExpected = records.Sum(x => x.Total);
        var collected = records.Sum(x => x.PaidAmount);
        var outstanding = totalExpected - collected;

        return Ok(new
        {
            billingMonth = currentMonth,
            totalExpected,
            collected,
            outstanding,
            paidRooms = records.Count(x => x.Status == "Paid"),
            partialRooms = records.Count(x => x.Status == "PartiallyPaid"),
            unpaidRooms = records.Count(x => x.Status == "Unpaid"),
            lateRooms = records.Count(x => x.Status == "Late"),
            roomStatuses = records.Select(x => new
            {
                x.Id,
                x.RoomId,
                roomNumber = x.Room!.RoomNumber,
                x.Total,
                x.PaidAmount,
                remainingAmount = x.Total - x.PaidAmount,
                x.Status,
                x.DueDate
            })
        });
    }

    [HttpPost("room-finances")]
    public async Task<IActionResult> CreateRoomFinanceRecord([FromBody] RoomFinanceRecordRequest request)
    {
        var billingMonth = new DateTime(request.BillingMonth.Year, request.BillingMonth.Month, 1);
        var entity = new RoomFinanceRecord
        {
            RoomId = request.RoomId,
            UtilityId = request.UtilityId,
            BillingMonth = billingMonth,
            MonthlyRoomFee = request.MonthlyRoomFee,
            ElectricityFee = request.ElectricityFee,
            WaterFee = request.WaterFee,
            HygieneFee = request.HygieneFee,
            ServiceFee = request.ServiceFee,
            InternetFee = request.InternetFee,
            OtherFee = request.OtherFee,
            Total = request.MonthlyRoomFee + request.ElectricityFee + request.WaterFee + request.HygieneFee + request.ServiceFee + request.InternetFee + request.OtherFee,
            PaidAmount = request.PaidAmount,
            Status = DormitoryWorkflowService.ResolveFinanceStatus(
                request.PaidAmount,
                request.MonthlyRoomFee + request.ElectricityFee + request.WaterFee + request.HygieneFee + request.ServiceFee + request.InternetFee + request.OtherFee,
                request.DueDate,
                request.PaidDate),
            DueDate = request.DueDate,
            PaidDate = request.PaidDate,
            PaymentMethod = request.PaymentMethod.Trim(),
            PaymentNote = request.PaymentNote.Trim(),
            RecordedBy = request.RecordedBy.Trim()
        };

        db.RoomFinanceRecords.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("room-finances/{id:int}")]
    public async Task<IActionResult> UpdateRoomFinanceRecord(int id, [FromBody] RoomFinanceRecordRequest request)
    {
        var entity = await db.RoomFinanceRecords.FindAsync(id);
        if (entity is null) return NotFound();
        var billingMonth = new DateTime(request.BillingMonth.Year, request.BillingMonth.Month, 1);

        entity.RoomId = request.RoomId;
        entity.UtilityId = request.UtilityId;
        entity.BillingMonth = billingMonth;
        entity.MonthlyRoomFee = request.MonthlyRoomFee;
        entity.ElectricityFee = request.ElectricityFee;
        entity.WaterFee = request.WaterFee;
        entity.HygieneFee = request.HygieneFee;
        entity.ServiceFee = request.ServiceFee;
        entity.InternetFee = request.InternetFee;
        entity.OtherFee = request.OtherFee;
        entity.Total = request.MonthlyRoomFee + request.ElectricityFee + request.WaterFee + request.HygieneFee + request.ServiceFee + request.InternetFee + request.OtherFee;
        entity.PaidAmount = request.PaidAmount;
        entity.DueDate = request.DueDate;
        entity.PaidDate = request.PaidDate;
        entity.PaymentMethod = request.PaymentMethod.Trim();
        entity.PaymentNote = request.PaymentNote.Trim();
        entity.RecordedBy = request.RecordedBy.Trim();
        entity.Status = DormitoryWorkflowService.ResolveFinanceStatus(entity.PaidAmount, entity.Total, entity.DueDate, entity.PaidDate);
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPost("room-finances/generate-from-utility/{utilityId:int}")]
    public async Task<IActionResult> GenerateRoomFinanceFromUtility(int utilityId)
    {
        var result = await DormitoryWorkflowService.GenerateRoomFinanceFromUtilityAsync(db, utilityId);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        await db.SaveChangesAsync();
        return Ok(new
        {
            message = "ÄÃ£ táº¡o cÃ´ng ná»£ táº¡i chÃ­nh cho phÃ²ng tá»« ká»³ Ä‘iá»‡n nÆ°á»›c.",
            recordId = result.Record!.Id
        });
    }

    [HttpPost("room-finances/{id:int}/mark-paid")]
    public async Task<IActionResult> MarkRoomFinancePaid(int id, [FromBody] RoomFinancePaymentRequest request)
    {
        var result = await DormitoryWorkflowService.MarkRoomFinancePaidAsync(
            db,
            id,
            request.PaidAmount,
            request.PaidDate ?? DateTime.Today,
            request.PaymentMethod,
            request.PaymentNote,
            request.RecordedBy);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        await db.SaveChangesAsync();
        return Ok(new { message = "ÄÃ£ ghi nháº­n thanh toÃ¡n cho phÃ²ng." });
    }

    [HttpDelete("room-finances/{id:int}")]
    public async Task<IActionResult> DeleteRoomFinanceRecord(int id)
    {
        var entity = await db.RoomFinanceRecords.FindAsync(id);
        if (entity is null) return NotFound();

        db.RoomFinanceRecords.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices()
    {
        var data = await db.Invoices
            .Include(x => x.Student)
            .Include(x => x.Room)
            .OrderByDescending(x => x.BillingMonth)
            .Select(x => new
            {
                x.Id,
                x.InvoiceCode,
                x.StudentId,
                studentName = x.Student!.Name,
                studentCode = x.Student.StudentCode,
                x.RoomId,
                roomNumber = x.Room!.RoomNumber,
                x.UtilityId,
                x.RoomFee,
                x.ElectricityFee,
                x.WaterFee,
                x.ServiceFee,
                x.Total,
                x.Status,
                x.BillingMonth,
                x.DueDate,
                x.PaidDate,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpPost("invoices")]
    public async Task<IActionResult> CreateInvoice([FromBody] InvoiceRequest request)
    {
        var entity = new Invoices
        {
            InvoiceCode = request.InvoiceCode.Trim(),
            StudentId = request.StudentId,
            RoomId = request.RoomId,
            UtilityId = request.UtilityId,
            RoomFee = request.RoomFee,
            ElectricityFee = request.ElectricityFee,
            WaterFee = request.WaterFee,
            ServiceFee = request.ServiceFee,
            Total = request.RoomFee + request.ElectricityFee + request.WaterFee + request.ServiceFee,
            Status = request.Status.Trim(),
            BillingMonth = request.BillingMonth,
            DueDate = request.DueDate,
            PaidDate = request.PaidDate
        };

        db.Invoices.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("invoices/{id:int}")]
    public async Task<IActionResult> UpdateInvoice(int id, [FromBody] InvoiceRequest request)
    {
        var entity = await db.Invoices.FindAsync(id);
        if (entity is null) return NotFound();

        entity.InvoiceCode = request.InvoiceCode.Trim();
        entity.StudentId = request.StudentId;
        entity.RoomId = request.RoomId;
        entity.UtilityId = request.UtilityId;
        entity.RoomFee = request.RoomFee;
        entity.ElectricityFee = request.ElectricityFee;
        entity.WaterFee = request.WaterFee;
        entity.ServiceFee = request.ServiceFee;
        entity.Total = request.RoomFee + request.ElectricityFee + request.WaterFee + request.ServiceFee;
        entity.Status = request.Status.Trim();
        entity.BillingMonth = request.BillingMonth;
        entity.DueDate = request.DueDate;
        entity.PaidDate = request.PaidDate;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPost("invoices/{id:int}/mark-paid")]
    public async Task<IActionResult> MarkInvoicePaid(int id, [FromBody] InvoicePaymentRequest request)
    {
        var entity = await db.Invoices.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Status = "Paid";
        entity.PaidDate = request.PaidDate ?? DateTime.Today;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok(new { message = "Đã ghi nhận thanh toán hóa đơn." });
    }

    [HttpDelete("invoices/{id:int}")]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        var entity = await db.Invoices.FindAsync(id);
        if (entity is null) return NotFound();

        db.Invoices.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
