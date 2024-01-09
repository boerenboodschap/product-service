using product_service.Models;
using product_service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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

    public class ProductFilterOptions
    {
        public string? Name { get; set; }
        public string? Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> Get([FromQuery] ProductFilterOptions filterOptions)
    {
        try
        {
            IQueryable<Product> query = _ProductsService.GetQueryable(); // Get the queryable object from your service/repository

            // Apply filters based on filter options
            if (filterOptions != null)
            {
                if (!string.IsNullOrEmpty(filterOptions.Name))
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    query = query.Where(p => p.Name.Contains(filterOptions.Name));
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                }

                // Add more filters based on other properties if needed
                // Example: filter by category, price range, etc.
                // query = query.Where(p => p.Category == filterOptions.Category);
                // query = query.Where(p => p.Price >= filterOptions.MinPrice && p.Price <= filterOptions.MaxPrice);
            }

            // Pagination
#pragma warning disable CS8602 // Dereference of a possibly null reference.

            int pageNumber = filterOptions.PageNumber ?? 1; // Default to page 1 if pageNumber is not provided
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            int pageSize = filterOptions.PageSize ?? 10; // Default page size to 10 if pageSize is not provided

            var paginatedProducts = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

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