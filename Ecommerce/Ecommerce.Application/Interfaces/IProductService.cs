using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Products;

namespace Ecommerce.Application.Interfaces;

public interface IProductService
{
    Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<Result<ProductDto>> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    Task<Result<ProductDto>> GetAsync(int id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync(CancellationToken ct = default);
}