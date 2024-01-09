namespace product_service.Models;

public class ProductFilterOptions
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}