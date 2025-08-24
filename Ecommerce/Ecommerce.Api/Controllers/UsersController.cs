using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;
    public UsersController(IUserService users) => _users = users;

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _users.GetByIdAsync(id, ct);
        if (!result.Succeeded) return NotFound(result.Error);
        return Ok(result.Data);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("userList")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _users.GetAllAsync(ct);
        if (!result.Succeeded) return NotFound(result.Error);
        return Ok(result.Data);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/ban")]
    public async Task<IActionResult> BanTheUser(int id, [FromBody] BanRequest body, CancellationToken ct)
    {
        var result = await _users.BanAsync(id, body.Banned, ct);
        if (!result.Succeeded) return NotFound(result.Error);
        return Ok(result.Data);

    }
    public record BanRequest(bool Banned);
}
