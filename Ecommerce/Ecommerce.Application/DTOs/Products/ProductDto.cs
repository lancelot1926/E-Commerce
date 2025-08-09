using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? Sku { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? MainImageUrl { get; set; }
    public bool IsActive { get; set; }
}
