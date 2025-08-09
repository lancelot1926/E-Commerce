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
}