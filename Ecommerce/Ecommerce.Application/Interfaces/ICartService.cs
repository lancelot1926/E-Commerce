using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Cart;

namespace Ecommerce.Application.Interfaces;

public interface ICartService
{
    Task<Result<CartDto>> GetAsync(int userId, CancellationToken ct = default);
    Task<Result<CartDto>> AddOrUpdateItemAsync(int userId, AddCartItemRequest req, CancellationToken ct = default);
    Task<Result> RemoveItemAsync(int userId, int productId, CancellationToken ct = default);
    Task<Result> ClearAsync(int userId, CancellationToken ct = default);
}
