using Dormitory.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dormitory.Models.DataContexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Buildings> Buildings { get; set; }
        public DbSet<Rooms> Rooms { get; set; }
        public DbSet<Students> Students { get; set; }
        public DbSet<Registrations> Registrations { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Utilities> Utilities { get; set; }
        public DbSet<Invoices> Invoices { get; set; }
        public DbSet<RoomFeeProfile> RoomFeeProfiles { get; set; }
        public DbSet<RoomFinanceRecord> RoomFinanceRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Buildings>()
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
        }
    }
}
