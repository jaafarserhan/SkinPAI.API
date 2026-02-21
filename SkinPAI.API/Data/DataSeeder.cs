using Microsoft.EntityFrameworkCore;
using SkinPAI.API.Models.Entities;

namespace SkinPAI.API.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(SkinPAIDbContext context, ILogger logger)
    {
        try
        {
            // Seed Distributors
            if (!await context.Distributors.AnyAsync())
            {
                await SeedDistributorsAsync(context, logger);
            }

            // Seed Brands
            if (!await context.Brands.AnyAsync())
            {
                await SeedBrandsAsync(context, logger);
            }

            // Seed Products (Iraq/Basra market)
            if (!await context.Products.AnyAsync())
            {
                await SeedProductsAsync(context, logger);
            }

            // Seed Product Bundles
            if (!await context.ProductBundles.AnyAsync())
            {
                await SeedProductBundlesAsync(context, logger);
            }

            logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding data");
            throw;
        }
    }

    private static async Task SeedDistributorsAsync(SkinPAIDbContext context, ILogger logger)
    {
        var distributors = new List<Distributor>
        {
            new Distributor
            {
                DistributorId = Guid.Parse("d0000000-0000-0000-0000-000000000001"),
                Name = "Basra Pharmacy - صيدلية البصرة",
                Website = "https://basrapharmacy.iq",
                LogoUrl = "/images/distributors/basra-pharmacy.png",
                IsPartner = true
            },
            new Distributor
            {
                DistributorId = Guid.Parse("d0000000-0000-0000-0000-000000000002"),
                Name = "Baghdad Beauty Center - مركز بغداد للتجميل",
                Website = "https://baghdadbeauty.iq",
                LogoUrl = "/images/distributors/baghdad-beauty.png",
                IsPartner = true
            },
            new Distributor
            {
                DistributorId = Guid.Parse("d0000000-0000-0000-0000-000000000003"),
                Name = "Al-Rasheed Cosmetics - الرشيد للمستحضرات",
                Website = "https://alrasheed.iq",
                LogoUrl = "/images/distributors/alrasheed.png",
                IsPartner = true
            },
            new Distributor
            {
                DistributorId = Guid.Parse("d0000000-0000-0000-0000-000000000004"),
                Name = "Iraqi Derma Store - متجر الجلدية العراقي",
                Website = "https://iraqiderma.iq",
                LogoUrl = "/images/distributors/iraqi-derma.png",
                IsPartner = true
            },
            new Distributor
            {
                DistributorId = Guid.Parse("d0000000-0000-0000-0000-000000000005"),
                Name = "Amazon Middle East",
                Website = "https://amazon.ae",
                LogoUrl = "/images/distributors/amazon.png",
                IsPartner = false
            }
        };

        await context.Distributors.AddRangeAsync(distributors);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} distributors", distributors.Count);
    }

    private static async Task SeedBrandsAsync(SkinPAIDbContext context, ILogger logger)
    {
        var brands = new List<Brand>
        {
            new Brand
            {
                BrandId = Guid.Parse("b0000000-0000-0000-0000-000000000001"),
                BrandName = "CeraVe",
                LogoUrl = "/images/brands/cerave.png",
                Description = "Developed with dermatologists, CeraVe delivers essential ceramides for healthy skin.",
                Website = "https://cerave.com",
                IsVerified = true,
                IsPartner = true
            },
            new Brand
            {
                BrandId = Guid.Parse("b0000000-0000-0000-0000-000000000002"),
                BrandName = "La Roche-Posay",
                LogoUrl = "/images/brands/laroche.png",
                Description = "French pharmaceutical skincare brand recommended by 90,000 dermatologists worldwide.",
                Website = "https://laroche-posay.com",
                IsVerified = true,
                IsPartner = true
            },
            new Brand
            {
                BrandId = Guid.Parse("b0000000-0000-0000-0000-000000000003"),
                BrandName = "The Ordinary",
                LogoUrl = "/images/brands/ordinary.png",
                Description = "Clinical formulations with integrity. Effective skincare at affordable prices.",
                Website = "https://theordinary.com",
                IsVerified = true,
                IsPartner = true
            },
            new Brand
            {
                BrandId = Guid.Parse("b0000000-0000-0000-0000-000000000004"),
                BrandName = "Bioderma",
                LogoUrl = "/images/brands/bioderma.png",
                Description = "French pharmaceutical brand focused on dermatological innovation.",
                Website = "https://bioderma.com",
                IsVerified = true,
                IsPartner = true
            },
            new Brand
            {
                BrandId = Guid.Parse("b0000000-0000-0000-0000-000000000005"),
                BrandName = "Eucerin",
                LogoUrl = "/images/brands/eucerin.png",
                Description = "Medical skincare brand with over 100 years of dermatological expertise.",
                Website = "https://eucerin.com",
                IsVerified = true,
                IsPartner = true
            },
            new Brand
            {
                BrandId = Guid.Parse("b0000000-0000-0000-0000-000000000006"),
                BrandName = "Neutrogena",
                LogoUrl = "/images/brands/neutrogena.png",
                Description = "Dermatologist-recommended skincare for healthy, beautiful skin.",
                Website = "https://neutrogena.com",
                IsVerified = true,
                IsPartner = false
            },
            new Brand
            {
                BrandId = Guid.Parse("b0000000-0000-0000-0000-000000000007"),
                BrandName = "Avene",
                LogoUrl = "/images/brands/avene.png",
                Description = "French skincare enriched with Avene Thermal Spring Water.",
                Website = "https://avene.com",
                IsVerified = true,
                IsPartner = true
            },
            new Brand
            {
                BrandId = Guid.Parse("b0000000-0000-0000-0000-000000000008"),
                BrandName = "Vichy",
                LogoUrl = "/images/brands/vichy.png",
                Description = "French luxury skincare with volcanic water from Vichy.",
                Website = "https://vichy.com",
                IsVerified = true,
                IsPartner = true
            }
        };

        await context.Brands.AddRangeAsync(brands);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} brands", brands.Count);
    }

    private static async Task SeedProductsAsync(SkinPAIDbContext context, ILogger logger)
    {
        // Get category IDs (already seeded in OnModelCreating)
        var cleanserCategoryId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var tonerCategoryId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var serumCategoryId = Guid.Parse("10000000-0000-0000-0000-000000000003");
        var moisturizerCategoryId = Guid.Parse("10000000-0000-0000-0000-000000000004");
        var sunscreenCategoryId = Guid.Parse("10000000-0000-0000-0000-000000000005");
        var treatmentCategoryId = Guid.Parse("10000000-0000-0000-0000-000000000006");

        // Brand IDs
        var ceraveBrandId = Guid.Parse("b0000000-0000-0000-0000-000000000001");
        var laRocheBrandId = Guid.Parse("b0000000-0000-0000-0000-000000000002");
        var ordinaryBrandId = Guid.Parse("b0000000-0000-0000-0000-000000000003");
        var biodermaBrandId = Guid.Parse("b0000000-0000-0000-0000-000000000004");
        var eucerinBrandId = Guid.Parse("b0000000-0000-0000-0000-000000000005");
        var neutrogenaBrandId = Guid.Parse("b0000000-0000-0000-0000-000000000006");
        var aveneBrandId = Guid.Parse("b0000000-0000-0000-0000-000000000007");
        var vichyBrandId = Guid.Parse("b0000000-0000-0000-0000-000000000008");

        // Distributor IDs
        var basraPharmacyId = Guid.Parse("d0000000-0000-0000-0000-000000000001");
        var baghdadBeautyId = Guid.Parse("d0000000-0000-0000-0000-000000000002");
        var alRasheedId = Guid.Parse("d0000000-0000-0000-0000-000000000003");
        var iraqiDermaId = Guid.Parse("d0000000-0000-0000-0000-000000000004");

        // Prices in Iraqi Dinar (IQD) - Exchange rate approximately 1 USD = 1,460 IQD
        var products = new List<Product>
        {
            // CeraVe Products
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = ceraveBrandId,
                CategoryId = cleanserCategoryId,
                DistributorId = basraPharmacyId,
                ProductName = "CeraVe Hydrating Facial Cleanser",
                Description = "Gentle cleanser with ceramides and hyaluronic acid for normal to dry skin. Removes dirt and makeup without disrupting the skin barrier.",
                ProductImageUrl = "/images/products/cerave-hydrating-cleanser.jpg",
                Price = 35000, // ~24 USD
                Currency = "IQD",
                Volume = "236ml",
                KeyIngredients = "[\"Ceramides\", \"Hyaluronic Acid\", \"Glycerin\"]",
                SkinTypes = "[\"Normal\", \"Dry\", \"Sensitive\"]",
                SkinConcerns = "[\"Dryness\", \"Dehydration\", \"Sensitivity\"]",
                AverageRating = 4.7m,
                TotalReviews = 1250,
                InStock = true,
                IsFeatured = true,
                IsRecommended = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = ceraveBrandId,
                CategoryId = cleanserCategoryId,
                DistributorId = basraPharmacyId,
                ProductName = "CeraVe Foaming Facial Cleanser",
                Description = "Daily foaming cleanser for oily skin. Removes oil and purifies pores without over-drying.",
                ProductImageUrl = "/images/products/cerave-foaming-cleanser.jpg",
                Price = 35000,
                Currency = "IQD",
                Volume = "236ml",
                KeyIngredients = "[\"Ceramides\", \"Niacinamide\", \"Hyaluronic Acid\"]",
                SkinTypes = "[\"Oily\", \"Combination\"]",
                SkinConcerns = "[\"Oiliness\", \"Acne\", \"Large Pores\"]",
                AverageRating = 4.6m,
                TotalReviews = 980,
                InStock = true,
                IsFeatured = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = ceraveBrandId,
                CategoryId = moisturizerCategoryId,
                DistributorId = basraPharmacyId,
                ProductName = "CeraVe Moisturizing Cream",
                Description = "Rich, non-greasy moisturizer with MVE technology for 24-hour hydration. Essential ceramides restore the skin barrier.",
                ProductImageUrl = "/images/products/cerave-moisturizer.jpg",
                Price = 45000, // ~31 USD
                Currency = "IQD",
                Volume = "454g",
                KeyIngredients = "[\"Ceramides\", \"Hyaluronic Acid\", \"MVE Technology\"]",
                SkinTypes = "[\"All Skin Types\", \"Dry\", \"Very Dry\"]",
                SkinConcerns = "[\"Dryness\", \"Eczema\", \"Rough Texture\"]",
                AverageRating = 4.8m,
                TotalReviews = 2100,
                InStock = true,
                IsFeatured = true,
                IsRecommended = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = ceraveBrandId,
                CategoryId = sunscreenCategoryId,
                DistributorId = basraPharmacyId,
                ProductName = "CeraVe Hydrating Sunscreen SPF 50",
                Description = "Broad spectrum SPF 50 sunscreen with ceramides and niacinamide. Lightweight, non-greasy formula.",
                ProductImageUrl = "/images/products/cerave-sunscreen.jpg",
                Price = 50000, // ~34 USD
                Currency = "IQD",
                Volume = "75ml",
                KeyIngredients = "[\"Zinc Oxide\", \"Ceramides\", \"Niacinamide\"]",
                SkinTypes = "[\"All Skin Types\"]",
                SkinConcerns = "[\"Sun Protection\", \"Aging\"]",
                AverageRating = 4.5m,
                TotalReviews = 650,
                InStock = true,
                IsFeatured = true
            },

            // La Roche-Posay Products
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = laRocheBrandId,
                CategoryId = cleanserCategoryId,
                DistributorId = baghdadBeautyId,
                ProductName = "La Roche-Posay Toleriane Hydrating Gentle Cleanser",
                Description = "Gentle, milky cleanser for normal to dry sensitive skin. Non-foaming formula respects skin's natural barrier.",
                ProductImageUrl = "/images/products/laroche-toleriane-cleanser.jpg",
                Price = 42000, // ~29 USD
                Currency = "IQD",
                Volume = "400ml",
                KeyIngredients = "[\"Ceramides\", \"Niacinamide\", \"Glycerin\"]",
                SkinTypes = "[\"Normal\", \"Dry\", \"Sensitive\"]",
                SkinConcerns = "[\"Sensitivity\", \"Dryness\", \"Redness\"]",
                AverageRating = 4.7m,
                TotalReviews = 890,
                InStock = true,
                IsFeatured = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = laRocheBrandId,
                CategoryId = serumCategoryId,
                DistributorId = baghdadBeautyId,
                ProductName = "La Roche-Posay Effaclar Serum",
                Description = "Pore-refining serum with salicylic acid and glycolic acid. Targets blackheads, blemishes, and uneven texture.",
                ProductImageUrl = "/images/products/laroche-effaclar-serum.jpg",
                Price = 75000, // ~51 USD
                Currency = "IQD",
                Volume = "30ml",
                KeyIngredients = "[\"Salicylic Acid\", \"Glycolic Acid\", \"LHA\"]",
                SkinTypes = "[\"Oily\", \"Combination\", \"Acne-Prone\"]",
                SkinConcerns = "[\"Acne\", \"Blackheads\", \"Large Pores\"]",
                AverageRating = 4.6m,
                TotalReviews = 720,
                InStock = true,
                IsFeatured = true,
                IsRecommended = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = laRocheBrandId,
                CategoryId = sunscreenCategoryId,
                DistributorId = baghdadBeautyId,
                ProductName = "La Roche-Posay Anthelios UV Mune 400 SPF 50+",
                Description = "Revolutionary sun protection with exclusive MEXORYL400 filter. Ultra-light invisible fluid for all skin types.",
                ProductImageUrl = "/images/products/laroche-anthelios.jpg",
                Price = 65000, // ~45 USD
                Currency = "IQD",
                Volume = "50ml",
                KeyIngredients = "[\"MEXORYL400\", \"MEXORYL XL\", \"MEXORYL SX\"]",
                SkinTypes = "[\"All Skin Types\"]",
                SkinConcerns = "[\"Sun Protection\", \"Aging\", \"Hyperpigmentation\"]",
                AverageRating = 4.9m,
                TotalReviews = 1500,
                InStock = true,
                IsFeatured = true,
                IsRecommended = true
            },

            // The Ordinary Products
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = ordinaryBrandId,
                CategoryId = serumCategoryId,
                DistributorId = alRasheedId,
                ProductName = "The Ordinary Niacinamide 10% + Zinc 1%",
                Description = "High-strength vitamin and mineral blemish formula. Reduces the appearance of blemishes and congestion.",
                ProductImageUrl = "/images/products/ordinary-niacinamide.jpg",
                Price = 18000, // ~12 USD
                Currency = "IQD",
                Volume = "30ml",
                KeyIngredients = "[\"Niacinamide\", \"Zinc PCA\"]",
                SkinTypes = "[\"All Skin Types\", \"Oily\", \"Acne-Prone\"]",
                SkinConcerns = "[\"Acne\", \"Oiliness\", \"Large Pores\", \"Uneven Texture\"]",
                AverageRating = 4.5m,
                TotalReviews = 3200,
                InStock = true,
                IsFeatured = true,
                IsRecommended = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = ordinaryBrandId,
                CategoryId = serumCategoryId,
                DistributorId = alRasheedId,
                ProductName = "The Ordinary Hyaluronic Acid 2% + B5",
                Description = "Multi-molecular hyaluronic acid formula for intense hydration at multiple skin depths.",
                ProductImageUrl = "/images/products/ordinary-hyaluronic.jpg",
                Price = 17000, // ~12 USD
                Currency = "IQD",
                Volume = "30ml",
                KeyIngredients = "[\"Hyaluronic Acid\", \"Vitamin B5\"]",
                SkinTypes = "[\"All Skin Types\"]",
                SkinConcerns = "[\"Dehydration\", \"Fine Lines\", \"Dullness\"]",
                AverageRating = 4.6m,
                TotalReviews = 2800,
                InStock = true,
                IsFeatured = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = ordinaryBrandId,
                CategoryId = serumCategoryId,
                DistributorId = alRasheedId,
                ProductName = "The Ordinary Retinol 0.5% in Squalane",
                Description = "Moderate strength retinol in a base of squalane for anti-aging benefits.",
                ProductImageUrl = "/images/products/ordinary-retinol.jpg",
                Price = 16000, // ~11 USD
                Currency = "IQD",
                Volume = "30ml",
                KeyIngredients = "[\"Retinol\", \"Squalane\"]",
                SkinTypes = "[\"Normal\", \"Dry\", \"Combination\"]",
                SkinConcerns = "[\"Fine Lines\", \"Wrinkles\", \"Uneven Texture\", \"Aging\"]",
                AverageRating = 4.4m,
                TotalReviews = 1800,
                InStock = true,
                IsFeatured = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = ordinaryBrandId,
                CategoryId = serumCategoryId,
                DistributorId = alRasheedId,
                ProductName = "The Ordinary Vitamin C Suspension 23% + HA Spheres 2%",
                Description = "Highly potent vitamin C treatment for brightening and antioxidant protection.",
                ProductImageUrl = "/images/products/ordinary-vitc.jpg",
                Price = 15000, // ~10 USD
                Currency = "IQD",
                Volume = "30ml",
                KeyIngredients = "[\"L-Ascorbic Acid\", \"Hyaluronic Acid\"]",
                SkinTypes = "[\"All Skin Types\"]",
                SkinConcerns = "[\"Dullness\", \"Hyperpigmentation\", \"Aging\"]",
                AverageRating = 4.3m,
                TotalReviews = 950,
                InStock = true
            },

            // Bioderma Products
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = biodermaBrandId,
                CategoryId = cleanserCategoryId,
                DistributorId = iraqiDermaId,
                ProductName = "Bioderma Sensibio H2O Micellar Water",
                Description = "Gentle yet powerful micellar water for sensitive skin. Cleanses, removes makeup, and soothes in one step.",
                ProductImageUrl = "/images/products/bioderma-sensibio.jpg",
                Price = 55000, // ~38 USD
                Currency = "IQD",
                Volume = "500ml",
                KeyIngredients = "[\"Micellar Technology\", \"Cucumber Extract\", \"Glycerin\"]",
                SkinTypes = "[\"Sensitive\", \"Normal\", \"Dry\"]",
                SkinConcerns = "[\"Sensitivity\", \"Redness\", \"Makeup Removal\"]",
                AverageRating = 4.8m,
                TotalReviews = 4200,
                InStock = true,
                IsFeatured = true,
                IsRecommended = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = biodermaBrandId,
                CategoryId = cleanserCategoryId,
                DistributorId = iraqiDermaId,
                ProductName = "Bioderma Sebium H2O Micellar Water",
                Description = "Purifying micellar water for oily and combination skin. Controls sebum and unclogs pores.",
                ProductImageUrl = "/images/products/bioderma-sebium.jpg",
                Price = 52000, // ~36 USD
                Currency = "IQD",
                Volume = "500ml",
                KeyIngredients = "[\"Micellar Technology\", \"Zinc Gluconate\", \"Copper Sulfate\"]",
                SkinTypes = "[\"Oily\", \"Combination\", \"Acne-Prone\"]",
                SkinConcerns = "[\"Oiliness\", \"Acne\", \"Large Pores\"]",
                AverageRating = 4.6m,
                TotalReviews = 1800,
                InStock = true,
                IsFeatured = true
            },

            // Eucerin Products
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = eucerinBrandId,
                CategoryId = moisturizerCategoryId,
                DistributorId = basraPharmacyId,
                ProductName = "Eucerin Original Healing Cream",
                Description = "Intensive healing cream for very dry, compromised skin. Fragrance-free and non-comedogenic.",
                ProductImageUrl = "/images/products/eucerin-healing.jpg",
                Price = 38000, // ~26 USD
                Currency = "IQD",
                Volume = "454g",
                KeyIngredients = "[\"Urea\", \"Ceramides\", \"Glycerin\"]",
                SkinTypes = "[\"Very Dry\", \"Dry\", \"Damaged\"]",
                SkinConcerns = "[\"Extreme Dryness\", \"Cracked Skin\", \"Eczema\"]",
                AverageRating = 4.7m,
                TotalReviews = 1100,
                InStock = true,
                IsFeatured = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = eucerinBrandId,
                CategoryId = sunscreenCategoryId,
                DistributorId = basraPharmacyId,
                ProductName = "Eucerin Sun Gel-Cream Oil Control SPF 50+",
                Description = "Ultra-light sunscreen with 8-hour anti-shine technology. Perfect for oily and acne-prone skin.",
                ProductImageUrl = "/images/products/eucerin-sun-oilcontrol.jpg",
                Price = 48000, // ~33 USD
                Currency = "IQD",
                Volume = "50ml",
                KeyIngredients = "[\"Advanced Spectral Technology\", \"Oil Control Technology\"]",
                SkinTypes = "[\"Oily\", \"Combination\", \"Acne-Prone\"]",
                SkinConcerns = "[\"Oiliness\", \"Sun Protection\", \"Acne\"]",
                AverageRating = 4.7m,
                TotalReviews = 950,
                InStock = true,
                IsFeatured = true,
                IsRecommended = true
            },

            // Avene Products
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = aveneBrandId,
                CategoryId = tonerCategoryId,
                DistributorId = baghdadBeautyId,
                ProductName = "Avene Thermal Spring Water Spray",
                Description = "Soothing thermal water spray with unique mineral composition. Calms and softens sensitive skin.",
                ProductImageUrl = "/images/products/avene-thermal-water.jpg",
                Price = 28000, // ~19 USD
                Currency = "IQD",
                Volume = "300ml",
                KeyIngredients = "[\"Avene Thermal Spring Water\"]",
                SkinTypes = "[\"All Skin Types\", \"Sensitive\"]",
                SkinConcerns = "[\"Sensitivity\", \"Redness\", \"Irritation\"]",
                AverageRating = 4.5m,
                TotalReviews = 1600,
                InStock = true,
                IsFeatured = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = aveneBrandId,
                CategoryId = moisturizerCategoryId,
                DistributorId = baghdadBeautyId,
                ProductName = "Avene Tolerance Extreme Emulsion",
                Description = "Ultra-minimal formula for hypersensitive skin. Only 7 essential ingredients for maximum tolerance.",
                ProductImageUrl = "/images/products/avene-tolerance.jpg",
                Price = 58000, // ~40 USD
                Currency = "IQD",
                Volume = "50ml",
                KeyIngredients = "[\"Avene Thermal Spring Water\", \"Squalane\", \"Glycerin\"]",
                SkinTypes = "[\"Hypersensitive\", \"Sensitive\", \"Reactive\"]",
                SkinConcerns = "[\"Extreme Sensitivity\", \"Allergies\", \"Intolerance\"]",
                AverageRating = 4.8m,
                TotalReviews = 720,
                InStock = true,
                IsRecommended = true
            },

            // Vichy Products
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = vichyBrandId,
                CategoryId = serumCategoryId,
                DistributorId = alRasheedId,
                ProductName = "Vichy Minéral 89 Hyaluronic Acid Serum",
                Description = "Fortifying daily booster with 89% mineralizing thermal water and hyaluronic acid.",
                ProductImageUrl = "/images/products/vichy-mineral89.jpg",
                Price = 72000, // ~49 USD
                Currency = "IQD",
                Volume = "50ml",
                KeyIngredients = "[\"Vichy Mineralizing Water\", \"Hyaluronic Acid\"]",
                SkinTypes = "[\"All Skin Types\"]",
                SkinConcerns = "[\"Dehydration\", \"Dullness\", \"Fine Lines\", \"Sensitivity\"]",
                AverageRating = 4.7m,
                TotalReviews = 2400,
                InStock = true,
                IsFeatured = true,
                IsRecommended = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = vichyBrandId,
                CategoryId = treatmentCategoryId,
                DistributorId = alRasheedId,
                ProductName = "Vichy Liftactiv Vitamin C Serum",
                Description = "Brightening and anti-aging serum with 15% pure vitamin C.",
                ProductImageUrl = "/images/products/vichy-liftactiv-vitc.jpg",
                Price = 85000, // ~58 USD
                Currency = "IQD",
                Volume = "20ml",
                KeyIngredients = "[\"Vitamin C 15%\", \"Vitamin E\", \"Fragmented Hyaluronic Acid\"]",
                SkinTypes = "[\"All Skin Types\"]",
                SkinConcerns = "[\"Dullness\", \"Dark Spots\", \"Aging\", \"Uneven Tone\"]",
                AverageRating = 4.6m,
                TotalReviews = 890,
                InStock = true,
                IsFeatured = true
            },

            // Neutrogena Products
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = neutrogenaBrandId,
                CategoryId = cleanserCategoryId,
                DistributorId = iraqiDermaId,
                ProductName = "Neutrogena Hydro Boost Gel Cleanser",
                Description = "Water-gel cleanser that boosts hydration while cleansing. Leaves skin clean and supple.",
                ProductImageUrl = "/images/products/neutrogena-hydroboost-cleanser.jpg",
                Price = 28000, // ~19 USD
                Currency = "IQD",
                Volume = "200ml",
                KeyIngredients = "[\"Hyaluronic Acid\", \"Glycerin\"]",
                SkinTypes = "[\"All Skin Types\", \"Dry\", \"Dehydrated\"]",
                SkinConcerns = "[\"Dehydration\", \"Dryness\"]",
                AverageRating = 4.4m,
                TotalReviews = 1200,
                InStock = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = neutrogenaBrandId,
                CategoryId = moisturizerCategoryId,
                DistributorId = iraqiDermaId,
                ProductName = "Neutrogena Hydro Boost Water Gel",
                Description = "Oil-free gel moisturizer with hyaluronic acid. Provides 48-hour hydration.",
                ProductImageUrl = "/images/products/neutrogena-hydroboost-gel.jpg",
                Price = 42000, // ~29 USD
                Currency = "IQD",
                Volume = "50ml",
                KeyIngredients = "[\"Hyaluronic Acid\", \"Olive Extract\"]",
                SkinTypes = "[\"All Skin Types\", \"Oily\", \"Combination\"]",
                SkinConcerns = "[\"Dehydration\", \"Oiliness\"]",
                AverageRating = 4.5m,
                TotalReviews = 2800,
                InStock = true,
                IsFeatured = true,
                IsRecommended = true
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                BrandId = neutrogenaBrandId,
                CategoryId = sunscreenCategoryId,
                DistributorId = iraqiDermaId,
                ProductName = "Neutrogena Ultra Sheer Dry-Touch SPF 55",
                Description = "Fast-absorbing, breathable sunscreen with Helioplex technology for superior protection.",
                ProductImageUrl = "/images/products/neutrogena-sunscreen.jpg",
                Price = 35000, // ~24 USD
                Currency = "IQD",
                Volume = "88ml",
                KeyIngredients = "[\"Helioplex Technology\", \"Avobenzone\"]",
                SkinTypes = "[\"All Skin Types\"]",
                SkinConcerns = "[\"Sun Protection\", \"Aging\"]",
                AverageRating = 4.3m,
                TotalReviews = 1650,
                InStock = true
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} products with Iraq/Basra market pricing (IQD)", products.Count);
    }

    private static async Task SeedProductBundlesAsync(SkinPAIDbContext context, ILogger logger)
    {
        // Get some products to add to bundles
        var products = await context.Products.Take(10).ToListAsync();
        if (products.Count < 4) return;

        // Get a brand for the bundles
        var brand = await context.Brands.FirstOrDefaultAsync();
        if (brand == null) return;

        var bundles = new List<ProductBundle>
        {
            new ProductBundle
            {
                BundleId = Guid.NewGuid(),
                BrandId = brand.BrandId,
                Name = "بداية العناية بالبشرة - Beginner Skincare Kit",
                Description = "Essential products for those starting their skincare journey. Includes cleanser, moisturizer, and sunscreen.",
                ImageUrl = "/images/bundles/beginner-kit.jpg",
                OriginalPrice = 115000, // Sum of individual prices
                BundlePrice = 95000, // ~15% discount
                Savings = 20000,
                Category = "Beginner",
                ForSkinTypes = "[\"All Skin Types\"]",
                ForSkinConcerns = "[\"General Care\"]",
                IsActive = true
            },
            new ProductBundle
            {
                BundleId = Guid.NewGuid(),
                BrandId = brand.BrandId,
                Name = "مكافحة الشيخوخة - Anti-Aging Essentials",
                Description = "Complete anti-aging routine with retinol, vitamin C, and hydrating serums for youthful skin.",
                ImageUrl = "/images/bundles/antiaging-kit.jpg",
                OriginalPrice = 180000,
                BundlePrice = 145000, // ~20% discount
                Savings = 35000,
                Category = "Anti-Aging",
                ForSkinTypes = "[\"Normal\", \"Dry\", \"Combination\"]",
                ForSkinConcerns = "[\"Fine Lines\", \"Wrinkles\", \"Aging\"]",
                IsActive = true
            },
            new ProductBundle
            {
                BundleId = Guid.NewGuid(),
                BrandId = brand.BrandId,
                Name = "البشرة الدهنية - Oily Skin Control",
                Description = "Products specifically formulated for oily and acne-prone skin. Control excess oil and minimize pores.",
                ImageUrl = "/images/bundles/oily-skin-kit.jpg",
                OriginalPrice = 125000,
                BundlePrice = 105000,
                Savings = 20000,
                Category = "Oily Skin",
                ForSkinTypes = "[\"Oily\", \"Combination\"]",
                ForSkinConcerns = "[\"Acne\", \"Oiliness\", \"Large Pores\"]",
                IsActive = true
            },
            new ProductBundle
            {
                BundleId = Guid.NewGuid(),
                BrandId = brand.BrandId,
                Name = "الحماية من الشمس - Sun Protection Set",
                Description = "Complete sun protection for the hot Iraqi summer. Multiple SPF options for face and body.",
                ImageUrl = "/images/bundles/sun-protection-kit.jpg",
                OriginalPrice = 150000,
                BundlePrice = 125000,
                Savings = 25000,
                Category = "Sun Protection",
                ForSkinTypes = "[\"All Skin Types\"]",
                ForSkinConcerns = "[\"Sun Damage\", \"Aging\", \"Hyperpigmentation\"]",
                IsActive = true
            }
        };

        await context.ProductBundles.AddRangeAsync(bundles);
        await context.SaveChangesAsync();

        // Add bundle items
        var savedBundles = await context.ProductBundles.ToListAsync();
        var bundleItems = new List<ProductBundleItem>();
        
        int displayOrder = 0;
        foreach (var bundle in savedBundles)
        {
            displayOrder = 0;
            var randomProducts = products.OrderBy(x => Guid.NewGuid()).Take(3).ToList();
            foreach (var product in randomProducts)
            {
                bundleItems.Add(new ProductBundleItem
                {
                    BundleId = bundle.BundleId,
                    ProductId = product.ProductId,
                    DisplayOrder = displayOrder++
                });
            }
        }

        await context.ProductBundleItems.AddRangeAsync(bundleItems);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} product bundles with items", bundles.Count);
    }
}
