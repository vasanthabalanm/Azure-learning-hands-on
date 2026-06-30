using OpenSearchDemo.Api.Models;

namespace OpenSearchDemo.Api.Services;

/// <summary>
/// Product-specific search operations.
/// </summary>
public interface IProductSearchService
{
    Task<Models.SearchResponse<Product>> SearchProductsAsync(ProductSearchRequest request, CancellationToken ct = default);
    Task<List<AutocompleteSuggestion>> GetAutocompleteAsync(string query, int limit = 5, CancellationToken ct = default);
    Task<bool> IndexProductAsync(Product product, CancellationToken ct = default);
    Task<int> BulkIndexProductsAsync(IEnumerable<Product> products, CancellationToken ct = default);
    Task<bool> DeleteProductAsync(string productId, CancellationToken ct = default);
    Task<Product?> GetProductByIdAsync(string productId, CancellationToken ct = default);
}
