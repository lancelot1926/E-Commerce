using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Pricing & stock
    public decimal Price { get; set; }          // e.g., 199.99
    public int StockQuantity { get; set; }      // units available

    // Basic catalog metadata
    public string? Sku { get; set; }            // stock keeping unit
    public string? Brand { get; set; }
    public string? Category { get; set; }       // keep as string for now

    // Media
    public string? MainImageUrl { get; set; }

    // Flags
    public bool IsActive { get; set; } = true;  // soft on/off for listing

    // Simple domain guard methods (no heavy DDD)
    public void IncreaseStock(int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        StockQuantity += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecreaseStock(int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (amount > StockQuantity) throw new InvalidOperationException("Not enough stock.");
        StockQuantity -= amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0) throw new ArgumentOutOfRangeException(nameof(newPrice));
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}