using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using WorkshopWeb.Models;

namespace WorkshopWeb.Data
{
    public class WorkshopDbContext : IdentityDbContext<CustomUser>
    {
        public WorkshopDbContext(DbContextOptions<WorkshopDbContext> dbContextOptions) : base(dbContextOptions)
        {
            try
            {
                var databaseCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                if (databaseCreator != null)
                {
                    if (!databaseCreator.CanConnect()) databaseCreator.Create();
                    if (!databaseCreator.HasTables()) databaseCreator.CreateTables();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                throw;
            }
        }

        public DbSet<CustomUser> Users { get; set; }

        public DbSet<TimeSlot> TimeSlots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TimeSlot>()
                .HasOne(t => t.Employee)
                .WithMany() // Assuming the CustomUser class has no navigation property back to TimeSlots, or it's not included here
                .HasForeignKey(t => t.EmployeeId)
                .IsRequired(); // Make sure this is marked as required if it should be non-nullable
        }
    }
}
