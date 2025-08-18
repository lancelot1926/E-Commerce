using Ecommerce.Application.DTOs.Cart;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ecommerce.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cart;
    public CartController(ICartService cart) => _cart = cart;

    private int UserId()
    {
        var id =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(id, out var userId))
            throw new UnauthorizedAccessException("Missing or invalid user id claim.");

        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok((await _cart.GetAsync(UserId(), ct)).Data);

    [HttpPost("items")]
    public async Task<IActionResult> AddOrUpdate([FromBody] AddCartItemRequest req, CancellationToken ct)
    {
        var result = await _cart.AddOrUpdateItemAsync(UserId(), req, ct);
        if (!result.Succeeded) return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> Remove(int productId, CancellationToken ct)
    {
        var result = await _cart.RemoveItemAsync(UserId(), productId, ct);
        if (!result.Succeeded) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        var result = await _cart.ClearAsync(UserId(), ct);
        if (!result.Succeeded) return BadRequest(new { error = result.Error });
        return NoContent();
    }
}
