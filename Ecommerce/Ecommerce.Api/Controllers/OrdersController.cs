using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static Ecommerce.Api.Controllers.UsersController;

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

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var isAdmin = User.IsInRole("Admin");
        var result = isAdmin
        ? await _orders.GetByIdAsync(id, ct)              // no user filter
        : await _orders.GetAsync(id, UserId(), ct);       // user-scoped

        if (!result.Succeeded) return NotFound(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok((await _orders.ListAsync(UserId(), ct)).Data);

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var r = await _orders.GetAllAsync(ct);
        return r.Succeeded ? Ok(r.Data) : BadRequest(new { error = r.Error });
    }


    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/state")]
    public async Task<IActionResult> StateUpdate(int id, [FromBody] StatRequest body, CancellationToken ct)
    {
        var isAdmin = User.IsInRole("Admin");
        var ord = await _orders.GetByIdAsync(id, ct);             // no user filter

        if (!ord.Succeeded) return NotFound(new { error = ord.Error });
        var result=await _orders.ChangeStateAsync(id, body.state, ct);
        return Ok(result.Data);
    }

    public record StatRequest(int state);
}
