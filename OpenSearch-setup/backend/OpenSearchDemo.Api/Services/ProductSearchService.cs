using OpenSearch.Client;
using OpenSearchDemo.Api.Models;
using SearchResult = OpenSearchDemo.Api.Models.SearchResponse<OpenSearchDemo.Api.Models.Product>;

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

    public async Task<Models.SearchResponse<Product>> SearchProductsAsync(ProductSearchRequest request, CancellationToken ct = default)
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
            return new Models.SearchResponse<Product>();
        }

        return new Models.SearchResponse<Product>
        {
            Hits = response.Hits.Select(h => new Models.SearchHit<Product>
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
        var mustQueries = new List<QueryContainer>();
        var filterQueries = new List<QueryContainer>();
        var shouldQueries = new List<QueryContainer>();

        // Full-text search
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            mustQueries.Add(q.MultiMatch(mm => mm
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
            filterQueries.Add(q.Term(t => t.Field(p => p.Category).Value(request.Category)));
        }

        // Subcategory filter
        if (!string.IsNullOrWhiteSpace(request.Subcategory))
        {
            filterQueries.Add(q.Term(t => t.Field(p => p.Subcategory).Value(request.Subcategory)));
        }

        // Brand filter
        if (!string.IsNullOrWhiteSpace(request.Brand))
        {
            filterQueries.Add(q.Term(t => t.Field(p => p.Brand).Value(request.Brand)));
        }

        // Price range filter
        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            filterQueries.Add(q.Range(r =>
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
            filterQueries.Add(q.Range(r => r.Field(p => p.Rating).GreaterThanOrEquals(request.MinRating.Value)));
        }

        // Tags filter
        if (request.Tags?.Any() == true)
        {
            filterQueries.Add(q.Terms(t => t.Field(p => p.Tags).Terms(request.Tags)));
        }

        // In stock filter
        if (request.InStock == true)
        {
            filterQueries.Add(q.Range(r => r.Field(p => p.Stock).GreaterThan(0)));
        }

        // Always filter active products
        filterQueries.Add(q.Term(t => t.Field(p => p.IsActive).Value(true)));

        // Build bool query
        return q.Bool(b =>
        {
            if (mustQueries.Any())
            {
                b.Must(mustQueries.ToArray());
            }
            else
            {
                b.Must(m => m.MatchAll());
            }

            if (filterQueries.Any())
            {
                b.Filter(filterQueries.ToArray());
            }

            if (shouldQueries.Any())
            {
                b.Should(shouldQueries.ToArray());
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

    private Dictionary<string, List<FacetBucket>>? BuildFacets(AggregateDictionary? aggregations)
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
                .Select(b => new FacetBucket { Key = b.Key, Count = (int)b.DocCount })
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
