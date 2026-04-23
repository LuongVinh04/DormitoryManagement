using Dormitory.Models.DataContexts;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Services.Infrastructure;

public static class DatabaseSchemaUpdater
{
    public static async Task EnsureFinancialSchemaAsync(AppDbContext db)
    {
        if (db.Database.IsSqlServer())
        {
            await db.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'dbo.RoomFeeProfiles', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[RoomFeeProfiles]
                    (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [RoomId] INT NOT NULL,
                        [MonthlyRoomFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [ElectricityUnitPrice] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [WaterUnitPrice] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [HygieneFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [ServiceFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [InternetFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [OtherFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [OtherFeeName] NVARCHAR(128) NOT NULL DEFAULT(N''),
                        [BillingCycleDay] INT NOT NULL DEFAULT(10),
                        [Notes] NVARCHAR(500) NOT NULL DEFAULT(N''),
                        [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                        [UpdatedAt] DATETIME2 NULL,
                        CONSTRAINT [FK_RoomFeeProfiles_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms]([Id]) ON DELETE CASCADE
                    );
                    CREATE UNIQUE INDEX [IX_RoomFeeProfiles_RoomId] ON [dbo].[RoomFeeProfiles]([RoomId]);
                END

                IF OBJECT_ID(N'dbo.RoomFinanceRecords', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[RoomFinanceRecords]
                    (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [RoomId] INT NOT NULL,
                        [UtilityId] INT NULL,
                        [BillingMonth] DATETIME2 NOT NULL,
                        [MonthlyRoomFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [ElectricityFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [WaterFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [HygieneFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [ServiceFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [InternetFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [OtherFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [Total] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [PaidAmount] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Unpaid'),
                        [DueDate] DATETIME2 NOT NULL,
                        [PaidDate] DATETIME2 NULL,
                        [PaymentMethod] NVARCHAR(64) NOT NULL DEFAULT(N''),
                        [PaymentNote] NVARCHAR(500) NOT NULL DEFAULT(N''),
                        [RecordedBy] NVARCHAR(128) NOT NULL DEFAULT(N''),
                        [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                        [UpdatedAt] DATETIME2 NULL,
                        CONSTRAINT [FK_RoomFinanceRecords_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms]([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_RoomFinanceRecords_Utilities_UtilityId] FOREIGN KEY ([UtilityId]) REFERENCES [dbo].[Utilities]([Id]) ON DELETE NO ACTION
                    );
                    CREATE UNIQUE INDEX [IX_RoomFinanceRecords_RoomId_BillingMonth] ON [dbo].[RoomFinanceRecords]([RoomId], [BillingMonth]);
                END

                IF OBJECT_ID(N'dbo.RoomCategories', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[RoomCategories]
                    (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Code] NVARCHAR(64) NOT NULL,
                        [Name] NVARCHAR(160) NOT NULL,
                        [BedLayout] NVARCHAR(160) NOT NULL DEFAULT(N''),
                        [DefaultCapacity] INT NOT NULL DEFAULT(0),
                        [BaseMonthlyFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [DepositAmount] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [HygieneFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [ServiceFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [InternetFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [ElectricityUnitPrice] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [WaterUnitPrice] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [Description] NVARCHAR(500) NOT NULL DEFAULT(N''),
                        [IsActive] BIT NOT NULL DEFAULT(1),
                        [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                        [UpdatedAt] DATETIME2 NULL
                    );
                    CREATE UNIQUE INDEX [IX_RoomCategories_Code] ON [dbo].[RoomCategories]([Code]);
                END

                IF OBJECT_ID(N'dbo.RoomZones', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[RoomZones]
                    (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Code] NVARCHAR(64) NOT NULL,
                        [Name] NVARCHAR(160) NOT NULL,
                        [BuildingId] INT NULL,
                        [GenderPolicy] NVARCHAR(32) NOT NULL DEFAULT(N'Mixed'),
                        [FloorFrom] INT NOT NULL DEFAULT(0),
                        [FloorTo] INT NOT NULL DEFAULT(0),
                        [ManagerName] NVARCHAR(160) NOT NULL DEFAULT(N''),
                        [Description] NVARCHAR(500) NOT NULL DEFAULT(N''),
                        [IsActive] BIT NOT NULL DEFAULT(1),
                        [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                        [UpdatedAt] DATETIME2 NULL,
                        CONSTRAINT [FK_RoomZones_Buildings_BuildingId] FOREIGN KEY ([BuildingId]) REFERENCES [dbo].[Buildings]([Id]) ON DELETE SET NULL
                    );
                    CREATE UNIQUE INDEX [IX_RoomZones_Code] ON [dbo].[RoomZones]([Code]);
                END

                IF OBJECT_ID(N'dbo.PaymentMethodCatalogs', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[PaymentMethodCatalogs]
                    (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Code] NVARCHAR(64) NOT NULL,
                        [Name] NVARCHAR(160) NOT NULL,
                        [AccountName] NVARCHAR(160) NOT NULL DEFAULT(N''),
                        [AccountNumber] NVARCHAR(80) NOT NULL DEFAULT(N''),
                        [BankName] NVARCHAR(160) NOT NULL DEFAULT(N''),
                        [ProcessingFee] DECIMAL(18,2) NOT NULL DEFAULT(0),
                        [Description] NVARCHAR(500) NOT NULL DEFAULT(N''),
                        [IsActive] BIT NOT NULL DEFAULT(1),
                        [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                        [UpdatedAt] DATETIME2 NULL
                    );
                    CREATE UNIQUE INDEX [IX_PaymentMethodCatalogs_Code] ON [dbo].[PaymentMethodCatalogs]([Code]);
                END

                IF COL_LENGTH('dbo.Rooms', 'RoomCategoryId') IS NULL
                    ALTER TABLE [dbo].[Rooms] ADD [RoomCategoryId] INT NULL;

                IF COL_LENGTH('dbo.Rooms', 'RoomZoneId') IS NULL
                    ALTER TABLE [dbo].[Rooms] ADD [RoomZoneId] INT NULL;
                """);
            return;
        }

        if (db.Database.IsSqlite())
        {
            await db.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "RoomFeeProfiles"
                (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_RoomFeeProfiles" PRIMARY KEY AUTOINCREMENT,
                    "RoomId" INTEGER NOT NULL,
                    "MonthlyRoomFee" TEXT NOT NULL DEFAULT 0,
                    "ElectricityUnitPrice" TEXT NOT NULL DEFAULT 0,
                    "WaterUnitPrice" TEXT NOT NULL DEFAULT 0,
                    "HygieneFee" TEXT NOT NULL DEFAULT 0,
                    "ServiceFee" TEXT NOT NULL DEFAULT 0,
                    "InternetFee" TEXT NOT NULL DEFAULT 0,
                    "OtherFee" TEXT NOT NULL DEFAULT 0,
                    "OtherFeeName" TEXT NOT NULL DEFAULT '',
                    "BillingCycleDay" INTEGER NOT NULL DEFAULT 10,
                    "Notes" TEXT NOT NULL DEFAULT '',
                    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    "UpdatedAt" TEXT NULL,
                    CONSTRAINT "FK_RoomFeeProfiles_Rooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES "Rooms" ("Id") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_RoomFeeProfiles_RoomId" ON "RoomFeeProfiles" ("RoomId");

                CREATE TABLE IF NOT EXISTS "RoomFinanceRecords"
                (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_RoomFinanceRecords" PRIMARY KEY AUTOINCREMENT,
                    "RoomId" INTEGER NOT NULL,
                    "UtilityId" INTEGER NULL,
                    "BillingMonth" TEXT NOT NULL,
                    "MonthlyRoomFee" TEXT NOT NULL DEFAULT 0,
                    "ElectricityFee" TEXT NOT NULL DEFAULT 0,
                    "WaterFee" TEXT NOT NULL DEFAULT 0,
                    "HygieneFee" TEXT NOT NULL DEFAULT 0,
                    "ServiceFee" TEXT NOT NULL DEFAULT 0,
                    "InternetFee" TEXT NOT NULL DEFAULT 0,
                    "OtherFee" TEXT NOT NULL DEFAULT 0,
                    "Total" TEXT NOT NULL DEFAULT 0,
                    "PaidAmount" TEXT NOT NULL DEFAULT 0,
                    "Status" TEXT NOT NULL DEFAULT 'Unpaid',
                    "DueDate" TEXT NOT NULL,
                    "PaidDate" TEXT NULL,
                    "PaymentMethod" TEXT NOT NULL DEFAULT '',
                    "PaymentNote" TEXT NOT NULL DEFAULT '',
                    "RecordedBy" TEXT NOT NULL DEFAULT '',
                    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    "UpdatedAt" TEXT NULL,
                    CONSTRAINT "FK_RoomFinanceRecords_Rooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES "Rooms" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_RoomFinanceRecords_Utilities_UtilityId" FOREIGN KEY ("UtilityId") REFERENCES "Utilities" ("Id") ON DELETE NO ACTION
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_RoomFinanceRecords_RoomId_BillingMonth" ON "RoomFinanceRecords" ("RoomId", "BillingMonth");

                CREATE TABLE IF NOT EXISTS "RoomCategories"
                (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_RoomCategories" PRIMARY KEY AUTOINCREMENT,
                    "Code" TEXT NOT NULL,
                    "Name" TEXT NOT NULL,
                    "BedLayout" TEXT NOT NULL DEFAULT '',
                    "DefaultCapacity" INTEGER NOT NULL DEFAULT 0,
                    "BaseMonthlyFee" TEXT NOT NULL DEFAULT 0,
                    "DepositAmount" TEXT NOT NULL DEFAULT 0,
                    "HygieneFee" TEXT NOT NULL DEFAULT 0,
                    "ServiceFee" TEXT NOT NULL DEFAULT 0,
                    "InternetFee" TEXT NOT NULL DEFAULT 0,
                    "ElectricityUnitPrice" TEXT NOT NULL DEFAULT 0,
                    "WaterUnitPrice" TEXT NOT NULL DEFAULT 0,
                    "Description" TEXT NOT NULL DEFAULT '',
                    "IsActive" INTEGER NOT NULL DEFAULT 1,
                    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    "UpdatedAt" TEXT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_RoomCategories_Code" ON "RoomCategories" ("Code");

                CREATE TABLE IF NOT EXISTS "RoomZones"
                (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_RoomZones" PRIMARY KEY AUTOINCREMENT,
                    "Code" TEXT NOT NULL,
                    "Name" TEXT NOT NULL,
                    "BuildingId" INTEGER NULL,
                    "GenderPolicy" TEXT NOT NULL DEFAULT 'Mixed',
                    "FloorFrom" INTEGER NOT NULL DEFAULT 0,
                    "FloorTo" INTEGER NOT NULL DEFAULT 0,
                    "ManagerName" TEXT NOT NULL DEFAULT '',
                    "Description" TEXT NOT NULL DEFAULT '',
                    "IsActive" INTEGER NOT NULL DEFAULT 1,
                    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    "UpdatedAt" TEXT NULL,
                    CONSTRAINT "FK_RoomZones_Buildings_BuildingId" FOREIGN KEY ("BuildingId") REFERENCES "Buildings" ("Id") ON DELETE SET NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_RoomZones_Code" ON "RoomZones" ("Code");

                CREATE TABLE IF NOT EXISTS "PaymentMethodCatalogs"
                (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_PaymentMethodCatalogs" PRIMARY KEY AUTOINCREMENT,
                    "Code" TEXT NOT NULL,
                    "Name" TEXT NOT NULL,
                    "AccountName" TEXT NOT NULL DEFAULT '',
                    "AccountNumber" TEXT NOT NULL DEFAULT '',
                    "BankName" TEXT NOT NULL DEFAULT '',
                    "ProcessingFee" TEXT NOT NULL DEFAULT 0,
                    "Description" TEXT NOT NULL DEFAULT '',
                    "IsActive" INTEGER NOT NULL DEFAULT 1,
                    "CreatedAt" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    "UpdatedAt" TEXT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_PaymentMethodCatalogs_Code" ON "PaymentMethodCatalogs" ("Code");
                """);

            await TryExecuteAsync(db, "ALTER TABLE \"Rooms\" ADD COLUMN \"RoomCategoryId\" INTEGER NULL;");
            await TryExecuteAsync(db, "ALTER TABLE \"Rooms\" ADD COLUMN \"RoomZoneId\" INTEGER NULL;");
        }
    }

    private static async Task TryExecuteAsync(AppDbContext db, string sql)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync(sql);
        }
        catch
        {
            // SQLite does not support ADD COLUMN IF NOT EXISTS on all target runtimes.
        }
    }
}
