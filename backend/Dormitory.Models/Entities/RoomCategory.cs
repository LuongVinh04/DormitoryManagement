namespace Dormitory.Models.Entities
{
    public class RoomCategory : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string BedLayout { get; set; } = string.Empty;
        public int DefaultCapacity { get; set; }
        public decimal BaseMonthlyFee { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal HygieneFee { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal InternetFee { get; set; }
        public decimal ElectricityUnitPrice { get; set; }
        public decimal WaterUnitPrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ICollection<Rooms> Rooms { get; set; } = new List<Rooms>();
    }
}
