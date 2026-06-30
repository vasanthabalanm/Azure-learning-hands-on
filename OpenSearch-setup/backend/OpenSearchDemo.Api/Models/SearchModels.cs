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
