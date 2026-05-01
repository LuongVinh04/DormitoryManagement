namespace Dormitory.Models.Entities
{
    public class RoomFeeProfile : BaseEntity
    {
        public int RoomId { get; set; }
        public decimal MonthlyRoomFee { get; set; }
        public decimal ElectricityUnitPrice { get; set; }
        public decimal WaterUnitPrice { get; set; }
        public decimal HygieneFee { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal InternetFee { get; set; }
        public decimal OtherFee { get; set; }
        public string OtherFeeName { get; set; } = string.Empty;
        public int BillingCycleDay { get; set; } = 10;
        public string Notes { get; set; } = string.Empty;

        public Rooms? Room { get; set; }
    }
}
