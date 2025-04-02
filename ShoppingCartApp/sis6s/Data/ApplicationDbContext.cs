using sis6s.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace sis6s.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Product - Category relationship
            builder.Entity<Product>()
                .HasOne(s => s.Category)
                .WithMany(t => t.Products)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Cart - User relationship
            builder.Entity<Cart>()
                .HasOne(g => g.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Cart - Order relationship
            builder.Entity<Cart>()
                .HasOne<Order>()
                .WithMany()
                .HasForeignKey(g => g.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure CartItem relationships
            builder.Entity<CartItem>()
                .HasOne(ct => ct.Cart)
                .WithMany(g => g.CartItems)
                .HasForeignKey(ct => ct.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(ct => ct.Product)
                .WithMany()
                .HasForeignKey(ct => ct.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Order - User relationship
            builder.Entity<Order>()
                .HasOne(d => d.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Order - OrderItem relationship
            builder.Entity<OrderItem>()
                .HasOne(c => c.Order)
                .WithMany(d => d.OrderItems)
                .HasForeignKey(c => c.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure OrderItem - Product relationship
            builder.Entity<OrderItem>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Payment - Order relationship
            builder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(d => d.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure default values and constraints
            builder.Entity<Product>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Entity<Product>()
                .Property(s => s.Stock)
                .HasDefaultValue(0);

            builder.Entity<CartItem>()
                .Property(ct => ct.Quantity)
                .HasDefaultValue(1);

            builder.Entity<OrderItem>()
                .Property(ct => ct.Quantity)
                .HasDefaultValue(1);

            builder.Entity<OrderItem>()
                .Property(ct => ct.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Entity<OrderItem>()
                .Property(ct => ct.SubTotal)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Entity<Order>()
                .Property(d => d.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Entity<Order>()
                .Property(d => d.ShippingFee)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Configure automatic timestamp updates
            builder.Entity<Product>()
                .Property(b => b.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<Category>()
                .Property(b => b.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<Cart>()
                .Property(b => b.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<CartItem>()
                .Property(b => b.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<Order>()
                .Property(b => b.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<OrderItem>()
                .Property(b => b.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<Payment>()
                .Property(b => b.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
