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
    public int FloorNumber { get; set; }
    public string RoomType { get; set; } = "Standard";
    public int Capacity { get; set; }
    public decimal PricePerMonth { get; set; }
    public string Status { get; set; } = "Available";
}

public class StudentRequest
{
    public string StudentCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Faculty { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public int? RoomId { get; set; }
}

public class RegistrationRequest
{
    public int StudentId { get; set; }
    public int RoomId { get; set; }
    public DateTime RegistrationDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string Note { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}

public class ContractRequest
{
    public string ContractCode { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public int RoomId { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal MonthlyFee { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active";
}

public class UtilityRequest
{
    public int RoomId { get; set; }
    public int ElectricityOld { get; set; }
    public int ElectricityNew { get; set; }
    public int WaterOld { get; set; }
    public int WaterNew { get; set; }
    public decimal ElectricityUnitPrice { get; set; }
    public decimal WaterUnitPrice { get; set; }
    public DateTime BillingMonth { get; set; }
}

public class InvoiceRequest
{
    public string InvoiceCode { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public int RoomId { get; set; }
    public int? UtilityId { get; set; }
    public decimal RoomFee { get; set; }
    public decimal ElectricityFee { get; set; }
    public decimal WaterFee { get; set; }
    public decimal ServiceFee { get; set; }
    public string Status { get; set; } = "Unpaid";
    public DateTime BillingMonth { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
}

public class UserRequest
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class RoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class StudentStatusRequest
{
    public string Status { get; set; } = "Active";
}

public class UserProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
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

public class RegistrationDecisionRequest
{
    public string Note { get; set; } = string.Empty;
    public DateTime? DecisionDate { get; set; }
}

public class InvoicePaymentRequest
{
    public DateTime? PaidDate { get; set; }
}

public class RoomFeeProfileRequest
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
}

public class RoomFinanceRecordRequest
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
    public DateTime DueDate { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = "Unpaid";
    public DateTime? PaidDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentNote { get; set; } = string.Empty;
    public string RecordedBy { get; set; } = string.Empty;
}

public class RoomFinancePaymentRequest
{
    public decimal PaidAmount { get; set; }
    public DateTime? PaidDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentNote { get; set; } = string.Empty;
    public string RecordedBy { get; set; } = string.Empty;
}
