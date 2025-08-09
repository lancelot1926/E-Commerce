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
