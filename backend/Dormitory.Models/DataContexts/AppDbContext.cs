using Dormitory.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dormitory.Models.DataContexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<RolePermissions> RolePermissions { get; set; }
        public DbSet<UserPermissions> UserPermissions { get; set; }
        public DbSet<Buildings> Buildings { get; set; }
        public DbSet<RoomCategory> RoomCategories { get; set; }
        public DbSet<RoomZone> RoomZones { get; set; }
        public DbSet<PaymentMethodCatalog> PaymentMethodCatalogs { get; set; }
        public DbSet<Rooms> Rooms { get; set; }
        public DbSet<Students> Students { get; set; }
        public DbSet<Registrations> Registrations { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Utilities> Utilities { get; set; }
        public DbSet<Invoices> Invoices { get; set; }
        public DbSet<RoomFeeProfile> RoomFeeProfiles { get; set; }
        public DbSet<RoomFinanceRecord> RoomFinanceRecords { get; set; }
        public DbSet<RoomFinanceStudentShare> RoomFinanceStudentShares { get; set; }
        public DbSet<RoomTransferRequest> RoomTransferRequests { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Buildings>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<RoomCategory>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<RoomZone>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<PaymentMethodCatalog>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<Rooms>()
                .HasIndex(x => new { x.BuildingId, x.RoomNumber })
                .IsUnique();

            modelBuilder.Entity<Students>()
                .HasIndex(x => x.StudentCode)
                .IsUnique();

            modelBuilder.Entity<Users>()
                .HasIndex(x => x.Username)
                .IsUnique();

            modelBuilder.Entity<Permissions>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<RolePermissions>()
                .HasIndex(x => new { x.RoleId, x.PermissionId })
                .IsUnique();

            modelBuilder.Entity<UserPermissions>()
                .HasIndex(x => new { x.UserId, x.PermissionId })
                .IsUnique();

            modelBuilder.Entity<Invoices>()
                .HasIndex(x => x.InvoiceCode)
                .IsUnique();

            modelBuilder.Entity<Contract>()
                .HasIndex(x => x.ContractCode)
                .IsUnique();

            modelBuilder.Entity<RoomFeeProfile>()
                .HasIndex(x => x.RoomId)
                .IsUnique();

            modelBuilder.Entity<RoomFinanceRecord>()
                .HasIndex(x => new { x.RoomId, x.BillingMonth })
                .IsUnique();

            modelBuilder.Entity<Rooms>()
                .HasOne(x => x.Building)
                .WithMany(x => x.Rooms)
                .HasForeignKey(x => x.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rooms>()
                .HasOne(x => x.RoomCategory)
                .WithMany(x => x.Rooms)
                .HasForeignKey(x => x.RoomCategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Rooms>()
                .HasOne(x => x.RoomZone)
                .WithMany(x => x.Rooms)
                .HasForeignKey(x => x.RoomZoneId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RoomZone>()
                .HasOne(x => x.Building)
                .WithMany()
                .HasForeignKey(x => x.BuildingId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Students>()
                .HasOne(x => x.Room)
                .WithMany(x => x.Students)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Registrations>()
                .HasOne(x => x.Student)
                .WithMany(x => x.Registrations)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Registrations>()
                .HasOne(x => x.Room)
                .WithMany(x => x.Registrations)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(x => x.Student)
                .WithMany(x => x.Contracts)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Contract>()
                .HasOne(x => x.Room)
                .WithMany(x => x.Contracts)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Utilities>()
                .HasOne(x => x.Room)
                .WithMany(x => x.Utilities)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Invoices>()
                .HasOne(x => x.Student)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Invoices>()
                .HasOne(x => x.Room)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoices>()
                .HasOne(x => x.Utility)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.UtilityId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RoomFeeProfile>()
                .HasOne(x => x.Room)
                .WithMany(x => x.FeeProfiles)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoomFinanceRecord>()
                .HasOne(x => x.Room)
                .WithMany(x => x.FinanceRecords)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoomFinanceRecord>()
                .HasOne(x => x.Utility)
                .WithMany()
                .HasForeignKey(x => x.UtilityId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Users>()
                .HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Users>()
                .HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RolePermissions>()
                .HasOne(x => x.Role)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermissions>()
                .HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPermissions>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserPermissions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPermissions>()
                .HasOne(x => x.Permission)
                .WithMany(x => x.UserPermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoomFinanceStudentShare>()
                .HasOne(x => x.RoomFinanceRecord)
                .WithMany(x => x.StudentShares)
                .HasForeignKey(x => x.RoomFinanceRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoomFinanceStudentShare>()
                .HasOne(x => x.Student)
                .WithMany(x => x.FinanceShares)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoomFinanceStudentShare>()
                .HasOne(x => x.Invoice)
                .WithMany()
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RoomTransferRequest>()
                .HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoomTransferRequest>()
                .HasOne(x => x.CurrentRoom)
                .WithMany()
                .HasForeignKey(x => x.CurrentRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoomTransferRequest>()
                .HasOne(x => x.DesiredRoom)
                .WithMany()
                .HasForeignKey(x => x.DesiredRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(x => x.Receiver)
                .WithMany()
                .HasForeignKey(x => x.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
