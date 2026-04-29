using Dormitory.Models.DataContexts;
using Dormitory.Models.Entities;
using DormitoryManagement.Services;
using DormitoryManagement.Services.Facilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace DormitoryManagement.Controllers.StudentPortal;

[ApiController]
[Route("api/student-portal")]
public class StudentPortalController(AppDbContext db, VnPayService vnPayService) : ControllerBase
{
    // ── Đăng ký tài khoản sinh viên ─────────────────────────────────

    [HttpPost("/api/auth/student-register")]
    [AllowAnonymous]
    public async Task<IActionResult> StudentRegister([FromBody] StudentRegisterRequest request)
    {
        // Tìm sinh viên theo mã
        var student = await db.Students.FirstOrDefaultAsync(x => x.StudentCode == request.StudentCode.Trim());
        if (student is null)
        {
            return BadRequest(new { message = "Không tìm thấy sinh viên với mã này trong hệ thống." });
        }

        // Kiểm tra email khớp
        if (!string.Equals(student.Email, request.Email?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Email không khớp với hồ sơ sinh viên." });
        }

        // Kiểm tra đã có tài khoản chưa
        var existingUser = await db.Users.AnyAsync(x => x.StudentId == student.Id);
        if (existingUser)
        {
            return BadRequest(new { message = "Sinh viên đã có tài khoản. Vui lòng đăng nhập." });
        }

        var usernameExists = await db.Users.AnyAsync(x => x.Username == request.Username.Trim());
        if (usernameExists)
        {
            return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });
        }

        // Tìm hoặc tạo role Student
        var studentRole = await db.Roles.FirstOrDefaultAsync(x => x.Name == "Student");
        if (studentRole is null)
        {
            studentRole = new Roles { Name = "Student", Description = "Tài khoản sinh viên" };
            db.Roles.Add(studentRole);
            await db.SaveChangesAsync();
        }

        var user = new Users
        {
            Username = request.Username.Trim(),
            FullName = student.Name,
            Email = student.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = studentRole.Id,
            StudentId = student.Id,
            IsActive = true
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(new { message = "Đăng ký thành công. Bạn có thể đăng nhập ngay." });
    }

