using Dormitory.Models.DataContexts;
using Dormitory.Models.Entities;
using DormitoryManagement.Services.Facilities;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Services.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await DatabaseSchemaUpdater.EnsureFinancialSchemaAsync(db);

        if (await db.Buildings.AnyAsync())
        {
            await SeedCatalogDefaultsAsync(db);
            await SeedFinanceDefaultsAsync(db);
            return;
        }

        var now = DateTime.UtcNow;

        var roles = new List<Roles>
        {
            new() { Name = "Admin", Description = "Quan tri he thong", CreatedAt = now },
            new() { Name = "Manager", Description = "Quan ly ky tuc xa", CreatedAt = now },
            new() { Name = "Accountant", Description = "Quan ly hoa don", CreatedAt = now }
        };
        db.Roles.AddRange(roles);
        await db.SaveChangesAsync();

        var users = new List<Users>
        {
            new() { Username = "admin", FullName = "System Admin", Email = "admin@dorm.local", PasswordHash = "admin123", RoleId = roles[0].Id, IsActive = true, CreatedAt = now },
            new() { Username = "manager", FullName = "Dormitory Manager", Email = "manager@dorm.local", PasswordHash = "manager123", RoleId = roles[1].Id, IsActive = true, CreatedAt = now }
        };
        db.Users.AddRange(users);

        var buildings = new List<Buildings>
        {
            new() { Code = "A", Name = "Khu A", GenderPolicy = "Male", NumberOfFloors = 5, ManagerName = "Nguyen Van A", Description = "Khu nha nam sinh", CreatedAt = now },
            new() { Code = "B", Name = "Khu B", GenderPolicy = "Female", NumberOfFloors = 6, ManagerName = "Tran Thi B", Description = "Khu nha nu sinh", CreatedAt = now }
        };
        db.Buildings.AddRange(buildings);
        await db.SaveChangesAsync();

        await SeedCatalogDefaultsAsync(db);
        var roomCategories = await db.RoomCategories.ToListAsync();
        var roomZones = await db.RoomZones.ToListAsync();
        var singleRoom = roomCategories.First(x => x.Code == "SINGLE");
        var doubleRoom = roomCategories.First(x => x.Code == "DOUBLE");
        var bunkRoom = roomCategories.First(x => x.Code == "BUNK");
        var zoneA = roomZones.First(x => x.Code == "A-LOW");
        var zoneB = roomZones.First(x => x.Code == "B-FEMALE");

        var rooms = new List<Rooms>
        {
            new() { BuildingId = buildings[0].Id, RoomCategoryId = doubleRoom.Id, RoomZoneId = zoneA.Id, RoomNumber = "A101", FloorNumber = 1, RoomType = doubleRoom.Code, Capacity = doubleRoom.DefaultCapacity, CurrentOccupancy = 2, PricePerMonth = doubleRoom.BaseMonthlyFee, Status = "Occupied", CreatedAt = now },
            new() { BuildingId = buildings[0].Id, RoomCategoryId = singleRoom.Id, RoomZoneId = zoneA.Id, RoomNumber = "A102", FloorNumber = 1, RoomType = singleRoom.Code, Capacity = singleRoom.DefaultCapacity, CurrentOccupancy = 1, PricePerMonth = singleRoom.BaseMonthlyFee, Status = "Occupied", CreatedAt = now },
            new() { BuildingId = buildings[1].Id, RoomCategoryId = bunkRoom.Id, RoomZoneId = zoneB.Id, RoomNumber = "B201", FloorNumber = 2, RoomType = bunkRoom.Code, Capacity = bunkRoom.DefaultCapacity, CurrentOccupancy = 2, PricePerMonth = bunkRoom.BaseMonthlyFee, Status = "Occupied", CreatedAt = now },
            new() { BuildingId = buildings[1].Id, RoomCategoryId = bunkRoom.Id, RoomZoneId = zoneB.Id, RoomNumber = "B202", FloorNumber = 2, RoomType = bunkRoom.Code, Capacity = bunkRoom.DefaultCapacity, CurrentOccupancy = 0, PricePerMonth = bunkRoom.BaseMonthlyFee, Status = "Available", CreatedAt = now }
        };
        db.Rooms.AddRange(rooms);
        await db.SaveChangesAsync();

        var students = new List<Students>
        {
            new() { StudentCode = "SV001", Name = "Le Minh Anh", Gender = "Male", DateOfBirth = new DateTime(2004, 5, 12), Phone = "0901000001", Email = "sv001@uni.edu.vn", Faculty = "CNTT", ClassName = "CTK46A", Address = "Da Nang", EmergencyContact = "0909000001", Status = "Active", RoomId = rooms[0].Id, CreatedAt = now },
            new() { StudentCode = "SV002", Name = "Pham Gia Bao", Gender = "Male", DateOfBirth = new DateTime(2004, 9, 1), Phone = "0901000002", Email = "sv002@uni.edu.vn", Faculty = "Kinh te", ClassName = "KT46B", Address = "Quang Nam", EmergencyContact = "0909000002", Status = "Active", RoomId = rooms[0].Id, CreatedAt = now },
            new() { StudentCode = "SV003", Name = "Tran Ngoc Lan", Gender = "Female", DateOfBirth = new DateTime(2005, 1, 9), Phone = "0901000003", Email = "sv003@uni.edu.vn", Faculty = "Ngoai ngu", ClassName = "NN47A", Address = "Hue", EmergencyContact = "0909000003", Status = "Active", RoomId = rooms[2].Id, CreatedAt = now },
            new() { StudentCode = "SV004", Name = "Vo Thi My", Gender = "Female", DateOfBirth = new DateTime(2005, 3, 20), Phone = "0901000004", Email = "sv004@uni.edu.vn", Faculty = "Du lich", ClassName = "DL47A", Address = "Quang Tri", EmergencyContact = "0909000004", Status = "PendingMoveIn", RoomId = rooms[2].Id, CreatedAt = now },
            new() { StudentCode = "SV005", Name = "Nguyen Tuan Kiet", Gender = "Male", DateOfBirth = new DateTime(2006, 11, 15), Phone = "0901000005", Email = "sv005@uni.edu.vn", Faculty = "Dien tu", ClassName = "DT48A", Address = "Quang Ngai", EmergencyContact = "0909000005", Status = "Waiting", RoomId = null, CreatedAt = now }
        };
        db.Students.AddRange(students);
        await db.SaveChangesAsync();

        var registrations = new List<Registrations>
        {
            new() { StudentId = students[4].Id, RoomId = rooms[1].Id, RegistrationDate = now.AddDays(-4), Note = "Dang cho xet duyet", Status = "Pending", CreatedAt = now },
            new() { StudentId = students[2].Id, RoomId = rooms[2].Id, RegistrationDate = now.AddMonths(-1), ApprovedDate = now.AddMonths(-1).AddDays(2), Note = "Da duyet", Status = "Approved", CreatedAt = now }
        };
        db.Registrations.AddRange(registrations);

        var contracts = new List<Contract>
        {
            new() { ContractCode = "HD-2026-001", StudentId = students[0].Id, RoomId = rooms[0].Id, DepositAmount = 1000000, MonthlyFee = rooms[0].PricePerMonth, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), Status = "Active", CreatedAt = now },
            new() { ContractCode = "HD-2026-002", StudentId = students[2].Id, RoomId = rooms[2].Id, DepositAmount = 1000000, MonthlyFee = rooms[2].PricePerMonth, StartDate = new DateTime(2026, 2, 1), EndDate = new DateTime(2026, 12, 31), Status = "Active", CreatedAt = now }
        };
        db.Contracts.AddRange(contracts);
        await db.SaveChangesAsync();

        var utilities = new List<Utilities>
        {
            new() { RoomId = rooms[0].Id, ElectricityOld = 1200, ElectricityNew = 1280, WaterOld = 450, WaterNew = 470, ElectricityUnitPrice = 3500, WaterUnitPrice = 15000, BillingMonth = new DateTime(2026, 4, 1), CreatedAt = now },
            new() { RoomId = rooms[2].Id, ElectricityOld = 980, ElectricityNew = 1030, WaterOld = 300, WaterNew = 320, ElectricityUnitPrice = 3500, WaterUnitPrice = 15000, BillingMonth = new DateTime(2026, 4, 1), CreatedAt = now }
        };
        db.Utilities.AddRange(utilities);
        await db.SaveChangesAsync();

        var invoices = new List<Invoices>
        {
            new()
            {
                InvoiceCode = "INV-2026-0401",
                StudentId = students[0].Id,
                RoomId = rooms[0].Id,
                UtilityId = utilities[0].Id,
                RoomFee = rooms[0].PricePerMonth,
                ElectricityFee = (utilities[0].ElectricityNew - utilities[0].ElectricityOld) * utilities[0].ElectricityUnitPrice,
                WaterFee = (utilities[0].WaterNew - utilities[0].WaterOld) * utilities[0].WaterUnitPrice,
                ServiceFee = 100000,
                Total = rooms[0].PricePerMonth + ((utilities[0].ElectricityNew - utilities[0].ElectricityOld) * utilities[0].ElectricityUnitPrice) + ((utilities[0].WaterNew - utilities[0].WaterOld) * utilities[0].WaterUnitPrice) + 100000,
                Status = "Paid",
                BillingMonth = new DateTime(2026, 4, 1),
                DueDate = new DateTime(2026, 4, 10),
                PaidDate = new DateTime(2026, 4, 8),
                CreatedAt = now
            },
            new()
            {
                InvoiceCode = "INV-2026-0402",
                StudentId = students[2].Id,
                RoomId = rooms[2].Id,
                UtilityId = utilities[1].Id,
                RoomFee = rooms[2].PricePerMonth,
                ElectricityFee = (utilities[1].ElectricityNew - utilities[1].ElectricityOld) * utilities[1].ElectricityUnitPrice,
                WaterFee = (utilities[1].WaterNew - utilities[1].WaterOld) * utilities[1].WaterUnitPrice,
                ServiceFee = 100000,
                Total = rooms[2].PricePerMonth + ((utilities[1].ElectricityNew - utilities[1].ElectricityOld) * utilities[1].ElectricityUnitPrice) + ((utilities[1].WaterNew - utilities[1].WaterOld) * utilities[1].WaterUnitPrice) + 100000,
                Status = "Unpaid",
                BillingMonth = new DateTime(2026, 4, 1),
                DueDate = new DateTime(2026, 4, 10),
                CreatedAt = now
            }
        };
        db.Invoices.AddRange(invoices);
        await db.SaveChangesAsync();

        foreach (var room in rooms)
        {
            await RoomOccupancyService.RecalculateRoomAsync(db, room.Id);
        }

        await db.SaveChangesAsync();
        await SeedFinanceDefaultsAsync(db);
    }

    private static async Task SeedFinanceDefaultsAsync(AppDbContext db)
    {
        var now = DateTime.UtcNow;
        var rooms = await db.Rooms.Include(x => x.RoomCategory).AsNoTracking().ToListAsync();
        var existingProfileRoomIds = await db.RoomFeeProfiles.Select(x => x.RoomId).ToListAsync();

        var newProfiles = rooms
            .Where(room => !existingProfileRoomIds.Contains(room.Id))
            .Select(room => new RoomFeeProfile
            {
                RoomId = room.Id,
                MonthlyRoomFee = room.PricePerMonth,
                ElectricityUnitPrice = room.RoomCategory?.ElectricityUnitPrice ?? 3500,
                WaterUnitPrice = room.RoomCategory?.WaterUnitPrice ?? 15000,
                HygieneFee = room.RoomCategory?.HygieneFee ?? 80000,
                ServiceFee = room.RoomCategory?.ServiceFee ?? 100000,
                InternetFee = room.RoomCategory?.InternetFee ?? 120000,
                OtherFee = 0,
                OtherFeeName = string.Empty,
                BillingCycleDay = 10,
                Notes = "Cau hinh phi mac dinh theo phong",
                CreatedAt = now
            })
            .ToList();

        if (newProfiles.Count > 0)
        {
            db.RoomFeeProfiles.AddRange(newProfiles);
            await db.SaveChangesAsync();
        }

        var financeRecordKeys = await db.RoomFinanceRecords
            .Select(x => new { x.RoomId, x.BillingMonth })
            .ToListAsync();

        var existingFinanceKeySet = financeRecordKeys
            .Select(x => $"{x.RoomId}:{x.BillingMonth:yyyyMM}")
            .ToHashSet();

        var profiles = await db.RoomFeeProfiles.AsNoTracking().ToListAsync();
        var utilities = await db.Utilities.AsNoTracking().ToListAsync();

        var financeRecords = new List<RoomFinanceRecord>();

        foreach (var utility in utilities)
        {
            var key = $"{utility.RoomId}:{utility.BillingMonth:yyyyMM}";
            if (existingFinanceKeySet.Contains(key))
            {
                continue;
            }

            var room = rooms.FirstOrDefault(x => x.Id == utility.RoomId);
            var profile = profiles.FirstOrDefault(x => x.RoomId == utility.RoomId);
            if (room is null || profile is null)
            {
                continue;
            }

            var electricityFee = Math.Max(0, utility.ElectricityNew - utility.ElectricityOld) * utility.ElectricityUnitPrice;
            var waterFee = Math.Max(0, utility.WaterNew - utility.WaterOld) * utility.WaterUnitPrice;
            var total = profile.MonthlyRoomFee + electricityFee + waterFee + profile.HygieneFee + profile.ServiceFee + profile.InternetFee + profile.OtherFee;

            financeRecords.Add(new RoomFinanceRecord
            {
                RoomId = utility.RoomId,
                UtilityId = utility.Id,
                BillingMonth = utility.BillingMonth,
                MonthlyRoomFee = profile.MonthlyRoomFee,
                ElectricityFee = electricityFee,
                WaterFee = waterFee,
                HygieneFee = profile.HygieneFee,
                ServiceFee = profile.ServiceFee,
                InternetFee = profile.InternetFee,
                OtherFee = profile.OtherFee,
                Total = total,
                PaidAmount = 0,
                Status = "Unpaid",
                DueDate = utility.BillingMonth.AddDays(profile.BillingCycleDay),
                PaymentMethod = string.Empty,
                PaymentNote = "Sinh tu du lieu dien nuoc",
                RecordedBy = "system",
                CreatedAt = now
            });
        }

        if (financeRecords.Count > 0)
        {
            db.RoomFinanceRecords.AddRange(financeRecords);
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedCatalogDefaultsAsync(AppDbContext db)
    {
        var now = DateTime.UtcNow;

        if (!await db.RoomCategories.AnyAsync())
        {
            db.RoomCategories.AddRange(
                new RoomCategory
                {
                    Code = "SINGLE",
                    Name = "Phòng đơn",
                    BedLayout = "1 giường đơn",
                    DefaultCapacity = 1,
                    BaseMonthlyFee = 2500000,
                    DepositAmount = 2500000,
                    HygieneFee = 80000,
                    ServiceFee = 150000,
                    InternetFee = 120000,
                    ElectricityUnitPrice = 3500,
                    WaterUnitPrice = 15000,
                    Description = "Phòng riêng cho một sinh viên hoặc cán bộ lưu trú.",
                    CreatedAt = now
                },
                new RoomCategory
                {
                    Code = "DOUBLE",
                    Name = "Phòng đôi",
                    BedLayout = "2 giường đơn",
                    DefaultCapacity = 2,
                    BaseMonthlyFee = 1800000,
                    DepositAmount = 1800000,
                    HygieneFee = 80000,
                    ServiceFee = 120000,
                    InternetFee = 120000,
                    ElectricityUnitPrice = 3500,
                    WaterUnitPrice = 15000,
                    Description = "Phòng tiêu chuẩn cho hai sinh viên.",
                    CreatedAt = now
                },
                new RoomCategory
                {
                    Code = "BUNK",
                    Name = "Phòng giường tầng",
                    BedLayout = "3 giường tầng, tối đa 6 người",
                    DefaultCapacity = 6,
                    BaseMonthlyFee = 1200000,
                    DepositAmount = 1200000,
                    HygieneFee = 100000,
                    ServiceFee = 100000,
                    InternetFee = 120000,
                    ElectricityUnitPrice = 3500,
                    WaterUnitPrice = 15000,
                    Description = "Phòng nhiều người phù hợp ký túc xá sinh viên.",
                    CreatedAt = now
                },
                new RoomCategory
                {
                    Code = "SHARED4",
                    Name = "Phòng 4 người",
                    BedLayout = "2 giường tầng, tối đa 4 người",
                    DefaultCapacity = 4,
                    BaseMonthlyFee = 1400000,
                    DepositAmount = 1400000,
                    HygieneFee = 90000,
                    ServiceFee = 110000,
                    InternetFee = 120000,
                    ElectricityUnitPrice = 3500,
                    WaterUnitPrice = 15000,
                    Description = "Phòng chia sẻ phổ biến cho bốn sinh viên.",
                    CreatedAt = now
                });
            await db.SaveChangesAsync();
        }
        else if (!await db.RoomCategories.AnyAsync(x => x.Code == "SHARED4"))
        {
            db.RoomCategories.Add(new RoomCategory
            {
                Code = "SHARED4",
                Name = "Phòng 4 người",
                BedLayout = "2 giường tầng, tối đa 4 người",
                DefaultCapacity = 4,
                BaseMonthlyFee = 1400000,
                DepositAmount = 1400000,
                HygieneFee = 90000,
                ServiceFee = 110000,
                InternetFee = 120000,
                ElectricityUnitPrice = 3500,
                WaterUnitPrice = 15000,
                Description = "Phòng chia sẻ phổ biến cho bốn sinh viên.",
                CreatedAt = now
            });
            await db.SaveChangesAsync();
        }

        if (!await db.PaymentMethodCatalogs.AnyAsync())
        {
            db.PaymentMethodCatalogs.AddRange(
                new PaymentMethodCatalog { Code = "Cash", Name = "Tiền mặt", Description = "Thu trực tiếp tại quầy tài chính.", CreatedAt = now },
                new PaymentMethodCatalog { Code = "Transfer", Name = "Chuyển khoản", BankName = "Ngân hàng", Description = "Sinh viên chuyển khoản theo mã hóa đơn.", CreatedAt = now },
                new PaymentMethodCatalog { Code = "POS", Name = "Quẹt thẻ/POS", ProcessingFee = 0, Description = "Thanh toán qua máy POS tại quầy.", CreatedAt = now });
            await db.SaveChangesAsync();
        }

        if (!await db.RoomZones.AnyAsync() && await db.Buildings.AnyAsync())
        {
            var buildings = await db.Buildings.OrderBy(x => x.Code).ToListAsync();
            var firstBuilding = buildings.FirstOrDefault();
            var secondBuilding = buildings.Skip(1).FirstOrDefault() ?? firstBuilding;

            db.RoomZones.AddRange(
                new RoomZone
                {
                    Code = "A-LOW",
                    Name = "Khu A tầng thấp",
                    BuildingId = firstBuilding?.Id,
                    GenderPolicy = firstBuilding?.GenderPolicy ?? "Mixed",
                    FloorFrom = 1,
                    FloorTo = 3,
                    ManagerName = firstBuilding?.ManagerName ?? string.Empty,
                    Description = "Nhóm phòng tầng thấp, ưu tiên sinh viên năm nhất hoặc cần di chuyển thuận tiện.",
                    CreatedAt = now
                },
                new RoomZone
                {
                    Code = "B-FEMALE",
                    Name = "Khu nữ B",
                    BuildingId = secondBuilding?.Id,
                    GenderPolicy = secondBuilding?.GenderPolicy ?? "Female",
                    FloorFrom = 1,
                    FloorTo = Math.Max(1, secondBuilding?.NumberOfFloors ?? 1),
                    ManagerName = secondBuilding?.ManagerName ?? string.Empty,
                    Description = "Phân khu nữ có kiểm soát theo tòa và tầng.",
                    CreatedAt = now
                });
            await db.SaveChangesAsync();
        }

        var categories = await db.RoomCategories.ToListAsync();
        var rooms = await db.Rooms.ToListAsync();
        foreach (var room in rooms)
        {
            var preferredCode = room.Capacity <= 1
                ? "SINGLE"
                : room.Capacity == 2
                    ? "DOUBLE"
                    : room.Capacity <= 4
                        ? "SHARED4"
                        : "BUNK";

            var category = categories.FirstOrDefault(x => x.Code == preferredCode)
                ?? categories.FirstOrDefault();

            if (category is null)
            {
                continue;
            }

            room.RoomCategoryId = category.Id;
            room.RoomType = category.Code;
            if (room.Capacity <= 0) room.Capacity = category.DefaultCapacity;
            if (room.PricePerMonth <= 0) room.PricePerMonth = category.BaseMonthlyFee;
        }

        if (rooms.Count > 0)
        {
            await db.SaveChangesAsync();
        }
    }
}
