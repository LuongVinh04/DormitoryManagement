namespace DormitoryManagement.Models;

public class BuildingRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string GenderPolicy { get; set; } = "Mixed";
    public int NumberOfFloors { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class RoomRequest
{
    public string RoomNumber { get; set; } = string.Empty;
    public int BuildingId { get; set; }
    public int? RoomCategoryId { get; set; }
    public int? RoomZoneId { get; set; }
    public int FloorNumber { get; set; }
    public string RoomType { get; set; } = "Standard";
    public int Capacity { get; set; }
    public decimal PricePerMonth { get; set; }
    public string Status { get; set; } = "Available";
}

public class AssignStudentRequest
{
    public int StudentId { get; set; }
    public string Status { get; set; } = "Active";
    public string Note { get; set; } = string.Empty;
}

public class TransferStudentRequest
{
    public int StudentId { get; set; }
    public int ToRoomId { get; set; }
    public string Status { get; set; } = "Active";
    public string Note { get; set; } = string.Empty;
}

public class RemoveStudentRequest
{
    public int StudentId { get; set; }
    public string Status { get; set; } = "Waiting";
    public string Note { get; set; } = string.Empty;
}
