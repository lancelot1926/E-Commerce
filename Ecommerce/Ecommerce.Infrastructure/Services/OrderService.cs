using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    public OrderService(AppDbContext db) => _db = db;

    public async Task<Result<OrderDto>> CheckoutAsync(int userId, CancellationToken ct = default)
    {
        var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (cart is null) return Result<OrderDto>.Fail("Cart is empty.");

        var items = await _db.CartItems.Where(i => i.CartId == cart.Id).ToListAsync(ct);
        if (items.Count == 0) return Result<OrderDto>.Fail("Cart is empty.");

        // Load products to check stock & names
        var productIds = items.Select(i => i.ProductId).ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(ct);

        // verify stock
        foreach (var it in items)
        {
            var p = products.First(x => x.Id == it.ProductId);
            if (!p.IsActive) return Result<OrderDto>.Fail($"Product '{p.Name}' is inactive.");
            if (p.StockQuantity < it.Quantity) return Result<OrderDto>.Fail($"Not enough stock for '{p.Name}'.");
        }

        // create order + decrement stock
        var order = new Order { UserId = userId, Status = OrderStatus.Paid };
        foreach (var it in items)
        {
            var p = products.First(x => x.Id == it.ProductId);
            p.StockQuantity -= it.Quantity;

            order.Items.Add(new OrderItem
            {
                Order = order,
                ProductId = p.Id,
                ProductName = p.Name,
                UnitPrice = it.UnitPrice,
                Quantity = it.Quantity
            });
        }

        order.Total = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        _db.Orders.Add(order);
        // clear cart
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(order).Reference(o => o.User).LoadAsync(ct);

        return Result<OrderDto>.Ok(order.ToDto());
    }

    public async Task<Result<OrderDto>> GetAsync(int id, int userId, CancellationToken ct = default)
    {
        var order = await _db.Orders.Include(o => o.Items).Include(o=>o.User).FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct);
        if (order is null) return Result<OrderDto>.Fail("Order not found.");
        return Result<OrderDto>.Ok(order.ToDto());
    }

    public async Task<Result<IReadOnlyList<OrderDto>>> ListAsync(int userId, CancellationToken ct = default)
    {
        var list = await _db.Orders.AsNoTracking().Include(o => o.Items)
            .Include(o => o.User)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.Id)
            .ToListAsync(ct);

        return Result<IReadOnlyList<OrderDto>>.Ok(list.Select(o => o.ToDto()).ToList());
    }

    public async Task<Result<List<OrderDto>>>GetAllAsync(CancellationToken ct = default)
    {
        var orders=await _db.Orders.Include(o=>o.Items).Include(o => o.User).OrderByDescending(o => o.Id).ToListAsync(ct);
        var dto = orders.Select(o => new OrderDto
        {
            Id = o.Id,
            UserId = o.UserId,
            Status = o.Status,
            Total = o.Items.Sum(i => i.UnitPrice * i.Quantity),
            Customer = o.User != null ? o.User.ToDto() : null,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        }).ToList();

        return Result<List<OrderDto>>.Ok(dto);
    }

    //For Admin
    public async Task<Result<OrderDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o=>o.User)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order == null) return Result<OrderDto>.Fail("Order not found.");
        return Result<OrderDto>.Ok(order.ToDto());
    }

    public async Task<Result<OrderDto>> ChangeStateAsync(int id,int state, CancellationToken ct = default)
    {
        if(!Enum.IsDefined(typeof(OrderStatus),state)) return Result<OrderDto>.Fail("Invalid state value.");
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null) return Result<OrderDto>.Fail("Order not found.");
        order.Status = (OrderStatus)state;
        order.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync(ct);
        return Result<OrderDto>.Ok(order.ToDto());
    }
}
