# Module 4: .NET Backend Integration

## 🎯 Learning Objectives

By the end of this module, you will:

- ✅ Set up OpenSearch.Client in .NET
- ✅ Create index management services
- ✅ Implement document CRUD operations
- ✅ Build search APIs with filtering and pagination
- ✅ Sync data from PostgreSQL to OpenSearch

---

## 4.1 Project Setup

### Step 1: Create the .NET Project

Open terminal in `OpenSearch-setup` folder:

```powershell
# Create solution and API project
dotnet new sln -n OpenSearchDemo
dotnet new webapi -n OpenSearchDemo.Api -o backend/OpenSearchDemo.Api
dotnet sln add backend/OpenSearchDemo.Api/OpenSearchDemo.Api.csproj

# Navigate to project
cd backend/OpenSearchDemo.Api
```

### Step 2: Install NuGet Packages

```powershell
# OpenSearch client
dotnet add package OpenSearch.Client

# Entity Framework for PostgreSQL sync
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design

# API documentation
dotnet add package Scalar.AspNetCore
```

### Step 3: Project Structure

```
backend/OpenSearchDemo.Api/
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Controllers/
│   ├── ProductsController.cs
│   ├── SearchController.cs
│   └── IndexController.cs
├── Models/
│   ├── Product.cs
│   └── SearchModels.cs
├── Services/
│   ├── IOpenSearchService.cs
│   ├── OpenSearchService.cs
│   ├── IProductSearchService.cs
│   └── ProductSearchService.cs
├── Data/
│   ├── AppDbContext.cs
│   └── DbSeeder.cs
└── Extensions/
    └── OpenSearchExtensions.cs
```

---

## 4.2 Configuration

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=opensearch_demo;Username=postgres;Password=admin;"
  },
  "OpenSearch": {
    "Uri": "http://localhost:9200",
    "DefaultIndex": "products",
    "Username": "",
    "Password": ""
  },
  "AllowedOrigins": "http://localhost:4200"
}
```

---

## 4.3 Models

### Product.cs

```csharp
namespace OpenSearchDemo.Api.Models;

/// <summary>
/// Product document model for OpenSearch indexing.
/// </summary>
public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Brand { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public double Rating { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ProductSpecifications? Specifications { get; set; }
}

public class ProductSpecifications
{
    public double? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? Color { get; set; }
}
```

### SearchModels.cs

```csharp
namespace OpenSearchDemo.Api.Models;

/// <summary>
/// Search request parameters.
/// </summary>
public class ProductSearchRequest
{
    public string? Query { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public string? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public double? MinRating { get; set; }
    public List<string>? Tags { get; set; }
    public bool? InStock { get; set; }
    public string SortBy { get; set; } = "relevance";
    public string SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Search response with pagination.
/// </summary>
public class SearchResponse<T>
{
    public List<SearchHit<T>> Hits { get; set; } = [];
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
    public long Took { get; set; }
    public Dictionary<string, List<FacetBucket>>? Facets { get; set; }
}

/// <summary>
/// Individual search hit with highlighting.
/// </summary>
public class SearchHit<T>
{
    public T Document { get; set; } = default!;
    public double Score { get; set; }
    public Dictionary<string, List<string>>? Highlights { get; set; }
}

/// <summary>
/// Facet bucket for aggregations.
/// </summary>
public class FacetBucket
{
    public string Key { get; set; } = string.Empty;
    public long Count { get; set; }
}

/// <summary>
/// Autocomplete suggestion.
/// </summary>
public class AutocompleteSuggestion
{
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
```

---

## 4.4 OpenSearch Service

### IOpenSearchService.cs

```csharp
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
```

### OpenSearchService.cs

```csharp
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

        var createResponse = await _client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0)
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("autocomplete", ca => ca
                            .Tokenizer("autocomplete_tokenizer")
                            .Filters("lowercase")
                        )
                        .Custom("autocomplete_search", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase")
                        )
                    )
                    .Tokenizers(t => t
                        .EdgeNGram("autocomplete_tokenizer", e => e
                            .MinGram(2)
                            .MaxGram(10)
                            .TokenChars(TokenChar.Letter, TokenChar.Digit)
                        )
                    )
                )
            )
            .Map<T>(m => m.AutoMap())
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
```

---

## 4.5 Product Search Service

### IProductSearchService.cs

```csharp
using OpenSearchDemo.Api.Models;

