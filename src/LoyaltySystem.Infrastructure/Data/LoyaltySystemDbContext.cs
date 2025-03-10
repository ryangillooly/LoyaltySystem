using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LoyaltySystem.Domain.Entities;

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
            modelBuilder.Entity<Store>().Property(s => s.Hours)
                .HasConversion(
                    h => System.Text.Json.JsonSerializer.Serialize(h, null),
                    h => System.Text.Json.JsonSerializer.Deserialize<OperatingHours>(h, null));
                    
            modelBuilder.Entity<Transaction>().Property(t => t._metadata)
                .HasColumnName("Metadata")
                .HasConversion(
                    m => System.Text.Json.JsonSerializer.Serialize(m, null),
                    m => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(m, null));
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
                    if (entry.Entity is Brand brand && brand.CreatedAt == default)
                        brand.CreatedAt = DateTime.UtcNow;
                        
                    if (entry.Entity is Store store && store.CreatedAt == default)
                        store.CreatedAt = DateTime.UtcNow;
                        
                    if (entry.Entity is LoyaltyProgram program && program.CreatedAt == default)
                        program.CreatedAt = DateTime.UtcNow;
                        
                    if (entry.Entity is Reward reward && reward.CreatedAt == default)
                        reward.CreatedAt = DateTime.UtcNow;
                        
                    if (entry.Entity is Customer customer && customer.CreatedAt == default)
                        customer.CreatedAt = DateTime.UtcNow;
                        
                    if (entry.Entity is LoyaltyCard card && card.CreatedAt == default)
                        card.CreatedAt = DateTime.UtcNow;
                        
                    if (entry.Entity is Transaction transaction && transaction.CreatedAt == default)
                        transaction.CreatedAt = DateTime.UtcNow;
                }
            }
            
            return base.SaveChangesAsync(cancellationToken);
        }
    }
} 