    // ── Thông tin sinh viên đang đăng nhập ──────────────────────────

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetStudentProfile()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var user = await db.Users
            .Include(x => x.Student)
            .ThenInclude(s => s!.Room)
            .ThenInclude(r => r!.Building)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user?.Student is null)
        {
            return BadRequest(new { message = "Tài khoản không liên kết với sinh viên." });
        }

        var s = user.Student;
        return Ok(new
        {
            student = new
            {
                s.Id,
                s.StudentCode,
                s.Name,
                s.Gender,
                s.DateOfBirth,
                s.Phone,
                s.Email,
                s.Faculty,
                s.ClassName,
                s.Address,
                s.EmergencyContact,
                s.Status,
                s.RoomId,
                roomNumber = s.Room?.RoomNumber,
                buildingName = s.Room?.Building?.Name
            }
        });
    }

    // ── Phòng ở và bạn cùng phòng ──────────────────────────────────

    [HttpGet("room")]
    [Authorize]
    public async Task<IActionResult> GetStudentRoom()
    {
        var student = await GetCurrentStudent();
        if (student is null) return Unauthorized();

        if (student.RoomId is null)
        {
            return Ok(new { room = (object?)null, roommates = Array.Empty<object>() });
        }

        var room = await db.Rooms
            .Include(x => x.Building)
            .FirstOrDefaultAsync(x => x.Id == student.RoomId);

        var roommates = await db.Students
            .Where(x => x.RoomId == student.RoomId && x.Id != student.Id)
            .Select(x => new
            {
                x.StudentCode,
                x.Name,
                x.Faculty,
                x.ClassName,
                x.Phone
            })
            .ToListAsync();

        return Ok(new
        {
            room = room is null ? null : new
            {
                room.RoomNumber,
                buildingName = room.Building?.Name,
                room.Capacity,
                room.CurrentOccupancy,
                room.Status
            },
            roommates
        });
    }

    // ── Tài chính cá nhân ───────────────────────────────────────────

    [HttpGet("finance")]
    [Authorize]
    public async Task<IActionResult> GetStudentFinance()
    {
        var student = await GetCurrentStudent();
        if (student is null) return Unauthorized();

        var shares = await db.RoomFinanceStudentShares
            .Include(x => x.RoomFinanceRecord)
            .Include(x => x.Invoice)
            .Where(x => x.StudentId == student.Id)
            .OrderByDescending(x => x.RoomFinanceRecord!.BillingMonth)
            .Select(x => new
            {
                x.Id,
                x.RoomFinanceRecordId,
                x.InvoiceId,
                invoiceCode = x.Invoice != null ? x.Invoice.InvoiceCode : string.Empty,
                billingMonth = x.RoomFinanceRecord!.BillingMonth,
                roomTotal = x.RoomFinanceRecord.Total,
                x.ExpectedAmount,
                x.PaidAmount,
                remainingAmount = x.ExpectedAmount - x.PaidAmount,
                x.Status,
                x.PaidDate,
                x.PaymentMethod,
                x.Note,
                dueDate = x.RoomFinanceRecord.DueDate
            })
            .ToListAsync();

        var invoices = await db.Invoices
            .Where(x => x.StudentId == student.Id)
            .OrderByDescending(x => x.BillingMonth)
            .Select(x => new
            {
                x.Id,
                x.InvoiceCode,
                x.Total,
                x.Status,
                x.BillingMonth,
                x.DueDate,
                x.PaidDate
            })
            .ToListAsync();

        var roomId = student.RoomId ?? 0;
        var roommateShares = await db.RoomFinanceStudentShares
            .Include(x => x.Student)
            .Include(x => x.RoomFinanceRecord)
            .Where(x =>
                student.RoomId != null &&
                x.RoomFinanceRecord != null &&
                x.RoomFinanceRecord.RoomId == roomId &&
                x.RoomFinanceRecord.BillingMonth >= DateTime.Today.AddMonths(-6))
            .OrderByDescending(x => x.RoomFinanceRecord!.BillingMonth)
            .ThenBy(x => x.Student!.StudentCode)
            .Select(x => new
            {
                x.Id,
                x.StudentId,
                studentCode = x.Student!.StudentCode,
                studentName = x.Student.Name,
                billingMonth = x.RoomFinanceRecord!.BillingMonth,
                x.ExpectedAmount,
                x.PaidAmount,
                remainingAmount = x.ExpectedAmount - x.PaidAmount,
                x.Status,
                x.PaidDate
            })
            .ToListAsync();

        return Ok(new { shares, invoices, roommateShares });
    }

    [HttpPost("invoices/{invoiceId:int}/vnpay/create")]
    [Authorize]
    public async Task<IActionResult> CreateVnPayPayment(int invoiceId)
    {
        var student = await GetCurrentStudent();
        if (student is null) return Unauthorized();

        var invoice = await db.Invoices
            .FirstOrDefaultAsync(x => x.Id == invoiceId && x.StudentId == student.Id);
        if (invoice is null)
        {
            return NotFound(new { message = "Không tìm thấy hóa đơn của sinh viên đang đăng nhập." });
        }

        if (invoice.Status == "Paid")
        {
            return BadRequest(new { message = "Hóa đơn này đã được thanh toán." });
        }

        if (!vnPayService.IsConfigured)
        {
            return BadRequest(new { message = "VNPay sandbox chưa được cấu hình. Cần nhập TmnCode và HashSecret trong appsettings.json." });
        }

        var share = await db.RoomFinanceStudentShares
            .FirstOrDefaultAsync(x => x.InvoiceId == invoice.Id && x.StudentId == student.Id);
        var amount = share is not null
            ? Math.Max(0, share.ExpectedAmount - share.PaidAmount)
            : invoice.Total;

        if (amount <= 0)
        {
            return BadRequest(new { message = "Hóa đơn không còn số tiền cần thanh toán." });
        }

        try
        {
            var paymentUrl = vnPayService.CreatePaymentUrl(HttpContext, invoice, amount);
            return Ok(new
            {
                paymentMethod = "VNPAY",
                invoiceId = invoice.Id,
                invoice.InvoiceCode,
                amount,
                paymentUrl
            });
        }
        catch (InvalidOperationException error)
        {
            return BadRequest(new { message = error.Message });
        }
    }

    [HttpPost("room-finance-shares/{shareId:int}/vnpay/create")]
    [Authorize]
    public async Task<IActionResult> CreateShareVnPayPayment(int shareId)
    {
        var student = await GetCurrentStudent();
        if (student is null) return Unauthorized();

        var share = await db.RoomFinanceStudentShares
            .Include(x => x.Invoice)
            .Include(x => x.RoomFinanceRecord)
            .FirstOrDefaultAsync(x => x.Id == shareId && x.StudentId == student.Id);

        if (share is null)
        {
            return NotFound(new { message = "Không tìm thấy khoản công nợ của sinh viên đang đăng nhập." });
        }

        if (share.RoomFinanceRecord is null)
        {
            return BadRequest(new { message = "Khoản công nợ chưa liên kết kỳ tài chính phòng." });
        }

        if (share.Status == "Paid" || share.PaidAmount >= share.ExpectedAmount)
        {
            return BadRequest(new { message = "Khoản công nợ này đã được thanh toán." });
        }

        if (!vnPayService.IsConfigured)
        {
            return BadRequest(new { message = "VNPay sandbox chưa được cấu hình. Cần nhập TmnCode và HashSecret trong appsettings.json." });
        }

        var invoice = await EnsureInvoiceForShareAsync(share, student);
        var amount = Math.Max(0, share.ExpectedAmount - share.PaidAmount);

        if (amount <= 0)
        {
            return BadRequest(new { message = "Khoản công nợ không còn số tiền cần thanh toán." });
        }

        try
        {
            var paymentUrl = vnPayService.CreatePaymentUrl(HttpContext, invoice, amount);
            return Ok(new
            {
                paymentMethod = "VNPAY",
                shareId = share.Id,
                invoiceId = invoice.Id,
                invoice.InvoiceCode,
                amount,
                paymentUrl
            });
        }
        catch (InvalidOperationException error)
        {
            return BadRequest(new { message = error.Message });
        }
    }

    [HttpGet("vnpay-return")]
    [AllowAnonymous]
    public async Task<IActionResult> VnPayReturn()
    {
        var txnRef = Request.Query["vnp_TxnRef"].ToString();
        var invoiceId = vnPayService.GetInvoiceIdFromTxnRef(txnRef);

        if (!vnPayService.ValidateReturn(Request.Query))
        {
            return Redirect(vnPayService.BuildClientReturnUrl(false, invoiceId, "Chữ ký VNPay không hợp lệ."));
        }

        if (invoiceId is null)
        {
            return Redirect(vnPayService.BuildClientReturnUrl(false, null, "Không xác định được hóa đơn."));
        }

        var responseCode = Request.Query["vnp_ResponseCode"].ToString();
        var transactionStatus = Request.Query["vnp_TransactionStatus"].ToString();
        if (responseCode != "00" || transactionStatus != "00")
        {
            return Redirect(vnPayService.BuildClientReturnUrl(false, invoiceId, "Thanh toán VNPay chưa thành công."));
        }

        var invoice = await db.Invoices.FirstOrDefaultAsync(x => x.Id == invoiceId.Value);
        if (invoice is null)
        {
            return Redirect(vnPayService.BuildClientReturnUrl(false, invoiceId, "Không tìm thấy hóa đơn."));
        }

        if (invoice.Status == "Paid")
        {
            return Redirect(vnPayService.BuildClientReturnUrl(true, invoice.Id, "Hóa đơn đã được thanh toán."));
        }

        var paidAmount = vnPayService.GetPaidAmount(Request.Query);
        if (paidAmount <= 0)
        {
            return Redirect(vnPayService.BuildClientReturnUrl(false, invoiceId, "Số tiền VNPay trả về không hợp lệ."));
        }

        await ApplyInvoicePaymentAsync(invoice, paidAmount, "VNPAY", DateTime.Now);
        await db.SaveChangesAsync();

        return Redirect(vnPayService.BuildClientReturnUrl(true, invoice.Id, "Thanh toán VNPay thành công."));
    }

    // ── Cập nhật thông tin cá nhân ──────────────────────────────────

    [HttpGet("vnpay-ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> VnPayIpn()
    {
        try
        {
            var txnRef = Request.Query["vnp_TxnRef"].ToString();
            var invoiceId = vnPayService.GetInvoiceIdFromTxnRef(txnRef);

            if (!vnPayService.ValidateReturn(Request.Query))
            {
                return VnPayIpnResponse("97", "Invalid signature");
            }

            if (invoiceId is null)
            {
                return VnPayIpnResponse("01", "Order not found");
            }

            var invoice = await db.Invoices.FirstOrDefaultAsync(x => x.Id == invoiceId.Value);
            if (invoice is null)
            {
                return VnPayIpnResponse("01", "Order not found");
            }

            if (invoice.Status == "Paid")
            {
                return VnPayIpnResponse("02", "Order already confirmed");
            }

            var paidAmount = vnPayService.GetPaidAmount(Request.Query);
            var share = await db.RoomFinanceStudentShares.FirstOrDefaultAsync(x => x.InvoiceId == invoice.Id);
            var expectedAmount = share is not null
                ? Math.Max(0, share.ExpectedAmount - share.PaidAmount)
                : invoice.Total;

            if (paidAmount <= 0 || Math.Round(paidAmount, 0) != Math.Round(expectedAmount, 0))
            {
                return VnPayIpnResponse("04", "Invalid amount");
            }

            var responseCode = Request.Query["vnp_ResponseCode"].ToString();
            var transactionStatus = Request.Query["vnp_TransactionStatus"].ToString();
            if (responseCode == "00" && transactionStatus == "00")
            {
                await ApplyInvoicePaymentAsync(invoice, paidAmount, "VNPAY", DateTime.Now);
                await db.SaveChangesAsync();
            }

            return VnPayIpnResponse("00", "Confirm Success");
        }
        catch
        {
            return VnPayIpnResponse("99", "Unknown error");
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] StudentProfileUpdateRequest request)
    {
        var student = await GetCurrentStudent();
        if (student is null) return Unauthorized();

        student.Phone = request.Phone?.Trim() ?? student.Phone;
        student.Email = request.Email?.Trim() ?? student.Email;
        student.Address = request.Address?.Trim() ?? student.Address;
        student.EmergencyContact = request.EmergencyContact?.Trim() ?? student.EmergencyContact;
        student.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(new { message = "Đã cập nhật thông tin cá nhân." });
    }

    // ── Yêu cầu chuyển phòng ────────────────────────────────────────

    [HttpGet("transfer-requests")]
    [Authorize]
    public async Task<IActionResult> GetMyTransferRequests()
    {
        var student = await GetCurrentStudent();
        if (student is null) return Unauthorized();

        var requests = await db.RoomTransferRequests
            .Include(x => x.CurrentRoom).ThenInclude(r => r!.Building)
            .Include(x => x.DesiredRoom).ThenInclude(r => r!.Building)
            .Where(x => x.StudentId == student.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                currentRoom = $"{x.CurrentRoom!.RoomNumber} ({x.CurrentRoom.Building!.Name})",
                desiredRoom = $"{x.DesiredRoom!.RoomNumber} ({x.DesiredRoom.Building!.Name})",
                x.Reason,
                x.Status,
                x.DecisionDate,
                x.DecisionNote,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(requests);
    }

    [HttpPost("transfer-requests")]
    [Authorize]
    public async Task<IActionResult> CreateTransferRequest([FromBody] TransferRequestCreate request)
    {
        var student = await GetCurrentStudent();
        if (student is null) return Unauthorized();

        if (student.RoomId is null)
        {
            return BadRequest(new { message = "Bạn chưa có phòng để chuyển." });
        }

        var entity = new RoomTransferRequest
        {
            StudentId = student.Id,
            CurrentRoomId = student.RoomId.Value,
            DesiredRoomId = request.DesiredRoomId,
            Reason = request.Reason.Trim(),
            Status = "Pending"
        };

        db.RoomTransferRequests.Add(entity);
        await db.SaveChangesAsync();

        return Ok(new { message = "Đã gửi yêu cầu chuyển phòng." });
    }

    // ── Chat ─────────────────────────────────────────────────────────

    [HttpGet("messages")]
    [Authorize]
    public async Task<IActionResult> GetMessages()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var messages = await db.ChatMessages
            .Include(x => x.Sender)
            .Include(x => x.Receiver)
            .Where(x => x.SenderId == userId || x.ReceiverId == userId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.SenderId,
                senderName = x.Sender!.FullName,
                x.ReceiverId,
                receiverName = x.Receiver!.FullName,
                x.Content,
                x.IsRead,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(messages);
    }

    [HttpPost("messages")]
    [Authorize]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var message = new ChatMessage
        {
            SenderId = userId.Value,
            ReceiverId = request.ReceiverId,
            Content = request.Content.Trim()
        };

        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        return Ok(new { message = "Đã gửi tin nhắn." });
    }

    [HttpGet("managers")]
    [Authorize]
    public async Task<IActionResult> GetManagers()
    {
        var managers = await db.Users
            .Include(x => x.Role)
            .Where(x => x.IsActive && x.StudentId == null)
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.Username,
                roleName = x.Role != null ? x.Role.Name : ""
            })
            .ToListAsync();

        return Ok(managers);
    }

    // ── Admin: Transfer management ──────────────────────────────────

    [HttpGet("/api/operations/transfer-requests")]
    [Authorize]
    public async Task<IActionResult> GetAllTransferRequests()
    {
        var requests = await db.RoomTransferRequests
            .Include(x => x.Student)
            .Include(x => x.CurrentRoom).ThenInclude(r => r!.Building)
            .Include(x => x.DesiredRoom).ThenInclude(r => r!.Building)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.StudentId,
                studentCode = x.Student!.StudentCode,
                studentName = x.Student.Name,
                currentRoom = $"{x.CurrentRoom!.RoomNumber} ({x.CurrentRoom.Building!.Name})",
                desiredRoom = $"{x.DesiredRoom!.RoomNumber} ({x.DesiredRoom.Building!.Name})",
                x.DesiredRoomId,
                x.Reason,
                x.Status,
                x.DecisionDate,
                x.DecisionNote,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(requests);
    }

    [HttpPost("/api/operations/transfer-requests/{id:int}/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveTransfer(int id)
    {
        var request = await db.RoomTransferRequests
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (request is null) return NotFound();

        if (request.Status != "Pending")
        {
            return BadRequest(new { message = "Yêu cầu đã được xử lý." });
        }

        // Execute the transfer via workflow
        var student = request.Student!;
        var oldRoomId = student.RoomId;
        student.RoomId = request.DesiredRoomId;
        student.UpdatedAt = DateTime.UtcNow;

        // Update room counts
        if (oldRoomId.HasValue)
        {
            await RoomOccupancyService.RecalculateRoomAsync(db, oldRoomId.Value);
        }
        await RoomOccupancyService.RecalculateRoomAsync(db, request.DesiredRoomId);

        request.Status = "Approved";
        request.DecisionDate = DateTime.UtcNow;
        request.DecisionNote = "Đã duyệt chuyển phòng.";
        request.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(new { message = "Đã duyệt và chuyển phòng cho sinh viên." });
    }

    [HttpPost("/api/operations/transfer-requests/{id:int}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectTransfer(int id, [FromBody] TransferDecisionRequest request)
    {
        var entity = await db.RoomTransferRequests.FindAsync(id);
        if (entity is null) return NotFound();

        if (entity.Status != "Pending")
        {
            return BadRequest(new { message = "Yêu cầu đã được xử lý." });
        }

        entity.Status = "Rejected";
        entity.DecisionDate = DateTime.UtcNow;
        entity.DecisionNote = request.Note?.Trim() ?? "Từ chối yêu cầu chuyển phòng.";
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(new { message = "Đã từ chối yêu cầu chuyển phòng." });
    }

    // ── Admin: Chat messages ────────────────────────────────────────

    [HttpGet("/api/operations/messages")]
    [Authorize]
    public async Task<IActionResult> GetAllMessages()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var messages = await db.ChatMessages
            .Include(x => x.Sender)
            .Include(x => x.Receiver)
            .Where(x => x.SenderId == userId || x.ReceiverId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .Select(x => new
            {
                x.Id,
                x.SenderId,
                senderName = x.Sender!.FullName,
                x.ReceiverId,
                receiverName = x.Receiver!.FullName,
                x.Content,
                x.IsRead,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(messages);
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private IActionResult VnPayIpnResponse(string rspCode, string message)
    {
        return Ok(new Dictionary<string, string>
        {
            ["RspCode"] = rspCode,
            ["Message"] = message
        });
    }

    private async Task ApplyInvoicePaymentAsync(Invoices invoice, decimal paidAmount, string paymentMethod, DateTime paidDate)
    {
        var share = await db.RoomFinanceStudentShares
            .FirstOrDefaultAsync(x => x.InvoiceId == invoice.Id);

        if (share is null)
        {
            invoice.Status = "Paid";
            invoice.PaidDate = paidDate;
            invoice.UpdatedAt = DateTime.UtcNow;
            return;
        }

        var amount = paidAmount > 0 ? paidAmount : share.ExpectedAmount - share.PaidAmount;
        share.PaidAmount = Math.Min(share.ExpectedAmount, share.PaidAmount + amount);
        share.PaidDate = paidDate;
        share.PaymentMethod = paymentMethod;
        share.Status = share.PaidAmount >= share.ExpectedAmount ? "Paid" : "PartiallyPaid";
        share.UpdatedAt = DateTime.UtcNow;

        invoice.Status = share.Status;
        invoice.PaidDate = share.Status == "Paid" ? paidDate : null;
        invoice.UpdatedAt = DateTime.UtcNow;

        await DormitoryManagement.Services.Operations.DormitoryWorkflowService.UpdateRoomFinanceFromSharesAsync(
            db,
            share.RoomFinanceRecordId,
            paidDate);
    }

    private async Task<Invoices> EnsureInvoiceForShareAsync(RoomFinanceStudentShare share, Students student)
    {
        if (share.Invoice is not null)
        {
            return share.Invoice;
        }

        if (share.InvoiceId.HasValue)
        {
            var linkedInvoice = await db.Invoices.FirstOrDefaultAsync(x => x.Id == share.InvoiceId.Value && x.StudentId == student.Id);
            if (linkedInvoice is not null)
            {
                share.Invoice = linkedInvoice;
                return linkedInvoice;
            }
        }

        var record = share.RoomFinanceRecord!;
        var invoice = new Invoices
        {
            InvoiceCode = $"INV-{record.BillingMonth:yyyyMM}-{student.StudentCode}-{share.Id}",
            StudentId = student.Id,
            RoomId = record.RoomId,
            UtilityId = record.UtilityId,
            RoomFee = Math.Round(share.ExpectedAmount, 0),
            ElectricityFee = 0,
            WaterFee = 0,
            ServiceFee = 0,
            Total = Math.Round(share.ExpectedAmount, 0),
            Status = share.Status == "Paid" ? "Paid" : "Unpaid",
            BillingMonth = record.BillingMonth,
            DueDate = record.DueDate,
            PaidDate = share.Status == "Paid" ? share.PaidDate : null,
            CreatedAt = DateTime.UtcNow
        };

        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        share.InvoiceId = invoice.Id;
        share.Invoice = invoice;
        share.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return invoice;
    }

    private int? GetCurrentUserId()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdStr, out int userId) ? userId : null;
    }

    private async Task<Students?> GetCurrentStudent()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return null;

        var user = await db.Users.Include(x => x.Student).FirstOrDefaultAsync(x => x.Id == userId);
        return user?.Student;
    }
}

// ── Request models ──────────────────────────────────────────────────

public class StudentRegisterRequest
{
    public string StudentCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class StudentProfileUpdateRequest
{
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
}

public class TransferRequestCreate
{
    public int DesiredRoomId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class TransferDecisionRequest
{
    public string? Note { get; set; }
}

public class SendMessageRequest
{
    public int ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
}
