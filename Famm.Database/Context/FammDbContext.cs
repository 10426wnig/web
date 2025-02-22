using System.Data.Entity;
using System.Data.Entity.SqlServer;
using Famm.Database.Configurations;
using Famm.Domain.Models;

namespace Famm.Database.Context
{
    public class FammDbContext : DbContext
    {
        static FammDbContext()
        {
            // Ensure SQL Server provider is loaded
            var instance = SqlProviderServices.Instance;
        }
        
        public FammDbContext() 
            : base("FammStoreDb")
        {
            // Disable automatic migrations
            System.Data.Entity.Database.SetInitializer(new CreateDatabaseIfNotExists<FammDbContext>());
        }

        // Define DbSets for each entity
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.Configurations.Add(new UserConfiguration());
            
            // Other configurations can be applied here in the same way
            
            // Configure relations without specific configurations
            // Configure Category entity relationships
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithRequired(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .WillCascadeOnDelete(false);
                
            modelBuilder.Entity<Category>()
                .HasOptional(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .WillCascadeOnDelete(false);

            // Configure Order entity relationships
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithRequired(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .WillCascadeOnDelete(true);
                
            modelBuilder.Entity<Order>()
                .HasOptional(o => o.BillingAddress)
                .WithMany()
                .HasForeignKey(o => o.BillingAddressId)
                .WillCascadeOnDelete(false);
                
            modelBuilder.Entity<Order>()
                .HasOptional(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.ShippingAddressId)
                .WillCascadeOnDelete(false);

            // Configure OrderItem entity relationships
            modelBuilder.Entity<OrderItem>()
                .HasRequired(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .WillCascadeOnDelete(false);

            // Configure Cart entity relationships
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithRequired(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .WillCascadeOnDelete(true);

            // Configure CartItem entity relationships
            modelBuilder.Entity<CartItem>()
                .HasRequired(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .WillCascadeOnDelete(false);
        }
    }
}
