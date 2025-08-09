using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<int> AddAsync(User entity, CancellationToken ct = default);
    Task UpdateAsync(User entity, CancellationToken ct = default);
}
