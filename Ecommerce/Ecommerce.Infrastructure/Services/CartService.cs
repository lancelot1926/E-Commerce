using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Cart;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _db;
    public CartService(AppDbContext db) => _db = db;

    public async Task<Result<CartDto>> GetAsync(int userId, CancellationToken ct = default)
    {
        var cart = await EnsureCart(userId, ct);

        var rows = await _db.CartItems
            .Where(i => i.CartId == cart.Id)
            .Join(_db.Products, i => i.ProductId, p => p.Id,
                (i, p) => new { p.Name, i.ProductId, Price = i.UnitPrice, Qty = i.Quantity })
            .ToListAsync(ct);

        var dto = new CartDto
        {
            UserId = cart.UserId,
            Items = rows.Select(r => new CartItemDto
            {
                ProductId = r.ProductId,
                Name = r.Name,
                UnitPrice = r.Price,
                Quantity = r.Qty
            }).ToList()
        };

        return Result<CartDto>.Ok(dto);
    }

    public async Task<Result<CartDto>> AddOrUpdateItemAsync(int userId, AddCartItemRequest req, CancellationToken ct = default)
    {
        var cart = await EnsureCart(userId, ct);

        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == req.ProductId, ct);
        if (product is null || !product.IsActive) return Result<CartDto>.Fail("Product not available.");
        if (req.Quantity <= 0) return Result<CartDto>.Fail("Quantity must be >= 1.");
        if (product.StockQuantity < req.Quantity) return Result<CartDto>.Fail("Insufficient stock.");

        var item = await _db.CartItems.FirstOrDefaultAsync(i => i.CartId == cart.Id && i.ProductId == req.ProductId, ct);
        if (item is null)
        {
            item = new CartItem { CartId = cart.Id, ProductId = req.ProductId, Quantity = req.Quantity, UnitPrice = product.Price };
            _db.CartItems.Add(item);
        }
        else
        {
            item.Quantity = req.Quantity;      // set quantity (you can switch to += to increment)
            item.UnitPrice = product.Price;    // refresh price snapshot
        }

        await _db.SaveChangesAsync(ct);
        return await GetAsync(userId, ct);
    }

    public async Task<Result> RemoveItemAsync(int userId, int productId, CancellationToken ct = default)
    {
        var cart = await EnsureCart(userId, ct);
        var item = await _db.CartItems.FirstOrDefaultAsync(i => i.CartId == cart.Id && i.ProductId == productId, ct);
        if (item is null) return Result.Ok();
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ClearAsync(int userId, CancellationToken ct = default)
    {
        var cart = await EnsureCart(userId, ct);
        var items = _db.CartItems.Where(i => i.CartId == cart.Id);
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private async Task<Cart> EnsureCart(int userId, CancellationToken ct)
    {
        var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync(ct);
        }
        return cart;
    }
}
