using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SkinPAI.API.Models.Entities;

namespace SkinPAI.API.Data;

public class SkinPAIDbContext : DbContext
{
    public SkinPAIDbContext(DbContextOptions<SkinPAIDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        // Suppress the PendingModelChangesWarning for EF 9
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    // Users & Authentication
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<SkinProfile> SkinProfiles { get; set; }

    // Subscriptions & Payments
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }

    // Skin Scanning
    public DbSet<SkinScan> SkinScans { get; set; }
    public DbSet<SkinAnalysisResult> SkinAnalysisResults { get; set; }
    public DbSet<DailyScanUsage> DailyScanUsages { get; set; }

    // Products
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Distributor> Distributors { get; set; }
    public DbSet<BrandDistributor> BrandDistributors { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductRecommendation> ProductRecommendations { get; set; }
    public DbSet<ProductBundle> ProductBundles { get; set; }
    public DbSet<ProductBundleItem> ProductBundleItems { get; set; }
    public DbSet<UserProductFavorite> UserProductFavorites { get; set; }

    // Routines
    public DbSet<UserRoutine> UserRoutines { get; set; }
    public DbSet<RoutineStep> RoutineSteps { get; set; }
    public DbSet<RoutineCompletion> RoutineCompletions { get; set; }
    public DbSet<RoutineReminder> RoutineReminders { get; set; }

    // Achievements
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<UserAchievement> UserAchievements { get; set; }

    // Notifications
    public DbSet<Notification> Notifications { get; set; }

    // Creator Stations
    public DbSet<CreatorStation> CreatorStations { get; set; }
    public DbSet<StationFollower> StationFollowers { get; set; }

    // Community
    public DbSet<CommunityPost> CommunityPosts { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<PostComment> PostComments { get; set; }

    // Campaigns
    public DbSet<BrandCampaign> BrandCampaigns { get; set; }
    public DbSet<CampaignParticipant> CampaignParticipants { get; set; }

    // Chat
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default delete behavior to Restrict to prevent cascade cycles
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // Configure decimal precision for all monetary/numeric properties
        modelBuilder.Entity<WalletTransaction>()
            .Property(w => w.Balance)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<WalletTransaction>()
            .Property(w => w.Amount)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<PaymentTransaction>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<SubscriptionPlan>()
            .Property(s => s.PriceAmount)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<Product>()
            .Property(p => p.DiscountPercent)
            .HasPrecision(5, 2);
        
        modelBuilder.Entity<Product>()
            .Property(p => p.AverageRating)
            .HasPrecision(3, 2);
        
        modelBuilder.Entity<User>()
            .Property(u => u.WalletBalance)
            .HasPrecision(18, 2);

        // User configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.MembershipType);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.IsDeleted);

            entity.HasOne(u => u.SkinProfile)
                .WithOne(sp => sp.User)
                .HasForeignKey<SkinProfile>(sp => sp.UserId);

            entity.HasOne(u => u.CreatorStation)
                .WithOne(cs => cs.User)
                .HasForeignKey<CreatorStation>(cs => cs.UserId);
        });

