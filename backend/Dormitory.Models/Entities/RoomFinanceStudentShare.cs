namespace Dormitory.Models.Entities
{
    public class RoomFinanceStudentShare : BaseEntity
    {
        public int RoomFinanceRecordId { get; set; }
        public int StudentId { get; set; }
        public int? InvoiceId { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } = "Unpaid";
        public DateTime? PaidDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;

        public RoomFinanceRecord? RoomFinanceRecord { get; set; }
        public Students? Student { get; set; }
        public Invoices? Invoice { get; set; }
    }
}