namespace OpenSearchDemo.Api.Services;

/// <summary>
/// Product-specific search operations.
/// </summary>
public interface IProductSearchService
{
    Task<SearchResponse<Product>> SearchProductsAsync(ProductSearchRequest request, CancellationToken ct = default);
    Task<List<AutocompleteSuggestion>> GetAutocompleteAsync(string query, int limit = 5, CancellationToken ct = default);
    Task<bool> IndexProductAsync(Product product, CancellationToken ct = default);
    Task<int> BulkIndexProductsAsync(IEnumerable<Product> products, CancellationToken ct = default);
    Task<bool> DeleteProductAsync(string productId, CancellationToken ct = default);
    Task<Product?> GetProductByIdAsync(string productId, CancellationToken ct = default);
}
```

### ProductSearchService.cs

```csharp
using OpenSearch.Client;
using OpenSearchDemo.Api.Models;

namespace OpenSearchDemo.Api.Services;

/// <summary>
/// Product search service implementation.
/// </summary>
public class ProductSearchService : IProductSearchService
{
    private readonly IOpenSearchService _openSearchService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductSearchService> _logger;
    private readonly string _indexName;

    public ProductSearchService(
        IOpenSearchService openSearchService,
        IConfiguration configuration,
        ILogger<ProductSearchService> logger)
    {
        _openSearchService = openSearchService;
        _configuration = configuration;
        _logger = logger;
        _indexName = configuration["OpenSearch:DefaultIndex"] ?? "products";
    }

