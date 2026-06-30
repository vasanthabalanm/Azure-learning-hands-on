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
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetProduct(string id, CancellationToken ct)
    {
        var product = await _searchService.GetProductByIdAsync(id, ct);
        if (product == null)
        {
            return NotFound(new { Error = $"Product '{id}' not found" });
        }
        return Ok(product);
    }

    /// <summary>
    /// Create or update a product.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateProduct([FromBody] Product product, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(product.Id))
        {
            product.Id = $"PROD{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
        
        product.CreatedAt = DateTime.UtcNow;
        
        var success = await _searchService.IndexProductAsync(product, ct);
        if (!success)
        {
            return StatusCode(500, new { Error = "Failed to index product" });
        }
        
        _logger.LogInformation("Created product {Id}: {Name}", product.Id, product.Name);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// Update an existing product.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateProduct(string id, [FromBody] Product product, CancellationToken ct)
    {
        product.Id = id;
        product.UpdatedAt = DateTime.UtcNow;
        
        var success = await _searchService.IndexProductAsync(product, ct);
        if (!success)
        {
            return StatusCode(500, new { Error = "Failed to update product" });
        }
        
        _logger.LogInformation("Updated product {Id}", id);
        return Ok(product);
    }

    /// <summary>
    /// Delete a product.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProduct(string id, CancellationToken ct)
    {
        var success = await _searchService.DeleteProductAsync(id, ct);
        if (!success)
        {
            return NotFound(new { Error = $"Product '{id}' not found" });
        }
        
        _logger.LogInformation("Deleted product {Id}", id);
        return NoContent();
    }

    /// <summary>
    /// Bulk index multiple products.
    /// </summary>
    [HttpPost("bulk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> BulkIndex([FromBody] List<Product> products, CancellationToken ct)
    {
        foreach (var product in products.Where(p => string.IsNullOrEmpty(p.Id)))
        {
            product.Id = $"PROD{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        var failedCount = await _searchService.BulkIndexProductsAsync(products, ct);
        
        _logger.LogInformation("Bulk indexed {Count} products, {Failed} failed", products.Count, failedCount);
        return Ok(new { Indexed = products.Count - failedCount, Failed = failedCount });
    }
}
