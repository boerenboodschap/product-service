using product_service.Models;
using product_service.Services;
using Microsoft.AspNetCore.Mvc;

namespace product_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductsService _ProductsService;

    public ProductsController(ProductsService ProductsService) =>
        _ProductsService = ProductsService;

    // [HttpGet]
    // public async Task<List<Product>> Get() =>
    //     await _ProductsService.GetAsync();

    [HttpGet]
    public async Task<ActionResult<List<Product>>> Get([FromQuery] ProductFilterOptions filterOptions)
    {
        try
        {
            var paginatedProducts = await _ProductsService.GetFilteredProductsAsync(filterOptions);
            return Ok(paginatedProducts);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
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
    public async Task<IActionResult> Post(Product newProduct)
    {
        await _ProductsService.CreateAsync(newProduct);

        return CreatedAtAction(nameof(Get), new { id = newProduct.Id }, newProduct);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Product updatedProduct)
    {
        var Product = await _ProductsService.GetAsync(id);

        if (Product is null)
        {
            return NotFound();
        }

        updatedProduct.Id = Product.Id;

        await _ProductsService.UpdateAsync(id, updatedProduct);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var Product = await _ProductsService.GetAsync(id);

        if (Product is null)
        {
            return NotFound();
        }

        await _ProductsService.RemoveAsync(id);

        return NoContent();
    }
}