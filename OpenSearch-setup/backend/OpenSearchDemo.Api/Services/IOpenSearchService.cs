using OpenSearch.Client;

namespace OpenSearchDemo.Api.Services;

/// <summary>
/// Core OpenSearch operations service.
/// </summary>
public interface IOpenSearchService
{
    /// <summary>
    /// Creates an index with specified mapping.
    /// </summary>
    Task<bool> CreateIndexAsync<T>(string indexName, CancellationToken ct = default) where T : class;
    
    /// <summary>
    /// Deletes an index.
    /// </summary>
    Task<bool> DeleteIndexAsync(string indexName, CancellationToken ct = default);
    
    /// <summary>
    /// Checks if an index exists.
    /// </summary>
    Task<bool> IndexExistsAsync(string indexName, CancellationToken ct = default);
    
    /// <summary>
    /// Indexes a single document.
    /// </summary>
    Task<bool> IndexDocumentAsync<T>(string indexName, T document, string id, CancellationToken ct = default) where T : class;
    
    /// <summary>
    /// Bulk indexes multiple documents.
    /// </summary>
    Task<BulkResponse> BulkIndexAsync<T>(string indexName, IEnumerable<T> documents, Func<T, string> idSelector, CancellationToken ct = default) where T : class;
    
    /// <summary>
    /// Gets a document by ID.
    /// </summary>
    Task<T?> GetDocumentAsync<T>(string indexName, string id, CancellationToken ct = default) where T : class;
    
    /// <summary>
    /// Deletes a document by ID.
    /// </summary>
    Task<bool> DeleteDocumentAsync(string indexName, string id, CancellationToken ct = default);
    
    /// <summary>
    /// Executes a search query.
    /// </summary>
    Task<ISearchResponse<T>> SearchAsync<T>(string indexName, Func<SearchDescriptor<T>, ISearchRequest> searchDescriptor, CancellationToken ct = default) where T : class;
    
    /// <summary>
    /// Refreshes an index to make changes searchable.
    /// </summary>
    Task RefreshIndexAsync(string indexName, CancellationToken ct = default);
}
