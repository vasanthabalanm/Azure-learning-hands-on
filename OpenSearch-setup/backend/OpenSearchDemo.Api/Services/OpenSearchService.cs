using OpenSearch.Client;
using OpenSearchDemo.Api.Models;

namespace OpenSearchDemo.Api.Services;

/// <summary>
/// Implementation of OpenSearch operations.
/// </summary>
public class OpenSearchService : IOpenSearchService
{
    private readonly IOpenSearchClient _client;
    private readonly ILogger<OpenSearchService> _logger;

    public OpenSearchService(IOpenSearchClient client, ILogger<OpenSearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> CreateIndexAsync<T>(string indexName, CancellationToken ct = default) where T : class
    {
        var existsResponse = await _client.Indices.ExistsAsync(indexName, ct: ct);
        if (existsResponse.Exists)
        {
            _logger.LogInformation("Index {IndexName} already exists", indexName);
            return true;
        }

        // Create index with custom mapping for Product
        var createResponse = await _client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0)
                .Setting("index.max_result_window", 50000)
            )
            .Map<Product>(m => m
                .Properties(p => p
                    .Keyword(k => k.Name(n => n.Id))
                    .Text(t => t
                        .Name(n => n.Name)
                        .Analyzer("standard")
                        .Fields(f => f.Keyword(kw => kw.Name("keyword")))
                    )
                    .Text(t => t.Name(n => n.Description).Analyzer("standard"))
                    .Keyword(k => k.Name(n => n.Category))
                    .Keyword(k => k.Name(n => n.Subcategory))
                    .Number(n => n.Name(p => p.Price).Type(NumberType.Float))
                    .Keyword(k => k.Name(n => n.Brand))
                    .Keyword(k => k.Name(n => n.Tags))
                    .Number(n => n.Name(p => p.Rating).Type(NumberType.Float))
                    .Number(n => n.Name(p => p.Stock).Type(NumberType.Integer))
                    .Boolean(b => b.Name(n => n.IsActive))
                    .Date(d => d.Name(n => n.CreatedAt))
                    .Date(d => d.Name(n => n.UpdatedAt))
                    .Object<ProductSpecifications>(o => o
                        .Name(n => n.Specifications)
                        .Properties(sp => sp
                            .Number(n => n.Name(s => s.Weight).Type(NumberType.Float))
                            .Keyword(k => k.Name(s => s.Dimensions))
                            .Keyword(k => k.Name(s => s.Color))
                        )
                    )
                )
            )
        , ct);

        if (!createResponse.IsValid)
        {
            _logger.LogError("Failed to create index {IndexName}: {Error}", indexName, createResponse.OriginalException?.Message);
            return false;
        }

        _logger.LogInformation("Created index {IndexName}", indexName);
        return true;
    }

    public async Task<bool> DeleteIndexAsync(string indexName, CancellationToken ct = default)
    {
        var response = await _client.Indices.DeleteAsync(indexName, ct: ct);
        if (response.IsValid)
        {
            _logger.LogInformation("Deleted index {IndexName}", indexName);
        }
        return response.IsValid;
    }

    public async Task<bool> IndexExistsAsync(string indexName, CancellationToken ct = default)
    {
        var response = await _client.Indices.ExistsAsync(indexName, ct: ct);
        return response.Exists;
    }

    public async Task<bool> IndexDocumentAsync<T>(string indexName, T document, string id, CancellationToken ct = default) where T : class
    {
        var response = await _client.IndexAsync(document, i => i
            .Index(indexName)
            .Id(id)
        , ct);

        if (!response.IsValid)
        {
            _logger.LogError("Failed to index document {Id}: {Error}", id, response.OriginalException?.Message);
            return false;
        }

        // Refresh index to make document immediately searchable
        await _client.Indices.RefreshAsync(indexName, ct: ct);
        return true;
    }

    public async Task<BulkResponse> BulkIndexAsync<T>(string indexName, IEnumerable<T> documents, Func<T, string> idSelector, CancellationToken ct = default) where T : class
    {
        var bulkDescriptor = new BulkDescriptor();
        
        foreach (var doc in documents)
        {
            bulkDescriptor.Index<T>(i => i
                .Index(indexName)
                .Id(idSelector(doc))
                .Document(doc)
            );
        }

        var response = await _client.BulkAsync(bulkDescriptor, ct);
        
        if (response.Errors)
        {
            foreach (var item in response.ItemsWithErrors)
            {
                _logger.LogError("Bulk index error for {Id}: {Error}", item.Id, item.Error?.Reason);
            }
        }
        else
        {
            _logger.LogInformation("Bulk indexed {Count} documents", response.Items.Count);
        }

        // Refresh to make documents searchable
        await _client.Indices.RefreshAsync(indexName, ct: ct);

        return response;
    }

    public async Task<T?> GetDocumentAsync<T>(string indexName, string id, CancellationToken ct = default) where T : class
    {
        var response = await _client.GetAsync<T>(id, g => g.Index(indexName), ct);
        return response.Found ? response.Source : null;
    }

    public async Task<bool> DeleteDocumentAsync(string indexName, string id, CancellationToken ct = default)
    {
        var response = await _client.DeleteAsync<object>(id, d => d.Index(indexName), ct);
        if (response.IsValid)
        {
            await _client.Indices.RefreshAsync(indexName, ct: ct);
        }
        return response.IsValid;
    }

    public async Task<ISearchResponse<T>> SearchAsync<T>(string indexName, Func<SearchDescriptor<T>, ISearchRequest> searchDescriptor, CancellationToken ct = default) where T : class
    {
        return await _client.SearchAsync<T>(s => searchDescriptor(s.Index(indexName)), ct);
    }

    public async Task RefreshIndexAsync(string indexName, CancellationToken ct = default)
    {
        await _client.Indices.RefreshAsync(indexName, ct: ct);
    }
}
