using Microsoft.EntityFrameworkCore.Storage;
using SkinPAI.API.Data;
using SkinPAI.API.Models.Entities;

namespace SkinPAI.API.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly SkinPAIDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<User>? _users;
    private IRepository<RefreshToken>? _refreshTokens;
    private IRepository<Role>? _roles;
    private IRepository<UserRole>? _userRoles;
    private IRepository<SkinProfile>? _skinProfiles;
    private IRepository<SubscriptionPlan>? _subscriptionPlans;
    private IRepository<UserSubscription>? _userSubscriptions;
    private IRepository<PaymentTransaction>? _paymentTransactions;
    private IRepository<WalletTransaction>? _walletTransactions;
    private IRepository<SkinScan>? _skinScans;
    private IRepository<SkinAnalysisResult>? _skinAnalysisResults;
    private IRepository<DailyScanUsage>? _dailyScanUsages;
    private IRepository<Brand>? _brands;
    private IRepository<Distributor>? _distributors;
    private IRepository<BrandDistributor>? _brandDistributors;
    private IRepository<ProductCategory>? _productCategories;
    private IRepository<Product>? _products;
    private IRepository<ProductRecommendation>? _productRecommendations;
    private IRepository<ProductBundle>? _productBundles;
    private IRepository<ProductBundleItem>? _productBundleItems;
    private IRepository<UserProductFavorite>? _userProductFavorites;
    private IRepository<UserRoutine>? _userRoutines;
    private IRepository<RoutineStep>? _routineSteps;
    private IRepository<RoutineCompletion>? _routineCompletions;
    private IRepository<RoutineReminder>? _routineReminders;
    private IRepository<Achievement>? _achievements;
    private IRepository<UserAchievement>? _userAchievements;
    private IRepository<Notification>? _notifications;
    private IRepository<CreatorStation>? _creatorStations;
    private IRepository<StationFollower>? _stationFollowers;
    private IRepository<CommunityPost>? _communityPosts;
    private IRepository<PostLike>? _postLikes;
    private IRepository<PostComment>? _postComments;
    private IRepository<BrandCampaign>? _brandCampaigns;
    private IRepository<CampaignParticipant>? _campaignParticipants;
    private IRepository<ChatMessage>? _chatMessages;

    public UnitOfWork(SkinPAIDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);
    public IRepository<Role> Roles => _roles ??= new Repository<Role>(_context);
    public IRepository<UserRole> UserRoles => _userRoles ??= new Repository<UserRole>(_context);
    public IRepository<SkinProfile> SkinProfiles => _skinProfiles ??= new Repository<SkinProfile>(_context);
    public IRepository<SubscriptionPlan> SubscriptionPlans => _subscriptionPlans ??= new Repository<SubscriptionPlan>(_context);
    public IRepository<UserSubscription> UserSubscriptions => _userSubscriptions ??= new Repository<UserSubscription>(_context);
    public IRepository<PaymentTransaction> PaymentTransactions => _paymentTransactions ??= new Repository<PaymentTransaction>(_context);
    public IRepository<WalletTransaction> WalletTransactions => _walletTransactions ??= new Repository<WalletTransaction>(_context);
    public IRepository<SkinScan> SkinScans => _skinScans ??= new Repository<SkinScan>(_context);
    public IRepository<SkinAnalysisResult> SkinAnalysisResults => _skinAnalysisResults ??= new Repository<SkinAnalysisResult>(_context);
    public IRepository<DailyScanUsage> DailyScanUsages => _dailyScanUsages ??= new Repository<DailyScanUsage>(_context);
    public IRepository<Brand> Brands => _brands ??= new Repository<Brand>(_context);
    public IRepository<Distributor> Distributors => _distributors ??= new Repository<Distributor>(_context);
    public IRepository<BrandDistributor> BrandDistributors => _brandDistributors ??= new Repository<BrandDistributor>(_context);
    public IRepository<ProductCategory> ProductCategories => _productCategories ??= new Repository<ProductCategory>(_context);
    public IRepository<Product> Products => _products ??= new Repository<Product>(_context);
    public IRepository<ProductRecommendation> ProductRecommendations => _productRecommendations ??= new Repository<ProductRecommendation>(_context);
    public IRepository<ProductBundle> ProductBundles => _productBundles ??= new Repository<ProductBundle>(_context);
    public IRepository<ProductBundleItem> ProductBundleItems => _productBundleItems ??= new Repository<ProductBundleItem>(_context);
    public IRepository<UserProductFavorite> UserProductFavorites => _userProductFavorites ??= new Repository<UserProductFavorite>(_context);
    public IRepository<UserRoutine> UserRoutines => _userRoutines ??= new Repository<UserRoutine>(_context);
    public IRepository<RoutineStep> RoutineSteps => _routineSteps ??= new Repository<RoutineStep>(_context);
    public IRepository<RoutineCompletion> RoutineCompletions => _routineCompletions ??= new Repository<RoutineCompletion>(_context);
    public IRepository<RoutineReminder> RoutineReminders => _routineReminders ??= new Repository<RoutineReminder>(_context);
    public IRepository<Achievement> Achievements => _achievements ??= new Repository<Achievement>(_context);
    public IRepository<UserAchievement> UserAchievements => _userAchievements ??= new Repository<UserAchievement>(_context);
    public IRepository<Notification> Notifications => _notifications ??= new Repository<Notification>(_context);
    public IRepository<CreatorStation> CreatorStations => _creatorStations ??= new Repository<CreatorStation>(_context);
    public IRepository<StationFollower> StationFollowers => _stationFollowers ??= new Repository<StationFollower>(_context);
    public IRepository<CommunityPost> CommunityPosts => _communityPosts ??= new Repository<CommunityPost>(_context);
    public IRepository<PostLike> PostLikes => _postLikes ??= new Repository<PostLike>(_context);
    public IRepository<PostComment> PostComments => _postComments ??= new Repository<PostComment>(_context);
    public IRepository<BrandCampaign> BrandCampaigns => _brandCampaigns ??= new Repository<BrandCampaign>(_context);
    public IRepository<CampaignParticipant> CampaignParticipants => _campaignParticipants ??= new Repository<CampaignParticipant>(_context);
    public IRepository<ChatMessage> ChatMessages => _chatMessages ??= new Repository<ChatMessage>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
