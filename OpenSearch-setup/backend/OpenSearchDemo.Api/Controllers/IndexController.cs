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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateIndex(CancellationToken ct)
    {
        var indexName = _configuration["OpenSearch:DefaultIndex"] ?? "products";
        var success = await _openSearchService.CreateIndexAsync<Product>(indexName, ct);
        
        if (!success)
        {
            return StatusCode(500, new { Error = "Failed to create index" });
        }
        
        _logger.LogInformation("Created index {IndexName}", indexName);
        return Ok(new { Message = $"Index '{indexName}' created successfully" });
    }

    /// <summary>
    /// Delete the products index.
    /// </summary>
    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteIndex(CancellationToken ct)
    {
        var indexName = _configuration["OpenSearch:DefaultIndex"] ?? "products";
        var success = await _openSearchService.DeleteIndexAsync(indexName, ct);
        
        if (!success)
        {
            return StatusCode(500, new { Error = "Failed to delete index" });
        }
        
        _logger.LogInformation("Deleted index {IndexName}", indexName);
        return Ok(new { Message = $"Index '{indexName}' deleted successfully" });
    }

    /// <summary>
    /// Seed sample products for testing.
    /// </summary>
    [HttpPost("seed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> SeedData(CancellationToken ct)
    {
        var products = GenerateSampleProducts();
        var failedCount = await _productSearchService.BulkIndexProductsAsync(products, ct);
        
        _logger.LogInformation("Seeded {Count} products", products.Count - failedCount);
        return Ok(new { 
            Message = $"Seeded {products.Count - failedCount} products", 
            Total = products.Count,
            Failed = failedCount 
        });
    }

    /// <summary>
    /// Reset index: delete, recreate, and seed with sample data.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ResetIndex(CancellationToken ct)
    {
        var indexName = _configuration["OpenSearch:DefaultIndex"] ?? "products";
        
        // Delete if exists
        await _openSearchService.DeleteIndexAsync(indexName, ct);
        
        // Create fresh
        var created = await _openSearchService.CreateIndexAsync<Product>(indexName, ct);
        if (!created)
        {
            return StatusCode(500, new { Error = "Failed to create index" });
        }
        
        // Seed data
        var products = GenerateSampleProducts();
        var failedCount = await _productSearchService.BulkIndexProductsAsync(products, ct);
        
        _logger.LogInformation("Reset index {IndexName} with {Count} products", indexName, products.Count - failedCount);
        return Ok(new { 
            Message = $"Index '{indexName}' reset with {products.Count - failedCount} products",
            Total = products.Count,
            Failed = failedCount 
        });
    }

    private static List<Product> GenerateSampleProducts()
    {
        return
        [
            new() { Id = "PROD001", Name = "Wireless Bluetooth Headphones", Description = "Premium noise-canceling wireless headphones with 30-hour battery life and comfortable ear cushions. Features active noise cancellation and crystal-clear audio quality.", Category = "Electronics", Subcategory = "Audio", Price = 149.99m, Brand = "TechAudio", Tags = ["wireless", "bluetooth", "noise-canceling", "premium"], Rating = 4.5, Stock = 250, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-60) },
            new() { Id = "PROD002", Name = "Ergonomic Office Chair", Description = "Adjustable lumbar support ergonomic chair perfect for home office with breathable mesh back. Supports up to 300 lbs with 5-year warranty.", Category = "Furniture", Subcategory = "Chairs", Price = 299.99m, Brand = "ComfortPlus", Tags = ["ergonomic", "office", "adjustable", "mesh"], Rating = 4.2, Stock = 50, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-55) },
            new() { Id = "PROD003", Name = "Mechanical Gaming Keyboard", Description = "RGB mechanical gaming keyboard with Cherry MX switches and programmable macros. Full N-key rollover and dedicated media controls.", Category = "Electronics", Subcategory = "Peripherals", Price = 129.99m, Brand = "GameTech", Tags = ["mechanical", "rgb", "gaming", "cherry-mx"], Rating = 4.7, Stock = 100, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-50) },
            new() { Id = "PROD004", Name = "Electric Standing Desk", Description = "Electric height adjustable standing desk 60x30 inches with memory presets. Quiet dual-motor system with anti-collision technology.", Category = "Furniture", Subcategory = "Desks", Price = 449.99m, Brand = "ErgoDesk", Tags = ["standing", "electric", "adjustable", "motorized"], Rating = 4.4, Stock = 25, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-45) },
            new() { Id = "PROD005", Name = "Aluminum Laptop Stand", Description = "Aluminum adjustable laptop stand for MacBook and laptops up to 17 inches. Improves ergonomics and increases airflow for better cooling.", Category = "Electronics", Subcategory = "Accessories", Price = 49.99m, Brand = "TechGear", Tags = ["aluminum", "adjustable", "portable", "cooling"], Rating = 4.3, Stock = 200, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-40) },
            new() { Id = "PROD006", Name = "True Wireless Earbuds Pro", Description = "True wireless earbuds with active noise cancellation and wireless charging case. IPX4 water resistance with 28-hour total battery life.", Category = "Electronics", Subcategory = "Audio", Price = 199.99m, Brand = "TechAudio", Tags = ["wireless", "noise-canceling", "earbuds", "water-resistant"], Rating = 4.6, Stock = 150, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-35) },
            new() { Id = "PROD007", Name = "4K Streaming Webcam", Description = "Ultra HD 4K webcam with auto-focus and dual noise-reducing microphones for streaming and video conferencing. Works with all major platforms.", Category = "Electronics", Subcategory = "Peripherals", Price = 179.99m, Brand = "StreamPro", Tags = ["4k", "webcam", "streaming", "auto-focus"], Rating = 4.4, Stock = 75, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new() { Id = "PROD008", Name = "Dual Monitor Arm", Description = "Heavy-duty dual monitor arm with full motion articulation and integrated cable management. Supports monitors 13-32 inches up to 20 lbs each.", Category = "Furniture", Subcategory = "Accessories", Price = 89.99m, Brand = "ErgoDesk", Tags = ["monitor", "dual", "adjustable", "cable-management"], Rating = 4.5, Stock = 120, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-25) },
            new() { Id = "PROD009", Name = "Ergonomic Wireless Mouse", Description = "Ergonomic wireless mouse with silent clicks and 18-month battery life. Vertical design reduces wrist strain during extended use.", Category = "Electronics", Subcategory = "Peripherals", Price = 39.99m, Brand = "TechGear", Tags = ["wireless", "ergonomic", "silent", "vertical"], Rating = 4.1, Stock = 300, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new() { Id = "PROD010", Name = "12-in-1 USB-C Hub", Description = "12-in-1 USB-C hub with HDMI 4K output, SD card reader, Gigabit Ethernet, and 100W PD charging. Compatible with all USB-C laptops.", Category = "Electronics", Subcategory = "Accessories", Price = 79.99m, Brand = "TechGear", Tags = ["usb-c", "hub", "portable", "hdmi", "ethernet"], Rating = 4.3, Stock = 180, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new() { Id = "PROD011", Name = "Portable Bluetooth Speaker", Description = "Waterproof portable Bluetooth speaker with 360-degree sound and 24-hour battery life. IPX7 rated for pool and beach use.", Category = "Electronics", Subcategory = "Audio", Price = 79.99m, Brand = "TechAudio", Tags = ["bluetooth", "portable", "waterproof", "speaker"], Rating = 4.4, Stock = 200, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new() { Id = "PROD012", Name = "Gaming Mouse Pad XL", Description = "Extended gaming mouse pad with smooth cloth surface and non-slip rubber base. Perfect for low DPI gaming with ample space.", Category = "Electronics", Subcategory = "Accessories", Price = 24.99m, Brand = "GameTech", Tags = ["gaming", "mousepad", "extended", "cloth"], Rating = 4.2, Stock = 400, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-8) },
            new() { Id = "PROD013", Name = "Mesh Office Chair Basic", Description = "Budget-friendly mesh office chair with adjustable height and armrests. Breathable mesh back keeps you cool during long work sessions.", Category = "Furniture", Subcategory = "Chairs", Price = 149.99m, Brand = "ComfortPlus", Tags = ["mesh", "office", "budget", "adjustable"], Rating = 3.9, Stock = 80, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new() { Id = "PROD014", Name = "Wireless Charging Pad", Description = "Fast wireless charging pad compatible with all Qi-enabled devices. Supports up to 15W fast charging with LED indicator.", Category = "Electronics", Subcategory = "Accessories", Price = 29.99m, Brand = "TechGear", Tags = ["wireless", "charging", "qi", "fast-charging"], Rating = 4.0, Stock = 350, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new() { Id = "PROD015", Name = "Studio Condenser Microphone", Description = "Professional USB condenser microphone for podcasting and streaming. Cardioid pickup pattern with built-in pop filter.", Category = "Electronics", Subcategory = "Audio", Price = 129.99m, Brand = "StreamPro", Tags = ["microphone", "usb", "condenser", "podcast"], Rating = 4.6, Stock = 60, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-1) }
        ];
    }
}
