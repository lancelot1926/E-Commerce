using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Orders;

namespace Ecommerce.Application.Interfaces;

public interface IOrderService
{
    Task<Result<OrderDto>> CheckoutAsync(int userId, CancellationToken ct = default);
    Task<Result<OrderDto>> GetAsync(int id, int userId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<OrderDto>>> ListAsync(int userId, CancellationToken ct = default);
}
