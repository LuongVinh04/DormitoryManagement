namespace Dormitory.Models.Entities
{
    public class RoomZone : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? BuildingId { get; set; }
        public string GenderPolicy { get; set; } = "Mixed";
        public int FloorFrom { get; set; }
        public int FloorTo { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public Buildings? Building { get; set; }
        public ICollection<Rooms> Rooms { get; set; } = new List<Rooms>();
    }
}
