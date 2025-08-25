using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Product
        modelBuilder.Entity<Product>(b =>
        {
            b.Property(p => p.Name).HasMaxLength(200).IsRequired();
            b.Property(p => p.Description).HasMaxLength(2000);
            b.Property(p => p.Price).HasColumnType("decimal(18,2)");
            b.Property(p => p.Sku).HasMaxLength(100);
            b.Property(p => p.Brand).HasMaxLength(100);
            b.Property(p => p.Category).HasMaxLength(100);
            b.Property(p => p.MainImageUrl).HasMaxLength(500);
            b.HasIndex(p => p.Sku).HasDatabaseName("IX_Product_Sku");
        });

        // User
        modelBuilder.Entity<User>(b =>
        {
            b.Property(u => u.Name).HasMaxLength(100).IsRequired();
            b.Property(u => u.Surname).HasMaxLength(100).IsRequired();
            b.Property(u => u.Email).HasMaxLength(256).IsRequired();
            b.Property(u => u.PhoneNumber).HasMaxLength(30).IsRequired();
            b.Property(u => u.PasswordHash).IsRequired();

            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.Role).HasMaxLength(50).IsRequired().HasDefaultValue("User");
            b.Property(u => u.IsBanned);

            // Address as owned (same table)
            b.OwnsOne(u => u.Address, adr =>
            {
                adr.Property(a => a.Line1).HasMaxLength(200).HasColumnName("Address_Line1");
                adr.Property(a => a.Line2).HasMaxLength(200).HasColumnName("Address_Line2");
                adr.Property(a => a.City).HasMaxLength(100).HasColumnName("Address_City");
                adr.Property(a => a.State).HasMaxLength(100).HasColumnName("Address_State");
                adr.Property(a => a.PostalCode).HasMaxLength(20).HasColumnName("Address_PostalCode");
                adr.Property(a => a.Country).HasMaxLength(100).HasColumnName("Address_Country");
            });
        });

        // Cart
        modelBuilder.Entity<Cart>(b =>
        {
            b.HasIndex(c => c.UserId).IsUnique(); // one cart per user
            b.HasMany<CartItem>().WithOne().HasForeignKey(ci => ci.CartId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(b =>
        {
            b.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
            b.Property(ci => ci.UnitPrice).HasColumnType("decimal(18,2)");
        });

        // Order
        modelBuilder.Entity<Order>(b =>
        {
            b.Property(o => o.Total).HasColumnType("decimal(18,2)");
            b.Property(o => o.PaymentId)
     .HasMaxLength(64)          // or whatever
     .IsRequired(false);        // <-- important
            b.HasMany<OrderItem>().WithOne(oi => oi.Order).HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            b.Property(oi => oi.ProductName).HasMaxLength(200);
        });

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = utcNow;

            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = utcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