    public async Task<SearchResponse<Product>> SearchProductsAsync(ProductSearchRequest request, CancellationToken ct = default)
    {
        var from = (request.Page - 1) * request.PageSize;

        var response = await _openSearchService.SearchAsync<Product>(_indexName, s => s
            .From(from)
            .Size(request.PageSize)
            .Query(q => BuildQuery(q, request))
            .Highlight(h => h
                .PreTags("<mark>")
                .PostTags("</mark>")
                .Fields(
                    f => f.Field(p => p.Name),
                    f => f.Field(p => p.Description).FragmentSize(150).NumberOfFragments(3)
                )
            )
            .Sort(BuildSort(request))
            .Aggregations(a => a
                .Terms("categories", t => t.Field(p => p.Category).Size(20))
                .Terms("brands", t => t.Field(p => p.Brand).Size(20))
                .Terms("tags", t => t.Field(p => p.Tags).Size(30))
                .Range("price_ranges", r => r
                    .Field(p => p.Price)
                    .Ranges(
                        rr => rr.To(50).Key("Under $50"),
                        rr => rr.From(50).To(100).Key("$50-$100"),
                        rr => rr.From(100).To(200).Key("$100-$200"),
                        rr => rr.From(200).To(500).Key("$200-$500"),
                        rr => rr.From(500).Key("$500+")
                    )
                )
            )
        , ct);

        if (!response.IsValid)
        {
            _logger.LogError("Search failed: {Error}", response.OriginalException?.Message);
            return new SearchResponse<Product>();
        }

        return new SearchResponse<Product>
        {
            Hits = response.Hits.Select(h => new SearchHit<Product>
            {
                Document = h.Source,
                Score = h.Score ?? 0,
                Highlights = h.Highlight?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToList()
                )
            }).ToList(),
            TotalCount = response.Total,
            Page = request.Page,
            PageSize = request.PageSize,
            Took = response.Took,
            Facets = BuildFacets(response.Aggregations)
        };
    }

    private QueryContainer BuildQuery(QueryContainerDescriptor<Product> q, ProductSearchRequest request)
    {
        var queries = new List<QueryContainer>();
        var filters = new List<QueryContainer>();

        // Full-text search
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queries.Add(q.MultiMatch(mm => mm
                .Query(request.Query)
                .Fields(f => f
                    .Field(p => p.Name, 3.0)
                    .Field(p => p.Description)
                    .Field(p => p.Tags, 2.0)
                    .Field(p => p.Brand, 1.5)
                )
                .Type(TextQueryType.BestFields)
                .Fuzziness(Fuzziness.Auto)
            ));
        }

        // Category filter
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            filters.Add(q.Term(t => t.Field(p => p.Category).Value(request.Category)));
        }

        // Subcategory filter
        if (!string.IsNullOrWhiteSpace(request.Subcategory))
        {
            filters.Add(q.Term(t => t.Field(p => p.Subcategory).Value(request.Subcategory)));
        }

        // Brand filter
        if (!string.IsNullOrWhiteSpace(request.Brand))
        {
            filters.Add(q.Term(t => t.Field(p => p.Brand).Value(request.Brand)));
        }

        // Price range filter
        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            filters.Add(q.Range(r =>
            {
                var range = r.Field(p => p.Price);
                if (request.MinPrice.HasValue) range.GreaterThanOrEquals((double)request.MinPrice.Value);
                if (request.MaxPrice.HasValue) range.LessThanOrEquals((double)request.MaxPrice.Value);
                return range;
            }));
        }

        // Rating filter
        if (request.MinRating.HasValue)
        {
            filters.Add(q.Range(r => r.Field(p => p.Rating).GreaterThanOrEquals(request.MinRating.Value)));
        }

        // Tags filter
        if (request.Tags?.Any() == true)
        {
            filters.Add(q.Terms(t => t.Field(p => p.Tags).Terms(request.Tags)));
        }

        // In stock filter
        if (request.InStock == true)
        {
            filters.Add(q.Range(r => r.Field(p => p.Stock).GreaterThan(0)));
        }

        // Always filter active products
        filters.Add(q.Term(t => t.Field(p => p.IsActive).Value(true)));

        // Build bool query
        return q.Bool(b =>
        {
            if (queries.Any())
            {
                b.Must(queries.ToArray());
            }
            else
            {
                b.Must(m => m.MatchAll());
            }

            if (filters.Any())
            {
                b.Filter(filters.ToArray());
            }

            return b;
        });
    }

    private Func<SortDescriptor<Product>, IPromise<IList<ISort>>> BuildSort(ProductSearchRequest request)
    {
        return s =>
        {
            switch (request.SortBy.ToLower())
            {
                case "price":
                    return request.SortOrder.ToLower() == "asc"
                        ? s.Ascending(p => p.Price)
                        : s.Descending(p => p.Price);
                case "rating":
                    return s.Descending(p => p.Rating);
                case "name":
                    return request.SortOrder.ToLower() == "asc"
                        ? s.Ascending(p => p.Name.Suffix("keyword"))
                        : s.Descending(p => p.Name.Suffix("keyword"));
                case "newest":
                    return s.Descending(p => p.CreatedAt);
                default: // relevance
                    return s.Descending(SortSpecialField.Score).Descending(p => p.Rating);
            }
        };
    }

    private Dictionary<string, List<FacetBucket>>? BuildFacets(AggregateDictionary aggregations)
    {
        if (aggregations == null) return null;

        var facets = new Dictionary<string, List<FacetBucket>>();

        if (aggregations.TryGetValue("categories", out var categoriesAgg) && categoriesAgg is BucketAggregate catBuckets)
        {
            facets["categories"] = catBuckets.Items
                .OfType<KeyedBucket<object>>()
                .Select(b => new FacetBucket { Key = b.Key.ToString()!, Count = b.DocCount ?? 0 })
                .ToList();
        }

        if (aggregations.TryGetValue("brands", out var brandsAgg) && brandsAgg is BucketAggregate brandBuckets)
        {
            facets["brands"] = brandBuckets.Items
                .OfType<KeyedBucket<object>>()
                .Select(b => new FacetBucket { Key = b.Key.ToString()!, Count = b.DocCount ?? 0 })
                .ToList();
        }

        if (aggregations.TryGetValue("tags", out var tagsAgg) && tagsAgg is BucketAggregate tagBuckets)
        {
            facets["tags"] = tagBuckets.Items
                .OfType<KeyedBucket<object>>()
                .Select(b => new FacetBucket { Key = b.Key.ToString()!, Count = b.DocCount ?? 0 })
                .ToList();
        }

        if (aggregations.TryGetValue("price_ranges", out var priceAgg) && priceAgg is BucketAggregate priceBuckets)
        {
            facets["priceRanges"] = priceBuckets.Items
                .OfType<RangeBucket>()
                .Select(b => new FacetBucket { Key = b.Key, Count = b.DocCount ?? 0 })
                .ToList();
        }

        return facets;
    }

    public async Task<List<AutocompleteSuggestion>> GetAutocompleteAsync(string query, int limit = 5, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return [];
        }

        var response = await _openSearchService.SearchAsync<Product>(_indexName, s => s
            .Size(limit)
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .MultiMatch(mm => mm
                            .Query(query)
                            .Fields(f => f
                                .Field(p => p.Name, 2.0)
                                .Field(p => p.Brand)
                            )
                            .Type(TextQueryType.PhrasePrefix)
                        )
                    )
                    .Filter(f => f.Term(t => t.Field(p => p.IsActive).Value(true)))
                )
            )
            .Source(src => src.Includes(i => i.Fields(p => p.Name, p => p.Category)))
        , ct);

        return response.Documents.Select(d => new AutocompleteSuggestion
        {
            Text = d.Name,
            Category = d.Category
        }).ToList();
    }

    public async Task<bool> IndexProductAsync(Product product, CancellationToken ct = default)
    {
        return await _openSearchService.IndexDocumentAsync(_indexName, product, product.Id, ct);
    }

    public async Task<int> BulkIndexProductsAsync(IEnumerable<Product> products, CancellationToken ct = default)
    {
        var response = await _openSearchService.BulkIndexAsync(_indexName, products, p => p.Id, ct);
        return response.Items.Count(i => !i.IsValid);
    }

    public async Task<bool> DeleteProductAsync(string productId, CancellationToken ct = default)
    {
        return await _openSearchService.DeleteDocumentAsync(_indexName, productId, ct);
    }

    public async Task<Product?> GetProductByIdAsync(string productId, CancellationToken ct = default)
    {
        return await _openSearchService.GetDocumentAsync<Product>(_indexName, productId, ct);
    }
}
```

---

## 4.6 Controllers

### SearchController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using OpenSearchDemo.Api.Models;
using OpenSearchDemo.Api.Services;

namespace OpenSearchDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IProductSearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(IProductSearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// Search products with filters, pagination, and facets.
    /// </summary>
    [HttpGet("products")]
    public async Task<ActionResult<SearchResponse<Product>>> SearchProducts([FromQuery] ProductSearchRequest request, CancellationToken ct)
    {
        try
        {
            var results = await _searchService.SearchProductsAsync(request, ct);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search failed for query: {Query}", request.Query);
            return StatusCode(500, "Search failed");
        }
    }

    /// <summary>
    /// Get autocomplete suggestions.
    /// </summary>
    [HttpGet("autocomplete")]
    public async Task<ActionResult<List<AutocompleteSuggestion>>> Autocomplete([FromQuery] string query, [FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var suggestions = await _searchService.GetAutocompleteAsync(query, limit, ct);
        return Ok(suggestions);
    }
}
```

