using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LoyaltySystem.Domain.Entities;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LoyaltySystem.Infrastructure.Data
{
    /// <summary>
    /// Database context for the loyalty system using Entity Framework Core.
    /// </summary>
    public class LoyaltySystemDbContext : DbContext
    {
        /// <summary>
        /// Brands in the system.
        /// </summary>
        public DbSet<Brand> Brands { get; set; }
        
        /// <summary>
        /// Stores in the system.
        /// </summary>
        public DbSet<Store> Stores { get; set; }
        
        /// <summary>
        /// Loyalty programs in the system.
        /// </summary>
        public DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; }
        
        /// <summary>
        /// Rewards in the system.
        /// </summary>
        public DbSet<Reward> Rewards { get; set; }
        
        /// <summary>
        /// Customers in the system.
        /// </summary>
        public DbSet<Customer> Customers { get; set; }
        
        /// <summary>
        /// Loyalty cards in the system.
        /// </summary>
        public DbSet<LoyaltyCard> LoyaltyCards { get; set; }
        
        /// <summary>
        /// Transactions in the system.
        /// </summary>
        public DbSet<Transaction> Transactions { get; set; }
        
        /// <summary>
        /// Creates a new instance of the database context.
        /// </summary>
        public LoyaltySystemDbContext(DbContextOptions<LoyaltySystemDbContext> options)
            : base(options)
        {
        }
        
        /// <summary>
        /// Configures the model for the loyalty system.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoyaltySystemDbContext).Assembly);
            
            // Configure value objects as owned entities
            modelBuilder.Entity<Brand>().OwnsOne(b => b.Contact);
            modelBuilder.Entity<Brand>().OwnsOne(b => b.Address);
            modelBuilder.Entity<Store>().OwnsOne(s => s.Address);
            modelBuilder.Entity<Store>().OwnsOne(s => s.Location);
            modelBuilder.Entity<LoyaltyProgram>().OwnsOne(p => p.ExpirationPolicy);
            
            // Configure complex types that don't directly map to a DB column
            modelBuilder.Entity<Store>().Property(s => s.OperatingHours)
                .HasConversion(
                    new ValueConverter<OperatingHours, string>(
                        h => System.Text.Json.JsonSerializer.Serialize(h, new System.Text.Json.JsonSerializerOptions()),
                        h => System.Text.Json.JsonSerializer.Deserialize<OperatingHours>(h, new System.Text.Json.JsonSerializerOptions())
                    ));
                    
            modelBuilder.Entity<Transaction>().Property("Metadata")
                .HasConversion(
                    new ValueConverter<Dictionary<string, string>, string>(
                        m => System.Text.Json.JsonSerializer.Serialize(m, new System.Text.Json.JsonSerializerOptions()),
                        m => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(m, new System.Text.Json.JsonSerializerOptions())
                    ));
        }
        
        /// <summary>
        /// Overrides SaveChanges to add audit information.
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Add audit information (CreatedAt/UpdatedAt)
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    // Use reflection to set CreatedAt if it's not already set
                    // This avoids direct assignment to read-only properties
                    var now = DateTime.UtcNow;
                    
                    if (entry.Entity is Brand || 
                        entry.Entity is Store || 
                        entry.Entity is LoyaltyProgram || 
                        entry.Entity is Reward || 
                        entry.Entity is Customer || 
                        entry.Entity is LoyaltyCard || 
                        entry.Entity is Transaction)
                    {
                        var createdAtProp = entry.Entity.GetType().GetProperty("CreatedAt");
                        var createdAtValue = (DateTime)createdAtProp.GetValue(entry.Entity);
                        
                        if (createdAtValue == default)
                        {
                            var field = entry.Entity.GetType().GetField("_createdAt", 
                                System.Reflection.BindingFlags.NonPublic | 
                                System.Reflection.BindingFlags.Instance);
                                
                            if (field != null)
                            {
                                field.SetValue(entry.Entity, now);
                            }
                        }
                    }
                }
            }
            
            return base.SaveChangesAsync(cancellationToken);
        }
    }
} 