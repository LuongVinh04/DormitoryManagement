namespace Dormitory.Models.Entities
{
    public class RoomTransferRequest : BaseEntity
    {
        public int StudentId { get; set; }
        public int CurrentRoomId { get; set; }
        public int DesiredRoomId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending | Approved | Rejected
        public DateTime? DecisionDate { get; set; }
        public string DecisionNote { get; set; } = string.Empty;

        public Students? Student { get; set; }
        public Rooms? CurrentRoom { get; set; }
        public Rooms? DesiredRoom { get; set; }
    }
}
