using product_service.Models;
using product_service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace product_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductsService _ProductsService;

    public ProductsController(ProductsService ProductsService) =>
        _ProductsService = ProductsService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string name = "",
            [FromQuery] string userId = ""
        )
    {
        try
        {
            var products = await _ProductsService.GetAsync(page, pageSize, name, userId);

            if (products == null || products.Count == 0)
            {
                return NotFound();
            }

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Product>> Get(string id)
    {
        var Product = await _ProductsService.GetAsync(id);

        if (Product is null)
        {
            return NotFound();
        }

        return Product;
    }

    [HttpGet("{id:length(24)}/stock/{amount}")]
    public async Task<IActionResult> Get(string id, int amount)
    {
        var Product = await _ProductsService.GetAsync(id);

        if (Product is null)
        {
            return NotFound();
        }

        // remove the specified stock amount
        if (amount > Product.Stock)
        {
            return BadRequest();
        }
        Product.Stock -= amount;

        await _ProductsService.UpdateAsync(id, Product);

        return NoContent();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Post(Product newProduct)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        newProduct.UserId = userId;

        await _ProductsService.CreateAsync(newProduct);

        return CreatedAtAction(nameof(Get), new { id = newProduct.Id }, newProduct);
    }

    [HttpPut("{id:length(24)}")]
    [Authorize]
    public async Task<IActionResult> Update(string id, Product updatedProduct)
    {

        var Product = await _ProductsService.GetAsync(id);
        if (Product is null)
        {
            return NotFound();
        }

        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != Product.UserId)
        {
            return Unauthorized();
        }

        updatedProduct.Id = Product.Id;

        await _ProductsService.UpdateAsync(id, updatedProduct);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {

        var Product = await _ProductsService.GetAsync(id);
        if (Product is null)
        {
            return NotFound();
        }

        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != Product.UserId)
        {
            return Unauthorized();
        }

        await _ProductsService.RemoveAsync(id);

        return NoContent();
    }
}