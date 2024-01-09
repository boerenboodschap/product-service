using product_service.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;


namespace product_service.Services;

public class ProductsService
{
    private readonly IMongoCollection<Product> _productsCollection;

    public ProductsService(
        IOptions<ProductDatabaseSettings> ProductDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            ProductDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            ProductDatabaseSettings.Value.DatabaseName);

        _productsCollection = mongoDatabase.GetCollection<Product>(
            ProductDatabaseSettings.Value.ProductsCollectionName);
    }

    public async Task<List<Product>> GetFilteredProductsAsync(ProductFilterOptions filterOptions)
    {
        try
        {
            IQueryable<Product> query = _productsCollection.AsQueryable();

            // Apply filters based on filter options
            if (filterOptions != null)
            {
                if (!string.IsNullOrEmpty(filterOptions.Name))
                {
                    query = query.Where(p => p.Name.Contains(filterOptions.Name));
                }

                // Add more filters based on other properties if needed
                // query = query.Where(p => p.Category == filterOptions.Category);
                // query = query.Where(p => p.Price >= filterOptions.MinPrice && p.Price <= filterOptions.MaxPrice);
            }

            // Pagination
            int pageNumber = filterOptions.PageNumber ?? 1; // Default to page 1 if pageNumber is not provided
            int pageSize = filterOptions.PageSize ?? 10; // Default page size to 10 if pageSize is not provided

            var paginatedProducts = await query.Skip((pageNumber - 1) * pageSize)
                                              .Take(pageSize)
                                              .ToListAsync();

            return paginatedProducts;
        }
        catch (Exception ex)
        {
            // Handle exceptions appropriately
            throw new Exception("Error retrieving filtered products", ex);
        }
    }

    public async Task<List<Product>> GetAsync() =>
        await _productsCollection.Find(_ => true).ToListAsync();

    public async Task<Product?> GetAsync(string id) =>
        await _productsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Product newProduct) =>
        await _productsCollection.InsertOneAsync(newProduct);

    public async Task UpdateAsync(string id, Product updatedProduct) =>
        await _productsCollection.ReplaceOneAsync(x => x.Id == id, updatedProduct);

    public async Task RemoveAsync(string id) =>
        await _productsCollection.DeleteOneAsync(x => x.Id == id);
}