using SkinPAI.API.Models.Entities;

namespace SkinPAI.API.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    IRepository<Role> Roles { get; }
    IRepository<UserRole> UserRoles { get; }
    IRepository<SkinProfile> SkinProfiles { get; }
    IRepository<SubscriptionPlan> SubscriptionPlans { get; }
    IRepository<UserSubscription> UserSubscriptions { get; }
    IRepository<PaymentTransaction> PaymentTransactions { get; }
    IRepository<WalletTransaction> WalletTransactions { get; }
    IRepository<SkinScan> SkinScans { get; }
    IRepository<SkinAnalysisResult> SkinAnalysisResults { get; }
    IRepository<DailyScanUsage> DailyScanUsages { get; }
    IRepository<Brand> Brands { get; }
    IRepository<Distributor> Distributors { get; }
    IRepository<BrandDistributor> BrandDistributors { get; }
    IRepository<ProductCategory> ProductCategories { get; }
    IRepository<Product> Products { get; }
    IRepository<ProductRecommendation> ProductRecommendations { get; }
    IRepository<ProductBundle> ProductBundles { get; }
    IRepository<ProductBundleItem> ProductBundleItems { get; }
    IRepository<UserProductFavorite> UserProductFavorites { get; }
    IRepository<UserRoutine> UserRoutines { get; }
    IRepository<RoutineStep> RoutineSteps { get; }
    IRepository<RoutineCompletion> RoutineCompletions { get; }
    IRepository<RoutineReminder> RoutineReminders { get; }
    IRepository<Achievement> Achievements { get; }
    IRepository<UserAchievement> UserAchievements { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<CreatorStation> CreatorStations { get; }
    IRepository<StationFollower> StationFollowers { get; }
    IRepository<CommunityPost> CommunityPosts { get; }
    IRepository<PostLike> PostLikes { get; }
    IRepository<PostComment> PostComments { get; }
    IRepository<BrandCampaign> BrandCampaigns { get; }
    IRepository<CampaignParticipant> CampaignParticipants { get; }
    IRepository<ChatMessage> ChatMessages { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
