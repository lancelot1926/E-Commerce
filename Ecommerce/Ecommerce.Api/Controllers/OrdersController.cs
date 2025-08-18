using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ecommerce.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;
    public OrdersController(IOrderService orders) => _orders = orders;

    private int UserId()
    {
        var id =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(id, out var userId))
            throw new UnauthorizedAccessException("Missing or invalid user id claim.");

        return userId;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CancellationToken ct)
    {
        var result = await _orders.CheckoutAsync(UserId(), ct);
        if (!result.Succeeded) return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var result = await _orders.GetAsync(id, UserId(), ct);
        if (!result.Succeeded) return NotFound(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok((await _orders.ListAsync(UserId(), ct)).Data);
}
