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

        await EnsureStatusColumnsAsync(db);

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

            IF COL_LENGTH('dbo.Rooms', 'Status') IS NULL
                ALTER TABLE [dbo].[Rooms] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Available');

            IF COL_LENGTH('dbo.Buildings', 'RoomCount') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Buildings] ADD [RoomCount] INT NOT NULL DEFAULT(0);

                EXEC(N'
                    UPDATE b
                    SET [RoomCount] = roomTotals.[RoomCount]
                    FROM [dbo].[Buildings] b
                    INNER JOIN (
                        SELECT [BuildingId], COUNT(*) AS [RoomCount]
                        FROM [dbo].[Rooms]
                        GROUP BY [BuildingId]
                    ) roomTotals ON roomTotals.[BuildingId] = b.[Id];
                ');
            END

            IF COL_LENGTH('dbo.Students', 'Status') IS NULL
                ALTER TABLE [dbo].[Students] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Active');

            IF COL_LENGTH('dbo.Registrations', 'Status') IS NULL
                ALTER TABLE [dbo].[Registrations] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Pending');

            IF COL_LENGTH('dbo.Contracts', 'Status') IS NULL
                ALTER TABLE [dbo].[Contracts] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Active');

            IF COL_LENGTH('dbo.Invoices', 'Status') IS NULL
                ALTER TABLE [dbo].[Invoices] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Unpaid');

            IF COL_LENGTH('dbo.RoomFinanceRecords', 'Status') IS NULL
                ALTER TABLE [dbo].[RoomFinanceRecords] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Unpaid');

            IF OBJECT_ID(N'dbo.RoomFinanceStudentShares', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.RoomFinanceStudentShares', 'Status') IS NULL
                ALTER TABLE [dbo].[RoomFinanceStudentShares] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Unpaid');

            IF OBJECT_ID(N'dbo.RoomTransferRequests', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.RoomTransferRequests', 'Status') IS NULL
                ALTER TABLE [dbo].[RoomTransferRequests] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Pending');

            IF COL_LENGTH('dbo.Users', 'StudentId') IS NULL
                ALTER TABLE [dbo].[Users] ADD [StudentId] INT NULL;

            IF COL_LENGTH('dbo.Contracts', 'RoomId') IS NOT NULL
                AND COLUMNPROPERTY(OBJECT_ID(N'dbo.Contracts'), 'RoomId', 'AllowsNull') = 0
            BEGIN
                DECLARE @DropContractRoomFkSql NVARCHAR(MAX) = N'';

                SELECT @DropContractRoomFkSql = @DropContractRoomFkSql
                    + N'ALTER TABLE [dbo].[Contracts] DROP CONSTRAINT [' + fk.[name] + N'];'
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
                INNER JOIN sys.columns col ON col.object_id = fkc.parent_object_id AND col.column_id = fkc.parent_column_id
                WHERE fk.parent_object_id = OBJECT_ID(N'dbo.Contracts')
                    AND col.[name] = N'RoomId';

                IF @DropContractRoomFkSql <> N''
                    EXEC sp_executesql @DropContractRoomFkSql;

                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = N'IX_Contracts_RoomId'
                        AND object_id = OBJECT_ID(N'dbo.Contracts')
                )
                    DROP INDEX [IX_Contracts_RoomId] ON [dbo].[Contracts];

                ALTER TABLE [dbo].[Contracts] ALTER COLUMN [RoomId] INT NULL;

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = N'IX_Contracts_RoomId'
                        AND object_id = OBJECT_ID(N'dbo.Contracts')
                )
                    CREATE INDEX [IX_Contracts_RoomId] ON [dbo].[Contracts]([RoomId]);

                IF OBJECT_ID(N'dbo.FK_Contracts_Rooms_RoomId', N'F') IS NULL
                    ALTER TABLE [dbo].[Contracts]
                    ADD CONSTRAINT [FK_Contracts_Rooms_RoomId]
                    FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms]([Id]) ON DELETE NO ACTION;
            END

            UPDATE c
            SET [RoomId] = s.[RoomId],
                [UpdatedAt] = GETUTCDATE()
            FROM [dbo].[Contracts] c
            INNER JOIN [dbo].[Students] s ON s.[Id] = c.[StudentId]
            WHERE c.[Status] = N'Active'
                AND c.[StartDate] < DATEADD(DAY, 1, CONVERT(date, GETDATE()))
                AND c.[EndDate] >= CONVERT(date, GETDATE())
                AND s.[RoomId] IS NOT NULL
                AND (c.[RoomId] IS NULL OR c.[RoomId] <> s.[RoomId]);

            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE [name] = N'IX_Contracts_StudentId'
                    AND object_id = OBJECT_ID(N'dbo.Contracts')
                    AND is_unique = 1
            )
                DROP INDEX [IX_Contracts_StudentId] ON [dbo].[Contracts];

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE [name] = N'IX_Contracts_StudentId'
                    AND object_id = OBJECT_ID(N'dbo.Contracts')
            )
                CREATE INDEX [IX_Contracts_StudentId] ON [dbo].[Contracts]([StudentId]);

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

            IF OBJECT_ID(N'dbo.ViolationRecords', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ViolationRecords]
                (
                    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [StudentId] INT NOT NULL,
                    [RoomId] INT NULL,
                    [ViolationType] NVARCHAR(64) NOT NULL DEFAULT(N''),
                    [Description] NVARCHAR(1000) NOT NULL DEFAULT(N''),
                    [FineAmount] DECIMAL(18,2) NOT NULL DEFAULT(0),
                    [TrainingPointDeduction] INT NOT NULL DEFAULT(0),
                    [ViolationDate] DATETIME2 NOT NULL,
                    [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Pending'),
                    [HandledBy] NVARCHAR(160) NOT NULL DEFAULT(N''),
                    [Note] NVARCHAR(500) NOT NULL DEFAULT(N''),
                    [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                    [UpdatedAt] DATETIME2 NULL,
                    CONSTRAINT [FK_ViolationRecords_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_ViolationRecords_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms]([Id]) ON DELETE SET NULL
                );

                CREATE INDEX [IX_ViolationRecords_StudentId] ON [dbo].[ViolationRecords]([StudentId]);
                CREATE INDEX [IX_ViolationRecords_RoomId] ON [dbo].[ViolationRecords]([RoomId]);
                CREATE INDEX [IX_ViolationRecords_Status] ON [dbo].[ViolationRecords]([Status]);
            END

            IF OBJECT_ID(N'dbo.MaintenanceRequests', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[MaintenanceRequests]
                (
                    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [StudentId] INT NOT NULL,
                    [RoomId] INT NOT NULL,
                    [EquipmentType] NVARCHAR(64) NOT NULL DEFAULT(N''),
                    [OtherEquipmentName] NVARCHAR(160) NOT NULL DEFAULT(N''),
                    [Description] NVARCHAR(1000) NOT NULL DEFAULT(N''),
                    [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Submitted'),
                    [AdminNote] NVARCHAR(500) NOT NULL DEFAULT(N''),
                    [TechnicianName] NVARCHAR(160) NOT NULL DEFAULT(N''),
                    [ApprovedAt] DATETIME2 NULL,
                    [StartedAt] DATETIME2 NULL,
                    [CompletedAt] DATETIME2 NULL,
                    [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                    [UpdatedAt] DATETIME2 NULL,
                    CONSTRAINT [FK_MaintenanceRequests_Students] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students]([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_MaintenanceRequests_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms]([Id]) ON DELETE NO ACTION
                );

                CREATE INDEX [IX_MaintenanceRequests_StudentId] ON [dbo].[MaintenanceRequests]([StudentId]);
                CREATE INDEX [IX_MaintenanceRequests_RoomId] ON [dbo].[MaintenanceRequests]([RoomId]);
                CREATE INDEX [IX_MaintenanceRequests_Status] ON [dbo].[MaintenanceRequests]([Status]);
            END

            IF OBJECT_ID(N'dbo.MaintenanceRequests', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.MaintenanceRequests', 'OtherEquipmentName') IS NULL
                ALTER TABLE [dbo].[MaintenanceRequests] ADD [OtherEquipmentName] NVARCHAR(160) NOT NULL DEFAULT(N'');
            """);
    }

    private static async Task EnsureStatusColumnsAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'dbo.Rooms', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.Rooms', 'Status') IS NULL
                ALTER TABLE [dbo].[Rooms] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Available');

            IF OBJECT_ID(N'dbo.Students', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.Students', 'Status') IS NULL
                ALTER TABLE [dbo].[Students] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Active');

            IF OBJECT_ID(N'dbo.Registrations', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.Registrations', 'Status') IS NULL
                ALTER TABLE [dbo].[Registrations] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Pending');

            IF OBJECT_ID(N'dbo.Contracts', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.Contracts', 'Status') IS NULL
                ALTER TABLE [dbo].[Contracts] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Active');

            IF OBJECT_ID(N'dbo.Invoices', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.Invoices', 'Status') IS NULL
                ALTER TABLE [dbo].[Invoices] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Unpaid');

            IF OBJECT_ID(N'dbo.RoomFeeProfiles', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.RoomFeeProfiles', 'Status') IS NULL
                ALTER TABLE [dbo].[RoomFeeProfiles] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Active');

            IF OBJECT_ID(N'dbo.RoomFeeProfiles', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.RoomFeeProfiles', 'EffectiveFrom') IS NULL
                ALTER TABLE [dbo].[RoomFeeProfiles] ADD [EffectiveFrom] DATETIME2 NULL;

            IF OBJECT_ID(N'dbo.RoomFeeProfiles', N'U') IS NOT NULL
                UPDATE [dbo].[RoomFeeProfiles]
                SET [EffectiveFrom] = CONVERT(date, GETDATE())
                WHERE [EffectiveFrom] IS NULL;

            IF OBJECT_ID(N'dbo.RoomFeeProfiles', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.RoomFeeProfiles', 'EffectiveTo') IS NULL
                ALTER TABLE [dbo].[RoomFeeProfiles] ADD [EffectiveTo] DATETIME2 NULL;

            IF OBJECT_ID(N'dbo.RoomFinanceRecords', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.RoomFinanceRecords', 'Status') IS NULL
                ALTER TABLE [dbo].[RoomFinanceRecords] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Unpaid');

            IF OBJECT_ID(N'dbo.RoomFinanceStudentShares', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.RoomFinanceStudentShares', 'Status') IS NULL
                ALTER TABLE [dbo].[RoomFinanceStudentShares] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Unpaid');

            IF OBJECT_ID(N'dbo.RoomTransferRequests', N'U') IS NOT NULL
                AND COL_LENGTH('dbo.RoomTransferRequests', 'Status') IS NULL
                ALTER TABLE [dbo].[RoomTransferRequests] ADD [Status] NVARCHAR(32) NOT NULL DEFAULT(N'Pending');

            IF OBJECT_ID(N'dbo.Invoices', N'U') IS NOT NULL
                UPDATE [dbo].[Invoices]
                SET [PaidDate] = NULL,
                    [UpdatedAt] = GETUTCDATE()
                WHERE [Status] <> N'Paid'
                    AND [PaidDate] IS NOT NULL;

            IF OBJECT_ID(N'dbo.RoomFinanceRecords', N'U') IS NOT NULL
                UPDATE [dbo].[RoomFinanceRecords]
                SET [PaidDate] = NULL,
                    [UpdatedAt] = GETUTCDATE()
                WHERE [Status] <> N'Paid'
                    AND [PaidDate] IS NOT NULL;
            """);
    }
}