        // RefreshToken configurations
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
        });

        // UserRole configurations
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
            
            // Prevent cascade delete cycles
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Role seed data
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = Guid.Parse("11111111-1111-1111-1111-111111111111"), RoleName = "Admin", Description = "Full system access" },
            new Role { RoleId = Guid.Parse("22222222-2222-2222-2222-222222222222"), RoleName = "Moderator", Description = "Content moderation access" },
            new Role { RoleId = Guid.Parse("33333333-3333-3333-3333-333333333333"), RoleName = "User", Description = "Standard user access" },
            new Role { RoleId = Guid.Parse("44444444-4444-4444-4444-444444444444"), RoleName = "Creator", Description = "Content creator access" }
        );

        // SubscriptionPlan seed data
        modelBuilder.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan { PlanId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), PlanName = "Member", BillingPeriod = "Monthly", PriceAmount = 9.99m, DailyScansLimit = 5, Features = "[\"5 scans per day\",\"Progress tracking\",\"Routine reminders\",\"Achievements\",\"Personalized tips\",\"Data backup\"]" },
            new SubscriptionPlan { PlanId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), PlanName = "Member", BillingPeriod = "Yearly", PriceAmount = 79.99m, DailyScansLimit = 5, Features = "[\"5 scans per day\",\"Progress tracking\",\"Routine reminders\",\"Achievements\",\"Personalized tips\",\"Data backup\"]" },
            new SubscriptionPlan { PlanId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), PlanName = "Pro", BillingPeriod = "Monthly", PriceAmount = 29.99m, DailyScansLimit = null, Features = "[\"Unlimited scans\",\"Creator Station\",\"Advanced Analytics\",\"Audience insights\",\"Monetization tools\",\"Priority support\",\"Early feature access\",\"Custom branding\",\"Verified badge\",\"Advanced data export\"]" },
            new SubscriptionPlan { PlanId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), PlanName = "Pro", BillingPeriod = "Yearly", PriceAmount = 239.99m, DailyScansLimit = null, Features = "[\"Unlimited scans\",\"Creator Station\",\"Advanced Analytics\",\"Audience insights\",\"Monetization tools\",\"Priority support\",\"Early feature access\",\"Custom branding\",\"Verified badge\",\"Advanced data export\"]" }
        );

        // ProductCategory seed data
        modelBuilder.Entity<ProductCategory>().HasData(
            new ProductCategory { CategoryId = Guid.Parse("10000000-0000-0000-0000-000000000001"), CategoryName = "Cleanser", CategoryIcon = "🧼", DisplayOrder = 1 },
            new ProductCategory { CategoryId = Guid.Parse("10000000-0000-0000-0000-000000000002"), CategoryName = "Toner", CategoryIcon = "💧", DisplayOrder = 2 },
            new ProductCategory { CategoryId = Guid.Parse("10000000-0000-0000-0000-000000000003"), CategoryName = "Serum", CategoryIcon = "✨", DisplayOrder = 3 },
            new ProductCategory { CategoryId = Guid.Parse("10000000-0000-0000-0000-000000000004"), CategoryName = "Moisturizer", CategoryIcon = "💦", DisplayOrder = 4 },
            new ProductCategory { CategoryId = Guid.Parse("10000000-0000-0000-0000-000000000005"), CategoryName = "Sunscreen", CategoryIcon = "☀️", DisplayOrder = 5 },
            new ProductCategory { CategoryId = Guid.Parse("10000000-0000-0000-0000-000000000006"), CategoryName = "Treatment", CategoryIcon = "💊", DisplayOrder = 6 },
            new ProductCategory { CategoryId = Guid.Parse("10000000-0000-0000-0000-000000000007"), CategoryName = "Mask", CategoryIcon = "🎭", DisplayOrder = 7 },
            new ProductCategory { CategoryId = Guid.Parse("10000000-0000-0000-0000-000000000008"), CategoryName = "Exfoliant", CategoryIcon = "🌟", DisplayOrder = 8 }
        );

        // Achievement seed data
        modelBuilder.Entity<Achievement>().HasData(
            new Achievement { AchievementId = Guid.Parse("a0000000-0000-0000-0000-000000000001"), Title = "First Scan", Description = "Complete your first skin scan", Icon = "📷", AchievementType = "Scans", RequiredProgress = 1, Points = 10 },
            new Achievement { AchievementId = Guid.Parse("a0000000-0000-0000-0000-000000000002"), Title = "Week Streak", Description = "Scan for 7 consecutive days", Icon = "🔥", AchievementType = "Scans", RequiredProgress = 7, Points = 50 },
            new Achievement { AchievementId = Guid.Parse("a0000000-0000-0000-0000-000000000003"), Title = "Routine Master", Description = "Complete 30 daily routines", Icon = "⭐", AchievementType = "Routines", RequiredProgress = 30, Points = 100 },
            new Achievement { AchievementId = Guid.Parse("a0000000-0000-0000-0000-000000000004"), Title = "Community Star", Description = "Get 100 likes on your posts", Icon = "💫", AchievementType = "Community", RequiredProgress = 100, Points = 75 },
            new Achievement { AchievementId = Guid.Parse("a0000000-0000-0000-0000-000000000005"), Title = "Skin Expert", Description = "Complete 100 skin scans", Icon = "🏆", AchievementType = "Scans", RequiredProgress = 100, Points = 200 }
        );

        // SkinScan configurations
        modelBuilder.Entity<SkinScan>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ScanDate);
            entity.HasIndex(e => e.AIProcessingStatus);

            entity.HasOne(s => s.AnalysisResult)
                .WithOne(a => a.SkinScan)
                .HasForeignKey<SkinAnalysisResult>(a => a.ScanId);
        });

        // UserSubscription configurations
        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.EndDate);
        });

        // Product configurations
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.BrandId);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.AverageRating);
            
            // Prevent cascade delete cycles
            entity.HasOne(e => e.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(e => e.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ProductRecommendation configurations
        modelBuilder.Entity<ProductRecommendation>(entity =>
        {
            // Prevent cascade delete cycles
            entity.HasOne(e => e.SkinScan)
                .WithMany(s => s.ProductRecommendations)
                .HasForeignKey(e => e.ScanId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Product)
                .WithMany(p => p.ProductRecommendations)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserProductFavorite configurations
        modelBuilder.Entity<UserProductFavorite>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
            
            // Prevent cascade delete cycles
            entity.HasOne(e => e.User)
                .WithMany(u => u.ProductFavorites)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Product)
                .WithMany(p => p.UserFavorites)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BrandDistributor configurations
        modelBuilder.Entity<BrandDistributor>(entity =>
        {
            entity.HasIndex(e => new { e.BrandId, e.DistributorId }).IsUnique();
        });

        // ProductBundleItem configurations
        modelBuilder.Entity<ProductBundleItem>(entity =>
        {
            entity.HasIndex(e => new { e.BundleId, e.ProductId }).IsUnique();
            
            // Prevent cascade delete cycles
            entity.HasOne(e => e.Product)
                .WithMany(p => p.BundleItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Bundle)
                .WithMany(b => b.BundleItems)
                .HasForeignKey(e => e.BundleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DailyScanUsage configurations
        modelBuilder.Entity<DailyScanUsage>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ScanDate }).IsUnique();
        });

        // CreatorStation configurations
        modelBuilder.Entity<CreatorStation>(entity =>
        {
            entity.HasIndex(e => e.StationSlug).IsUnique();
            entity.HasIndex(e => e.FollowersCount);
        });

        // StationFollower configurations
        modelBuilder.Entity<StationFollower>(entity =>
        {
            entity.HasIndex(e => new { e.StationId, e.FollowerUserId }).IsUnique();
            
            // Prevent cascade delete cycles
            entity.HasOne(e => e.Follower)
                .WithMany(u => u.FollowingStations)
                .HasForeignKey(e => e.FollowerUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Station)
                .WithMany(s => s.Followers)
                .HasForeignKey(e => e.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CommunityPost configurations
        modelBuilder.Entity<CommunityPost>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.StationId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PublishedAt);
            
            // Prevent cascade delete cycles
            entity.HasOne(e => e.Station)
                .WithMany(s => s.Posts)
                .HasForeignKey(e => e.StationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PostLike configurations
        modelBuilder.Entity<PostLike>(entity =>
        {
            entity.HasIndex(e => new { e.PostId, e.UserId }).IsUnique();
            
            // Prevent cascade delete cycles
            entity.HasOne(e => e.User)
                .WithMany(u => u.PostLikes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PostComment configurations
        modelBuilder.Entity<PostComment>(entity =>
        {
            // Prevent cascade delete cycles  
            entity.HasOne(e => e.User)
                .WithMany(u => u.PostComments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(pc => pc.ParentComment)
                .WithMany(pc => pc.Replies)
                .HasForeignKey(pc => pc.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ChatMessage configurations
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasIndex(e => e.SenderUserId);
            entity.HasIndex(e => e.ReceiverUserId);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // WalletTransaction configurations
        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // PaymentTransaction configurations
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Notification configurations
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.CreatedAt);
        });

        // CampaignParticipant configurations
        modelBuilder.Entity<CampaignParticipant>(entity =>
        {
            // Prevent cascade delete cycles
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Participants)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RoutineStep configurations
        modelBuilder.Entity<RoutineStep>(entity =>
        {
            // Prevent cascade delete cycles on Product
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserAchievement configurations
        modelBuilder.Entity<UserAchievement>(entity =>
        {
            // Prevent cascade delete cycles
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserAchievements)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(e => e.AchievementId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RoutineCompletion configurations
        modelBuilder.Entity<RoutineCompletion>(entity =>
        {
            // Prevent cascade delete cycles
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Routine)
                .WithMany(r => r.Completions)
                .HasForeignKey(e => e.RoutineId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RoutineReminder configurations
        modelBuilder.Entity<RoutineReminder>(entity =>
        {
            // Prevent cascade delete cycles
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Routine)
                .WithMany(r => r.Reminders)
                .HasForeignKey(e => e.RoutineId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DailyScanUsage configurations
        modelBuilder.Entity<DailyScanUsage>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ScanDate }).IsUnique();
        });

        // UserSubscription configurations (extended)
        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.EndDate);
            
            // Prevent cascade delete cycles
            entity.HasOne(e => e.Plan)
                .WithMany()
                .HasForeignKey(e => e.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
