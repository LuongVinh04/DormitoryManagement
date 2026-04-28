namespace Dormitory.Models.Entities
{
    public class RoomFinanceRecord : BaseEntity
    {
        public int RoomId { get; set; }
        public int? UtilityId { get; set; }
        public DateTime BillingMonth { get; set; }
        public decimal MonthlyRoomFee { get; set; }
        public decimal ElectricityFee { get; set; }
        public decimal WaterFee { get; set; }
        public decimal HygieneFee { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal InternetFee { get; set; }
        public decimal OtherFee { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } = "Unpaid";
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentNote { get; set; } = string.Empty;
        public string RecordedBy { get; set; } = string.Empty;

        public Rooms? Room { get; set; }
        public Utilities? Utility { get; set; }
        public ICollection<RoomFinanceStudentShare> StudentShares { get; set; } = new List<RoomFinanceStudentShare>();
    }
}
