using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Products;

public class ProductQueryParameters
{
    private const int MaxPageSize = 50;

    public int PageNumber { get; set; } = 1;

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? SortBy { get; set; } // name, price
    public string? SortOrder { get; set; } = "asc"; // asc/desc
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsActive { get; set; }
}
