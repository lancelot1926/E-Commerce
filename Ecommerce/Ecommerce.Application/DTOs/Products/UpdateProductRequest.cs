using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs.Products;

public class UpdateProductRequest
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(0, 1_000_000)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public string? Sku { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? MainImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
