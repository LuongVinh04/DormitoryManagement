using ClosedXML.Excel;
using Dormitory.Models.DataContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Controllers.Reports;

[ApiController]
[Route("api/export")]
[Authorize]
public class ExportController(AppDbContext db) : ControllerBase
{
    [HttpGet("students")]
    public async Task<IActionResult> ExportStudents()
    {
        var students = await db.Students
            .Include(x => x.Room).ThenInclude(r => r!.Building)
            .OrderBy(x => x.StudentCode)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Sinh viên");

        ws.Cell(1, 1).Value = "Mã SV";
        ws.Cell(1, 2).Value = "Họ tên";
        ws.Cell(1, 3).Value = "Giới tính";
        ws.Cell(1, 4).Value = "Ngày sinh";
        ws.Cell(1, 5).Value = "SĐT";
        ws.Cell(1, 6).Value = "Email";
        ws.Cell(1, 7).Value = "Khoa";
        ws.Cell(1, 8).Value = "Lớp";
        ws.Cell(1, 9).Value = "Phòng";
        ws.Cell(1, 10).Value = "Tòa nhà";
        ws.Cell(1, 11).Value = "Trạng thái";

        var headerRow = ws.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;

        for (int i = 0; i < students.Count; i++)
        {
            var s = students[i];
            var r = i + 2;
            ws.Cell(r, 1).Value = s.StudentCode;
            ws.Cell(r, 2).Value = s.Name;
            ws.Cell(r, 3).Value = s.Gender;
            ws.Cell(r, 4).Value = s.DateOfBirth.ToString("dd/MM/yyyy");
            ws.Cell(r, 5).Value = s.Phone;
            ws.Cell(r, 6).Value = s.Email;
            ws.Cell(r, 7).Value = s.Faculty;
            ws.Cell(r, 8).Value = s.ClassName;
            ws.Cell(r, 9).Value = s.Room?.RoomNumber ?? "";
            ws.Cell(r, 10).Value = s.Room?.Building?.Name ?? "";
            ws.Cell(r, 11).Value = s.Status;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "danh-sach-sinh-vien.xlsx");
    }

    [HttpGet("room-finances")]
    public async Task<IActionResult> ExportRoomFinances()
    {
        var records = await db.RoomFinanceRecords
            .Include(x => x.Room).ThenInclude(r => r!.Building)
            .OrderByDescending(x => x.BillingMonth)
            .ThenBy(x => x.Room!.RoomNumber)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Công nợ phòng");

        string[] headers = { "Phòng", "Tòa nhà", "Kỳ", "Tiền phòng", "Tiền điện", "Tiền nước", "Vệ sinh", "Dịch vụ", "Internet", "Khác", "Tổng", "Đã thu", "Còn lại", "Trạng thái", "Hạn thu" };
        for (int c = 0; c < headers.Length; c++)
        {
            ws.Cell(1, c + 1).Value = headers[c];
        }
        var headerRow = ws.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;

        for (int i = 0; i < records.Count; i++)
        {
            var rec = records[i];
            var r = i + 2;
            ws.Cell(r, 1).Value = rec.Room?.RoomNumber ?? "";
            ws.Cell(r, 2).Value = rec.Room?.Building?.Name ?? "";
            ws.Cell(r, 3).Value = rec.BillingMonth.ToString("MM/yyyy");
            ws.Cell(r, 4).Value = (double)rec.MonthlyRoomFee;
            ws.Cell(r, 5).Value = (double)rec.ElectricityFee;
            ws.Cell(r, 6).Value = (double)rec.WaterFee;
            ws.Cell(r, 7).Value = (double)rec.HygieneFee;
            ws.Cell(r, 8).Value = (double)rec.ServiceFee;
            ws.Cell(r, 9).Value = (double)rec.InternetFee;
            ws.Cell(r, 10).Value = (double)rec.OtherFee;
            ws.Cell(r, 11).Value = (double)rec.Total;
            ws.Cell(r, 12).Value = (double)rec.PaidAmount;
            ws.Cell(r, 13).Value = (double)(rec.Total - rec.PaidAmount);
            ws.Cell(r, 14).Value = rec.Status;
            ws.Cell(r, 15).Value = rec.DueDate.ToString("dd/MM/yyyy");

            // Number format for currency columns
            for (int c = 4; c <= 13; c++)
                ws.Cell(r, c).Style.NumberFormat.Format = "#,##0";
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "cong-no-phong.xlsx");
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> ExportInvoices()
    {
        var invoices = await db.Invoices
            .Include(x => x.Student)
            .Include(x => x.Room).ThenInclude(r => r!.Building)
            .OrderByDescending(x => x.BillingMonth)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Hóa đơn");

        string[] headers = { "Mã HĐ", "Sinh viên", "Phòng", "Tòa", "Tiền phòng", "Tiền điện", "Tiền nước", "Dịch vụ", "Tổng", "Trạng thái", "Kỳ", "Hạn", "Ngày thu" };
        for (int c = 0; c < headers.Length; c++)
            ws.Cell(1, c + 1).Value = headers[c];

        var headerRow = ws.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;

        for (int i = 0; i < invoices.Count; i++)
        {
            var inv = invoices[i];
            var r = i + 2;
            ws.Cell(r, 1).Value = inv.InvoiceCode;
            ws.Cell(r, 2).Value = inv.Student?.Name ?? "";
            ws.Cell(r, 3).Value = inv.Room?.RoomNumber ?? "";
            ws.Cell(r, 4).Value = inv.Room?.Building?.Name ?? "";
            ws.Cell(r, 5).Value = (double)inv.RoomFee;
            ws.Cell(r, 6).Value = (double)inv.ElectricityFee;
            ws.Cell(r, 7).Value = (double)inv.WaterFee;
            ws.Cell(r, 8).Value = (double)inv.ServiceFee;
            ws.Cell(r, 9).Value = (double)inv.Total;
            ws.Cell(r, 10).Value = inv.Status;
            ws.Cell(r, 11).Value = inv.BillingMonth.ToString("MM/yyyy");
            ws.Cell(r, 12).Value = inv.DueDate.ToString("dd/MM/yyyy");
            ws.Cell(r, 13).Value = inv.PaidDate?.ToString("dd/MM/yyyy") ?? "";

            for (int c = 5; c <= 9; c++)
                ws.Cell(r, c).Style.NumberFormat.Format = "#,##0";
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "hoa-don.xlsx");
    }
}