### ProductsController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using OpenSearchDemo.Api.Models;
using OpenSearchDemo.Api.Services;

namespace OpenSearchDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductSearchService _searchService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductSearchService searchService, ILogger<ProductsController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// Get a product by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(string id, CancellationToken ct)
    {
        var product = await _searchService.GetProductByIdAsync(id, ct);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    /// <summary>
    /// Create or update a product.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateProduct([FromBody] Product product, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(product.Id))
        {
            product.Id = Guid.NewGuid().ToString();
        }
        
        product.CreatedAt = DateTime.UtcNow;
        
        var success = await _searchService.IndexProductAsync(product, ct);
        if (!success)
        {
            return StatusCode(500, "Failed to index product");
        }
        
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// Delete a product.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(string id, CancellationToken ct)
    {
        var success = await _searchService.DeleteProductAsync(id, ct);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Bulk index products.
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult> BulkIndex([FromBody] List<Product> products, CancellationToken ct)
    {
        var failedCount = await _searchService.BulkIndexProductsAsync(products, ct);
        return Ok(new { Indexed = products.Count - failedCount, Failed = failedCount });
    }
}
```

### IndexController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using OpenSearchDemo.Api.Models;
using OpenSearchDemo.Api.Services;

namespace OpenSearchDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IndexController : ControllerBase
{
    private readonly IOpenSearchService _openSearchService;
    private readonly IProductSearchService _productSearchService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IndexController> _logger;

    public IndexController(
        IOpenSearchService openSearchService,
        IProductSearchService productSearchService,
        IConfiguration configuration,
        ILogger<IndexController> logger)
    {
        _openSearchService = openSearchService;
        _productSearchService = productSearchService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Check if the products index exists.
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult> GetIndexStatus(CancellationToken ct)
    {
        var indexName = _configuration["OpenSearch:DefaultIndex"] ?? "products";
        var exists = await _openSearchService.IndexExistsAsync(indexName, ct);
        return Ok(new { IndexName = indexName, Exists = exists });
    }

    /// <summary>
    /// Create the products index with mapping.
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult> CreateIndex(CancellationToken ct)
    {
        var indexName = _configuration["OpenSearch:DefaultIndex"] ?? "products";
        var success = await _openSearchService.CreateIndexAsync<Product>(indexName, ct);
        if (!success)
        {
            return StatusCode(500, "Failed to create index");
        }
        return Ok(new { Message = $"Index '{indexName}' created successfully" });
    }

    /// <summary>
    /// Delete the products index.
    /// </summary>
    [HttpDelete("delete")]
    public async Task<ActionResult> DeleteIndex(CancellationToken ct)
    {
        var indexName = _configuration["OpenSearch:DefaultIndex"] ?? "products";
        var success = await _openSearchService.DeleteIndexAsync(indexName, ct);
        if (!success)
        {
            return StatusCode(500, "Failed to delete index");
        }
        return Ok(new { Message = $"Index '{indexName}' deleted successfully" });
    }

    /// <summary>
    /// Seed sample products for testing.
    /// </summary>
    [HttpPost("seed")]
    public async Task<ActionResult> SeedData(CancellationToken ct)
    {
        var products = GenerateSampleProducts();
        var failedCount = await _productSearchService.BulkIndexProductsAsync(products, ct);
        return Ok(new { Message = $"Seeded {products.Count - failedCount} products", Failed = failedCount });
    }

    private List<Product> GenerateSampleProducts()
    {
        return new List<Product>
        {
            new() { Id = "PROD001", Name = "Wireless Bluetooth Headphones", Description = "Premium noise-canceling wireless headphones with 30-hour battery life and comfortable ear cushions", Category = "Electronics", Subcategory = "Audio", Price = 149.99m, Brand = "TechAudio", Tags = ["wireless", "bluetooth", "noise-canceling"], Rating = 4.5, Stock = 250, IsActive = true },
            new() { Id = "PROD002", Name = "Ergonomic Office Chair", Description = "Adjustable lumbar support ergonomic chair perfect for home office with breathable mesh back", Category = "Furniture", Subcategory = "Chairs", Price = 299.99m, Brand = "ComfortPlus", Tags = ["ergonomic", "office", "adjustable"], Rating = 4.2, Stock = 50, IsActive = true },
            new() { Id = "PROD003", Name = "Mechanical Gaming Keyboard", Description = "RGB mechanical gaming keyboard with Cherry MX switches and programmable macros", Category = "Electronics", Subcategory = "Peripherals", Price = 129.99m, Brand = "GameTech", Tags = ["mechanical", "rgb", "gaming"], Rating = 4.7, Stock = 100, IsActive = true },
            new() { Id = "PROD004", Name = "Standing Desk", Description = "Electric height adjustable standing desk 60x30 inches with memory presets", Category = "Furniture", Subcategory = "Desks", Price = 449.99m, Brand = "ErgoDesk", Tags = ["standing", "electric", "adjustable"], Rating = 4.4, Stock = 25, IsActive = true },
            new() { Id = "PROD005", Name = "Laptop Stand", Description = "Aluminum adjustable laptop stand for MacBook and laptops up to 17 inches", Category = "Electronics", Subcategory = "Accessories", Price = 49.99m, Brand = "TechGear", Tags = ["aluminum", "adjustable", "portable"], Rating = 4.3, Stock = 200, IsActive = true },
            new() { Id = "PROD006", Name = "Noise Canceling Earbuds", Description = "True wireless earbuds with active noise cancellation and wireless charging case", Category = "Electronics", Subcategory = "Audio", Price = 199.99m, Brand = "TechAudio", Tags = ["wireless", "noise-canceling", "earbuds"], Rating = 4.6, Stock = 150, IsActive = true },
            new() { Id = "PROD007", Name = "4K Webcam", Description = "Ultra HD 4K webcam with auto-focus and noise-reducing microphone for streaming", Category = "Electronics", Subcategory = "Peripherals", Price = 179.99m, Brand = "StreamPro", Tags = ["4k", "webcam", "streaming"], Rating = 4.4, Stock = 75, IsActive = true },
            new() { Id = "PROD008", Name = "Monitor Arm", Description = "Heavy-duty dual monitor arm with full motion articulation and cable management", Category = "Furniture", Subcategory = "Accessories", Price = 89.99m, Brand = "ErgoDesk", Tags = ["monitor", "dual", "adjustable"], Rating = 4.5, Stock = 120, IsActive = true },
            new() { Id = "PROD009", Name = "Wireless Mouse", Description = "Ergonomic wireless mouse with silent clicks and 18-month battery life", Category = "Electronics", Subcategory = "Peripherals", Price = 39.99m, Brand = "TechGear", Tags = ["wireless", "ergonomic", "silent"], Rating = 4.1, Stock = 300, IsActive = true },
            new() { Id = "PROD010", Name = "USB-C Hub", Description = "12-in-1 USB-C hub with HDMI, SD card reader, Ethernet, and PD charging", Category = "Electronics", Subcategory = "Accessories", Price = 79.99m, Brand = "TechGear", Tags = ["usb-c", "hub", "portable"], Rating = 4.3, Stock = 180, IsActive = true }
        };
    }
}
```

---

## 4.7 Program.cs

```csharp
using OpenSearch.Client;
using OpenSearchDemo.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────────────────
// OpenSearch Client Configuration
// ─────────────────────────────────────────────────────────────────────────────
var openSearchUri = builder.Configuration["OpenSearch:Uri"] ?? "http://localhost:9200";
var settings = new ConnectionSettings(new Uri(openSearchUri))
    .DefaultIndex(builder.Configuration["OpenSearch:DefaultIndex"] ?? "products")
    .EnableDebugMode()
    .PrettyJson()
    .RequestTimeout(TimeSpan.FromSeconds(30));

// Add authentication if configured
var username = builder.Configuration["OpenSearch:Username"];
var password = builder.Configuration["OpenSearch:Password"];
if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
{
    settings.BasicAuthentication(username, password);
}

builder.Services.AddSingleton<IOpenSearchClient>(new OpenSearchClient(settings));

// ─────────────────────────────────────────────────────────────────────────────
// Services Registration
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IOpenSearchService, OpenSearchService>();
builder.Services.AddSingleton<IProductSearchService, ProductSearchService>();

// ─────────────────────────────────────────────────────────────────────────────
// CORS Configuration
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowedOrigins"] ?? "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ─────────────────────────────────────────────────────────────────────────────
// Middleware Pipeline
// ─────────────────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapScalarApiReference();
}

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 4.8 Testing the API

### Start the Services

```powershell
# Terminal 1: Start OpenSearch
cd OpenSearch-setup
docker-compose up -d

# Terminal 2: Start .NET API
cd backend/OpenSearchDemo.Api
dotnet run
```

### Test Endpoints

```bash
# Check index status
curl http://localhost:5000/api/index/status

# Create index
curl -X POST http://localhost:5000/api/index/create

# Seed sample data
curl -X POST http://localhost:5000/api/index/seed

# Search products
curl "http://localhost:5000/api/search/products?query=wireless"

# Search with filters
curl "http://localhost:5000/api/search/products?query=headphones&category=Electronics&maxPrice=200"

# Autocomplete
curl "http://localhost:5000/api/search/autocomplete?query=wire"
```

---

## 4.9 Checkpoint Questions

1. ✅ How do you configure the OpenSearch client in .NET?
2. ✅ What's the difference between `must` and `filter` in the bool query?
3. ✅ How does field boosting work in multi_match queries?
4. ✅ What are aggregations used for?
5. ✅ How do you implement pagination?

---

## Next Steps

✅ **Module 4 Complete!**

👉 Continue to [Module 5: Angular Integration](./05-angular-integration.md) to build the frontend UI.
