using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;
    public ProductRepository(AppDbContext db) => _db = db;

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
        => await _db.Products.AsNoTracking().ToListAsync(ct);

    public async Task<int> AddAsync(Product entity, CancellationToken ct = default)
    {
        _db.Products.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task UpdateAsync(Product entity, CancellationToken ct = default)
    {
        _db.Products.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var p = await _db.Products.FindAsync(new object?[] { id }, ct);
        if (p is null) return;
        _db.Products.Remove(p);
        await _db.SaveChangesAsync(ct);
    }
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
    int pageNumber,
    int pageSize,
    string? search,
    string? category,
    string? brand,
    string? sortBy,
    string? sortOrder,
    decimal? minPrice,
    decimal? maxPrice,
    bool? isActive,
    CancellationToken ct = default)
    {
        var query = _db.Products.AsNoTracking().AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(p => p.Brand == brand);

        // Price range
        if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

        // Active flag
        if (isActive.HasValue) query = query.Where(p => p.IsActive == isActive.Value);

        // Sorting
        bool desc = (sortOrder ?? "asc").Equals("desc", StringComparison.OrdinalIgnoreCase);
        query = (sortBy ?? "").ToLower() switch
        {
            "name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            _ => query.OrderBy(p => p.Id) // default stable sort
        };


        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

}