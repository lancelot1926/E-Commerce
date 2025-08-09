using Ecommerce.Api.Data;
using Ecommerce.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _db;
    public CartController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ShoppingCart>> Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                     User.FindFirstValue("sub")!;
        var cart = await _db.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            cart = new ShoppingCart { UserId = userId };
            _db.ShoppingCarts.Add(cart);
            await _db.SaveChangesAsync();
        }
        return cart;
    }

    public record AddItemDto(int ProductId, int Quantity, decimal UnitPrice);

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddItemDto dto)
    {
        var userId = User.FindFirstValue("sub")!;
        var cart = await _db.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? new ShoppingCart { UserId = userId };

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existing is null)
            cart.Items.Add(new CartItem { ProductId = dto.ProductId, Quantity = dto.Quantity, UnitPrice = dto.UnitPrice });
        else
            existing.Quantity += dto.Quantity;

        _db.ShoppingCarts.Update(cart);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        var userId = User.FindFirstValue("sub")!;
        var cart = await _db.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null) return NotFound();

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return NotFound();

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
