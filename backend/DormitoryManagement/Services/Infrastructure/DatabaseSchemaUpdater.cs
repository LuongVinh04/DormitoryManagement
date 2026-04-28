using Dormitory.Models.DataContexts;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Services.Infrastructure;

public static class DatabaseSchemaUpdater
{
    public static async Task EnsureFinancialSchemaAsync(AppDbContext db)
    {
        if (!db.Database.IsSqlServer())
        {
            throw new InvalidOperationException("Project hien tai da thong nhat chi su dung SQL Server.");
        }

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

            IF COL_LENGTH('dbo.Users', 'StudentId') IS NULL
                ALTER TABLE [dbo].[Users] ADD [StudentId] INT NULL;

            IF OBJECT_ID(N'dbo.RoomFinanceStudentShares', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[RoomFinanceStudentShares]
                (
                    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [RoomFinanceRecordId] INT NOT NULL,
                    [StudentId] INT NOT NULL,
                    [InvoiceId] INT NULL,
                    [ExpectedAmount] DECIMAL(18,2) NOT NULL DEFAULT(0),
                    [PaidAmount] DECIMAL(18,2) NOT NULL DEFAULT(0),
                    [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Unpaid'),
                    [PaidDate] DATETIME2 NULL,
                    [PaymentMethod] NVARCHAR(64) NOT NULL DEFAULT(N''),
                    [Note] NVARCHAR(500) NOT NULL DEFAULT(N''),
                    [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                    [UpdatedAt] DATETIME2 NULL,
                    CONSTRAINT [FK_RoomFinanceStudentShares_RoomFinanceRecords] FOREIGN KEY ([RoomFinanceRecordId]) REFERENCES [dbo].[RoomFinanceRecords]([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_RoomFinanceStudentShares_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_RoomFinanceStudentShares_Invoices] FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoices]([Id]) ON DELETE NO ACTION
                );
            END

            IF COL_LENGTH('dbo.RoomFinanceStudentShares', 'InvoiceId') IS NULL
                ALTER TABLE [dbo].[RoomFinanceStudentShares] ADD [InvoiceId] INT NULL;

            IF OBJECT_ID(N'dbo.FK_RoomFinanceStudentShares_Invoices', N'F') IS NULL
                ALTER TABLE [dbo].[RoomFinanceStudentShares]
                ADD CONSTRAINT [FK_RoomFinanceStudentShares_Invoices]
                FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoices]([Id]) ON DELETE NO ACTION;

            IF OBJECT_ID(N'dbo.RoomTransferRequests', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[RoomTransferRequests]
                (
                    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [StudentId] INT NOT NULL,
                    [CurrentRoomId] INT NOT NULL,
                    [DesiredRoomId] INT NOT NULL,
                    [Reason] NVARCHAR(1000) NOT NULL DEFAULT(N''),
                    [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Pending'),
                    [DecisionDate] DATETIME2 NULL,
                    [DecisionNote] NVARCHAR(500) NOT NULL DEFAULT(N''),
                    [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                    [UpdatedAt] DATETIME2 NULL,
                    CONSTRAINT [FK_RoomTransferRequests_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_RoomTransferRequests_CurrentRoom] FOREIGN KEY ([CurrentRoomId]) REFERENCES [dbo].[Rooms]([Id]) ON DELETE NO ACTION,
                    CONSTRAINT [FK_RoomTransferRequests_DesiredRoom] FOREIGN KEY ([DesiredRoomId]) REFERENCES [dbo].[Rooms]([Id]) ON DELETE NO ACTION
                );
            END

            IF OBJECT_ID(N'dbo.ChatMessages', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ChatMessages]
                (
                    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [SenderId] INT NOT NULL,
                    [ReceiverId] INT NOT NULL,
                    [Content] NVARCHAR(2000) NOT NULL DEFAULT(N''),
                    [IsRead] BIT NOT NULL DEFAULT(0),
                    [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                    [UpdatedAt] DATETIME2 NULL,
                    CONSTRAINT [FK_ChatMessages_Sender] FOREIGN KEY ([SenderId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
                    CONSTRAINT [FK_ChatMessages_Receiver] FOREIGN KEY ([ReceiverId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION
                );
            END
            """);
    }
}
