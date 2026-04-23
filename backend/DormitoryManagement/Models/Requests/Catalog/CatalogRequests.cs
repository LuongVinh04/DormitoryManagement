namespace DormitoryManagement.Models;

public class RoomCategoryRequest
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
}

public class RoomZoneRequest
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
}

public class PaymentMethodCatalogRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public decimal ProcessingFee { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
