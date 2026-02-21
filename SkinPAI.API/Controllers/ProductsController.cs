using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Services;
using System.Security.Claims;

namespace SkinPAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
    }

    /// <summary>
    /// Get products with filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<ProductDto>>> GetProducts([FromQuery] ProductFilterRequest filter)
    {
        _logger.LogDebug("🛍️ PRODUCTS GET: Fetching products | Page: {Page} | Search: {Search}", 
            filter.Page, filter.SearchTerm ?? "none");
        
        var products = await _productService.GetProductsAsync(filter);
        _logger.LogInformation("🛍️ PRODUCTS GET: Retrieved {Count} products | Total: {Total}", 
            products.Items.Count, products.TotalCount);
        return Ok(products);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{productId}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid productId)
    {
        _logger.LogDebug("🛍️ PRODUCT GET: Fetching product | ProductId: {ProductId}", productId);
        
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            _logger.LogWarning("⚠️ PRODUCT NOT FOUND: Product does not exist | ProductId: {ProductId}", productId);
            return NotFound();
        }
        _logger.LogInformation("🛍️ PRODUCT GET: Retrieved | ProductId: {ProductId} | Name: {ProductName}", 
            productId, product.ProductName);
        return Ok(product);
    }

    /// <summary>
    /// Get all product categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<ProductCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductCategoryDto>>> GetCategories()
    {
        _logger.LogDebug("📁 CATEGORIES GET: Fetching all categories");
        var categories = await _productService.GetCategoriesAsync();
        _logger.LogDebug("📁 CATEGORIES GET: Retrieved {Count} categories", categories.Count);
        return Ok(categories);
    }

    /// <summary>
    /// Get all brands
    /// </summary>
    [HttpGet("brands")]
    [ProducesResponseType(typeof(List<BrandDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BrandDto>>> GetBrands()
    {
        _logger.LogDebug("🏷️ BRANDS GET: Fetching all brands");
        var brands = await _productService.GetBrandsAsync();
        _logger.LogDebug("🏷️ BRANDS GET: Retrieved {Count} brands", brands.Count);
        return Ok(brands);
    }

    /// <summary>
    /// Get all distributors
    /// </summary>
    [HttpGet("distributors")]
    [ProducesResponseType(typeof(List<DistributorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DistributorDto>>> GetDistributors()
    {
        _logger.LogDebug("🏪 DISTRIBUTORS GET: Fetching all distributors");
        var distributors = await _productService.GetDistributorsAsync();
        _logger.LogDebug("🏪 DISTRIBUTORS GET: Retrieved {Count} distributors", distributors.Count);
        return Ok(distributors);
    }

    /// <summary>
    /// Get user's favorite products
    /// </summary>
    [Authorize]
    [HttpGet("favorites")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductDto>>> GetFavorites()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        _logger.LogDebug("⭐ FAVORITES GET: Fetching favorites | UserId: {UserId}", userId);
        var favorites = await _productService.GetUserFavoritesAsync(userId.Value);
        _logger.LogInformation("⭐ FAVORITES GET: Retrieved {Count} favorites | UserId: {UserId}", favorites.Count, userId);
        return Ok(favorites);
    }

    /// <summary>
    /// Add product to favorites
    /// </summary>
    [Authorize]
    [HttpPost("favorites/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> AddToFavorites(Guid productId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        _logger.LogInformation("⭐ FAVORITE ADD: Adding to favorites | UserId: {UserId} | ProductId: {ProductId}", userId, productId);
        var added = await _productService.AddToFavoritesAsync(userId.Value, productId);
        _logger.LogInformation("⭐ FAVORITE ADD: {Result} | UserId: {UserId} | ProductId: {ProductId}", 
            added ? "Added" : "Already favorited", userId, productId);
        return Ok(new { success = added, message = added ? "Added to favorites" : "Already in favorites" });
    }

    /// <summary>
    /// Remove product from favorites
    /// </summary>
    [Authorize]
    [HttpDelete("favorites/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RemoveFromFavorites(Guid productId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        _logger.LogInformation("⭐ FAVORITE REMOVE: Removing from favorites | UserId: {UserId} | ProductId: {ProductId}", userId, productId);
        var removed = await _productService.RemoveFromFavoritesAsync(userId.Value, productId);
        _logger.LogInformation("⭐ FAVORITE REMOVE: {Result} | UserId: {UserId} | ProductId: {ProductId}", 
            removed ? "Removed" : "Not in favorites", userId, productId);
        return Ok(new { success = removed, message = removed ? "Removed from favorites" : "Not in favorites" });
    }

    /// <summary>
    /// Get all product bundles
    /// </summary>
    [HttpGet("bundles")]
    [ProducesResponseType(typeof(List<ProductBundleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductBundleDto>>> GetBundles()
    {
        _logger.LogDebug("📦 BUNDLES GET: Fetching all bundles");
        var bundles = await _productService.GetBundlesAsync();
        _logger.LogDebug("📦 BUNDLES GET: Retrieved {Count} bundles", bundles.Count);
        return Ok(bundles);
    }

    /// <summary>
    /// Get bundle by ID
    /// </summary>
    [HttpGet("bundles/{bundleId}")]
    [ProducesResponseType(typeof(ProductBundleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductBundleDto>> GetBundle(Guid bundleId)
    {
        _logger.LogDebug("📦 BUNDLE GET: Fetching bundle | BundleId: {BundleId}", bundleId);
        
        var bundle = await _productService.GetBundleByIdAsync(bundleId);
        if (bundle == null)
        {
            _logger.LogWarning("⚠️ BUNDLE NOT FOUND: Bundle does not exist | BundleId: {BundleId}", bundleId);
            return NotFound();
        }
        return Ok(bundle);
    }

    /// <summary>
    /// Get product recommendations based on scan
    /// </summary>
    [Authorize]
    [HttpGet("recommendations/scan/{scanId}")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductDto>>> GetRecommendationsForScan(Guid scanId)
    {
        var userId = GetUserId();
        _logger.LogInformation("💡 RECOMMENDATIONS GET: Fetching scan-based recommendations | UserId: {UserId} | ScanId: {ScanId}", 
            userId, scanId);
        
        var recommendations = await _productService.GetRecommendationsForScanAsync(scanId);
        _logger.LogInformation("💡 RECOMMENDATIONS GET: Retrieved {Count} recommendations | ScanId: {ScanId}", 
            recommendations.Count, scanId);
        return Ok(recommendations);
    }

    /// <summary>
    /// Get personalized recommendations for user
    /// </summary>
    [Authorize]
    [HttpGet("recommendations")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductDto>>> GetRecommendations()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        _logger.LogInformation("💡 RECOMMENDATIONS GET: Fetching personalized recommendations | UserId: {UserId}", userId);
        var recommendations = await _productService.GetPersonalizedRecommendationsAsync(userId.Value);
        _logger.LogInformation("💡 RECOMMENDATIONS GET: Retrieved {Count} personalized recommendations | UserId: {UserId}", 
            recommendations.Count, userId);
        return Ok(recommendations);
    }

    /// <summary>
    /// Search products by keyword
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<ProductDto>>> SearchProducts([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("🔍 PRODUCT SEARCH: Searching | Query: '{Query}' | Page: {Page}", query, page);
        
        var filter = new ProductFilterRequest(SearchTerm: query, Page: page, PageSize: pageSize);
        var products = await _productService.GetProductsAsync(filter);
        _logger.LogInformation("🔍 PRODUCT SEARCH: Found {Count} results | Query: '{Query}'", 
            products.TotalCount, query);
        return Ok(products);
    }
}
