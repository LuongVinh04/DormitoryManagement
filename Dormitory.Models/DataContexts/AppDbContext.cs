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

    }
}
