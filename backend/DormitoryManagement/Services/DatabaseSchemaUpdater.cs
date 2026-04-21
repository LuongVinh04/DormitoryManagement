using Dormitory.Models.DataContexts;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Services;

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
                """);
        }
    }
}
