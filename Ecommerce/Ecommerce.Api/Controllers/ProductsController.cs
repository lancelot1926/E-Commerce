using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;
    public ProductsController(IProductService products) => _products = products;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _products.GetAllAsync(ct);
        return Ok(result.Data); // empty list is fine
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var result = await _products.GetAsync(id, ct);
        if (!result.Succeeded) return NotFound(result.Error);
        return Ok(result.Data);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _products.CreateAsync(req, ct);
        if (!result.Succeeded) return BadRequest(result.Error);
        return CreatedAtAction(nameof(Get), new { id = result.Data!.Id }, result.Data);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _products.UpdateAsync(id, req, ct);
        if (!result.Succeeded) return NotFound(result.Error);
        return Ok(result.Data);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _products.DeleteAsync(id, ct);
        if (!result.Succeeded) return BadRequest(result.Error);
        return NoContent();
    }
}
