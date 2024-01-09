using product_service.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<Product>> GetAsync(int page, int pageSize, string filter)
    {
        var filterBuilder = Builders<Product>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filterBuilder = Builders<Product>.Filter.Where(x => x.Name.ToLower().Contains(filter.ToLower()));
        }

        var products = await _productsCollection.Find(filterBuilder)
                                .Skip((page - 1) * pageSize)
                                .Limit(pageSize)
                                .ToListAsync();

        return products;
    }

    public async Task<Product?> GetAsync(string id) =>
        await _productsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Product newProduct) =>
        await _productsCollection.InsertOneAsync(newProduct);

    public async Task UpdateAsync(string id, Product updatedProduct) =>
        await _productsCollection.ReplaceOneAsync(x => x.Id == id, updatedProduct);

    public async Task RemoveAsync(string id) =>
        await _productsCollection.DeleteOneAsync(x => x.Id == id);
}