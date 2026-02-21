namespace SkinPAI.API.Models.DTOs;

// ==================== Product DTOs ====================
public record ProductDto(
    Guid ProductId,
    string ProductName,
    string? Description,
    string? ProductImageUrl,
    decimal Price,
    decimal? OriginalPrice,
    int? DiscountPercent,
    decimal? AverageRating,
    int TotalReviews,
    bool InStock,
    bool IsFeatured,
    bool IsRecommended,
    string? Volume,
    string? ShopUrl,
    string[]? KeyIngredients,
    string[]? SkinTypes,
    string[]? SkinConcerns,
    BrandDto Brand,
    ProductCategoryDto Category,
    DistributorDto? Distributor
);

public record ProductRecommendationDto(
    Guid ProductId,
    string ProductName,
    string? ProductImageUrl,
    decimal Price,
    decimal RecommendationScore,
    string? RecommendationReason,
    string BrandName
);

public record BrandDto(
    Guid BrandId,
    string BrandName,
    string? LogoUrl,
    string? Description,
    string? Website,
    bool IsVerified,
    bool IsPartner
);

public record DistributorDto(
    Guid DistributorId,
    string Name,
    string? LogoUrl,
    string? Website,
    bool IsPartner
);

public record ProductCategoryDto(
    Guid CategoryId,
    string CategoryName,
    string? Description,
    string? CategoryIcon
);

public record ProductBundleDto(
    Guid BundleId,
    string Name,
    string? Description,
    string? ImageUrl,
    decimal BundlePrice,
    decimal OriginalPrice,
    decimal Savings,
    string? Category,
    string[]? Benefits,
    string[]? ForSkinTypes,
    string[]? ForSkinConcerns,
    BrandDto Brand,
    List<ProductSummaryDto> Products
);

public record ProductFilterRequest(
    string? SearchTerm = null,
    Guid[]? BrandIds = null,
    Guid[]? CategoryIds = null,
    string[]? SkinTypes = null,
    string[]? SkinConcerns = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? InStock = null,
    bool? OnSale = null,
    string? SortBy = null,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 20
);

public record PaginatedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
