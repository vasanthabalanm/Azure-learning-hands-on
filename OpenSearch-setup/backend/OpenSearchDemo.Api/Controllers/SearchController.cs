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
    /// <remarks>
    /// Example queries:
    /// - GET /api/search/products?query=wireless
    /// - GET /api/search/products?query=headphones&amp;category=Electronics&amp;maxPrice=200
    /// - GET /api/search/products?brand=TechAudio&amp;sortBy=price&amp;sortOrder=asc
    /// </remarks>
    [HttpGet("products")]
    [ProducesResponseType(typeof(Models.SearchResponse<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Models.SearchResponse<Product>>> SearchProducts([FromQuery] ProductSearchRequest request, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Searching products with query: {Query}, Category: {Category}, Page: {Page}", 
                request.Query, request.Category, request.Page);
            
            var results = await _searchService.SearchProductsAsync(request, ct);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search failed for query: {Query}", request.Query);
            return StatusCode(500, new { Error = "Search failed", Message = ex.Message });
        }
    }

    /// <summary>
    /// Get autocomplete suggestions for product search.
    /// </summary>
    /// <param name="query">Search query (minimum 2 characters)</param>
    /// <param name="limit">Maximum number of suggestions (default: 5)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("autocomplete")]
    [ProducesResponseType(typeof(List<AutocompleteSuggestion>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AutocompleteSuggestion>>> Autocomplete(
        [FromQuery] string query, 
        [FromQuery] int limit = 5, 
        CancellationToken ct = default)
    {
        var suggestions = await _searchService.GetAutocompleteAsync(query, limit, ct);
        return Ok(suggestions);
    }
}
