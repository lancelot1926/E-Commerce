using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo) => _repo = repo;

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var entity = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Sku = request.Sku,
            Brand = request.Brand,
            Category = request.Category,
            MainImageUrl = request.MainImageUrl,
            IsActive = true
        };

        var id = await _repo.AddAsync(entity, ct);
        entity.Id = id;

        return Result<ProductDto>.Ok(entity.ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        await _repo.DeleteAsync(id, ct);
        return Result.Ok();
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _repo.GetAllAsync(ct);
        return Result<IReadOnlyList<ProductDto>>.Ok(list.Select(p => p.ToDto()).ToList());
    }

    public async Task<Result<ProductDto>> GetAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<ProductDto>.Fail("Product not found.");
        return Result<ProductDto>.Ok(entity.ToDto());
    }

    public async Task<Result<ProductDto>> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<ProductDto>.Fail("Product not found.");

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Price = request.Price;
        entity.StockQuantity = request.StockQuantity;
        entity.Sku = request.Sku;
        entity.Brand = request.Brand;
        entity.Category = request.Category;
        entity.MainImageUrl = request.MainImageUrl;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entity, ct);
        return Result<ProductDto>.Ok(entity.ToDto());
    }
}