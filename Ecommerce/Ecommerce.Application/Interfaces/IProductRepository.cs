using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task<int> AddAsync(Product entity, CancellationToken ct = default);
    Task UpdateAsync(Product entity, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
