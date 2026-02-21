using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Models.Entities;
using SkinPAI.API.Repositories;

namespace SkinPAI.API.Services;

public interface IProductService
{
    Task<PaginatedResponse<ProductDto>> GetProductsAsync(ProductFilterRequest filter);
    Task<ProductDto?> GetProductByIdAsync(Guid productId);
    Task<List<ProductCategoryDto>> GetCategoriesAsync();
    Task<List<BrandDto>> GetBrandsAsync();
    Task<List<DistributorDto>> GetDistributorsAsync();
    Task<List<ProductBundleDto>> GetBundlesAsync();
    Task<ProductBundleDto?> GetBundleByIdAsync(Guid bundleId);
    Task<List<ProductDto>> GetUserFavoritesAsync(Guid userId);
    Task<bool> AddToFavoritesAsync(Guid userId, Guid productId);
    Task<bool> RemoveFromFavoritesAsync(Guid userId, Guid productId);
    Task<bool> IsFavoriteAsync(Guid userId, Guid productId);
    Task<List<ProductDto>> GetRecommendationsForScanAsync(Guid scanId);
    Task<List<ProductDto>> GetPersonalizedRecommendationsAsync(Guid userId);
}

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ProductDto>> GetProductsAsync(ProductFilterRequest filter)
    {
        var query = _unitOfWork.Products.Query()
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Distributor)
            .Where(p => p.IsActive);

        // Apply filters
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var searchLower = filter.SearchTerm.ToLower();
            query = query.Where(p => 
                p.ProductName.ToLower().Contains(searchLower) ||
                (p.Description != null && p.Description.ToLower().Contains(searchLower)) ||
                p.Brand.BrandName.ToLower().Contains(searchLower));
        }

        if (filter.BrandIds != null && filter.BrandIds.Any())
        {
            query = query.Where(p => filter.BrandIds.Contains(p.BrandId));
        }

        if (filter.CategoryIds != null && filter.CategoryIds.Any())
        {
            query = query.Where(p => filter.CategoryIds.Contains(p.CategoryId));
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        if (filter.InStock.HasValue)
        {
            query = query.Where(p => p.InStock == filter.InStock.Value);
        }

        if (filter.OnSale.HasValue && filter.OnSale.Value)
        {
            query = query.Where(p => p.OriginalPrice.HasValue && p.Price < p.OriginalPrice);
        }

        // Apply sorting
        query = filter.SortBy?.ToLower() switch
        {
            "price" => filter.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "rating" => filter.SortDescending ? query.OrderByDescending(p => p.AverageRating) : query.OrderBy(p => p.AverageRating),
            "name" => filter.SortDescending ? query.OrderByDescending(p => p.ProductName) : query.OrderBy(p => p.ProductName),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.AverageRating)
        };

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var products = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var productDtos = products.Select(MapToProductDto).ToList();

        return new PaginatedResponse<ProductDto>(
            productDtos,
            totalCount,
            filter.Page,
            filter.PageSize,
            (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        );
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
    {
        var product = await _unitOfWork.Products.Query()
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Distributor)
            .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive);

        return product != null ? MapToProductDto(product) : null;
    }

    public async Task<List<ProductCategoryDto>> GetCategoriesAsync()
    {
        var categories = await _unitOfWork.ProductCategories.Query()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        return categories.Select(c => new ProductCategoryDto(
            c.CategoryId,
            c.CategoryName,
            c.Description,
            c.CategoryIcon
        )).ToList();
    }

    public async Task<List<BrandDto>> GetBrandsAsync()
    {
        var brands = await _unitOfWork.Brands.Query()
            .OrderBy(b => b.BrandName)
            .ToListAsync();

        return brands.Select(b => new BrandDto(
            b.BrandId,
            b.BrandName,
            b.LogoUrl,
            b.Description,
            b.Website,
            b.IsVerified,
            b.IsPartner
        )).ToList();
    }

    public async Task<List<DistributorDto>> GetDistributorsAsync()
    {
        var distributors = await _unitOfWork.Distributors.Query()
            .OrderBy(d => d.Name)
            .ToListAsync();

        return distributors.Select(d => new DistributorDto(
            d.DistributorId,
            d.Name,
            d.LogoUrl,
            d.Website,
            d.IsPartner
        )).ToList();
    }

    public async Task<List<ProductBundleDto>> GetBundlesAsync()
    {
        var bundles = await _unitOfWork.ProductBundles.Query()
            .Include(b => b.Brand)
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return bundles.Select(MapToProductBundleDto).ToList();
    }

    public async Task<ProductBundleDto?> GetBundleByIdAsync(Guid bundleId)
    {
        var bundle = await _unitOfWork.ProductBundles.Query()
            .Include(b => b.Brand)
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.BundleId == bundleId && b.IsActive);

        return bundle != null ? MapToProductBundleDto(bundle) : null;
    }

    public async Task<List<ProductDto>> GetUserFavoritesAsync(Guid userId)
    {
        var favorites = await _unitOfWork.UserProductFavorites.Query()
            .Include(f => f.Product)
                .ThenInclude(p => p.Brand)
            .Include(f => f.Product)
                .ThenInclude(p => p.Category)
            .Include(f => f.Product)
                .ThenInclude(p => p.Distributor)
            .Where(f => f.UserId == userId && f.Product.IsActive)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return favorites.Select(f => MapToProductDto(f.Product)).ToList();
    }

    public async Task<bool> AddToFavoritesAsync(Guid userId, Guid productId)
    {
        var exists = await _unitOfWork.UserProductFavorites
            .AnyAsync(f => f.UserId == userId && f.ProductId == productId);

        if (exists) return false;

        await _unitOfWork.UserProductFavorites.AddAsync(new UserProductFavorite
        {
            UserId = userId,
            ProductId = productId
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromFavoritesAsync(Guid userId, Guid productId)
    {
        var favorite = await _unitOfWork.UserProductFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

        if (favorite == null) return false;

        _unitOfWork.UserProductFavorites.Remove(favorite);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsFavoriteAsync(Guid userId, Guid productId)
    {
        return await _unitOfWork.UserProductFavorites
            .AnyAsync(f => f.UserId == userId && f.ProductId == productId);
    }

    public async Task<List<ProductDto>> GetRecommendationsForScanAsync(Guid scanId)
    {
        var recommendations = await _unitOfWork.ProductRecommendations.Query()
            .Include(r => r.Product)
                .ThenInclude(p => p.Brand)
            .Include(r => r.Product)
                .ThenInclude(p => p.Category)
            .Include(r => r.Product)
                .ThenInclude(p => p.Distributor)
            .Where(r => r.ScanId == scanId)
            .OrderByDescending(r => r.RecommendationScore)
            .ToListAsync();

        return recommendations.Select(r => MapToProductDto(r.Product)).ToList();
    }

    public async Task<List<ProductDto>> GetPersonalizedRecommendationsAsync(Guid userId)
    {
        // Get user's latest skin scan
        var latestScan = await _unitOfWork.SkinScans.Query()
            .Include(s => s.AnalysisResult)
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.ScanDate)
            .FirstOrDefaultAsync();

        if (latestScan == null)
        {
            // Return featured products if no scan
            var featuredProducts = await _unitOfWork.Products.Query()
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Distributor)
                .Where(p => p.IsActive && p.IsFeatured)
                .Take(10)
                .ToListAsync();
            return featuredProducts.Select(MapToProductDto).ToList();
        }

        // Get recommendations for the latest scan
        return await GetRecommendationsForScanAsync(latestScan.ScanId);
    }

    private ProductDto MapToProductDto(Product product)
    {
        return new ProductDto(
            product.ProductId,
            product.ProductName,
            product.Description,
            product.ProductImageUrl,
            product.Price,
            product.OriginalPrice,
            product.DiscountPercent,
            product.AverageRating,
            product.TotalReviews,
            product.InStock,
            product.IsFeatured,
            product.IsRecommended,
            product.Volume,
            product.ShopUrl,
            product.KeyIngredients != null ? JsonSerializer.Deserialize<string[]>(product.KeyIngredients) : null,
            product.SkinTypes != null ? JsonSerializer.Deserialize<string[]>(product.SkinTypes) : null,
            product.SkinConcerns != null ? JsonSerializer.Deserialize<string[]>(product.SkinConcerns) : null,
            new BrandDto(
                product.Brand.BrandId,
                product.Brand.BrandName,
                product.Brand.LogoUrl,
                product.Brand.Description,
                product.Brand.Website,
                product.Brand.IsVerified,
                product.Brand.IsPartner
            ),
            new ProductCategoryDto(
                product.Category.CategoryId,
                product.Category.CategoryName,
                product.Category.Description,
                product.Category.CategoryIcon
            ),
            product.Distributor != null ? new DistributorDto(
                product.Distributor.DistributorId,
                product.Distributor.Name,
                product.Distributor.LogoUrl,
                product.Distributor.Website,
                product.Distributor.IsPartner
            ) : null
        );
    }

    private ProductBundleDto MapToProductBundleDto(ProductBundle bundle)
    {
        return new ProductBundleDto(
            bundle.BundleId,
            bundle.Name,
            bundle.Description,
            bundle.ImageUrl,
            bundle.BundlePrice,
            bundle.OriginalPrice,
            bundle.Savings,
            bundle.Category,
            bundle.Benefits != null ? JsonSerializer.Deserialize<string[]>(bundle.Benefits) : null,
            bundle.ForSkinTypes != null ? JsonSerializer.Deserialize<string[]>(bundle.ForSkinTypes) : null,
            bundle.ForSkinConcerns != null ? JsonSerializer.Deserialize<string[]>(bundle.ForSkinConcerns) : null,
            new BrandDto(
                bundle.Brand.BrandId,
                bundle.Brand.BrandName,
                bundle.Brand.LogoUrl,
                bundle.Brand.Description,
                bundle.Brand.Website,
                bundle.Brand.IsVerified,
                bundle.Brand.IsPartner
            ),
            bundle.BundleItems.OrderBy(bi => bi.DisplayOrder).Select(bi => new ProductSummaryDto(
                bi.Product.ProductId,
                bi.Product.ProductName,
                bi.Product.ProductImageUrl,
                bi.Product.Price,
                bi.Product.OriginalPrice,
                bi.Product.DiscountPercent ?? 0m,
                bi.Product.AverageRating ?? 0m,
                bi.Product.TotalReviews,
                bi.Product.InStock,
                bi.Product.Brand?.BrandName ?? ""
            )).ToList()
        );
    }
}
