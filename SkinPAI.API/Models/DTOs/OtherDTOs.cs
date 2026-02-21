namespace SkinPAI.API.Models.DTOs;

// ==================== Notification DTOs ====================
public record NotificationDto(
    Guid NotificationId,
    string NotificationType,
    string Title,
    string? Body,
    bool IsRead,
    string? ActionUrl,
    string? IconUrl,
    Guid? RelatedEntityId,
    DateTime CreatedAt,
    DateTime? ReadAt
);

public record MarkNotificationReadRequest(
    Guid[] NotificationIds
);

public record NotificationSettingsDto(
    bool ScanReminders,
    bool RoutineReminders,
    bool ProductDeals,
    bool CommunityUpdates,
    bool InfluencerPosts,
    bool EmailNotifications
);

public record UpdateNotificationSettingsRequest(
    bool? ScanReminders,
    bool? RoutineReminders,
    bool? ProductDeals,
    bool? CommunityUpdates,
    bool? InfluencerPosts,
    bool? EmailNotifications
);

// ==================== Achievement DTOs ====================
public record AchievementDto(
    Guid AchievementId,
    string AchievementCode,
    string AchievementName,
    string? Description,
    string? IconUrl,
    string? Category,
    string? Rarity,
    int PointsValue,
    string? UnlockCriteria
);

public record UserAchievementDto(
    Guid AchievementId,
    string AchievementCode,
    string AchievementName,
    string? Description,
    string? IconUrl,
    string? Category,
    string? Rarity,
    int PointsValue,
    DateTime UnlockedAt,
    int Progress
);

// ==================== Chat DTOs ====================
public record ChatMessageDto(
    Guid MessageId,
    Guid SenderId,
    string? SenderName,
    string? SenderProfileImage,
    Guid ReceiverId,
    string MessageType,
    string Content,
    string? MediaUrl,
    bool IsRead,
    DateTime SentAt,
    DateTime? ReadAt,
    bool IsMine
);

public record SendMessageRequest(
    Guid ReceiverId,
    string Content,
    string? MessageType = "Text",
    string? MediaUrl = null
);

public record ChatConversationDto(
    Guid PartnerId,
    string PartnerName,
    string? PartnerProfileImage,
    bool IsVerified,
    string? LastMessage,
    DateTime LastMessageAt,
    int UnreadCount
);

// ==================== Dashboard DTOs ====================
public record MemberDashboardDto(
    UserDto User,
    DailyScanUsageDto DailyScanUsage,
    SkinScanDto? LatestScan,
    List<SkinScanDto> RecentScans,
    List<RoutineDto> ActiveRoutines,
    List<UserAchievementDto> RecentAchievements,
    int UnreadNotifications
);

public record ProDashboardDto(
    UserDto User,
    CreatorStationDto? Station,
    DailyScanUsageDto DailyScanUsage,
    SkinScanDto? LatestScan,
    int TotalFollowers,
    int TotalPosts,
    long TotalViews,
    int TotalLikes,
    List<CommunityPostDto> RecentPosts,
    int UnreadMessages,
    int UnreadNotifications
);

// ==================== Statistics DTOs ====================
public record SkinProgressDto(
    List<SkinProgressPointDto> OverallScoreProgress,
    List<SkinProgressPointDto> HydrationProgress,
    List<SkinProgressPointDto> AcneProgress,
    decimal AverageScore,
    decimal ScoreChange,
    int TotalScans
);

public record SkinProgressPointDto(
    DateTime Date,
    decimal Value
);

public record CommunityStatsDto(
    int TotalMembers,
    int ActiveToday,
    int PostsToday,
    string[] TopHashtags
);

// ==================== Subscription DTOs ====================
public record SubscriptionPlanDto(
    Guid PlanId,
    string PlanCode,
    string PlanName,
    string? Description,
    decimal PriceMonthly,
    decimal PriceYearly,
    int ScansPerDay,
    bool HasAdvancedAnalysis,
    bool HasProductRecommendations,
    bool HasProgressTracking,
    bool HasCommunityAccess,
    bool HasCreatorStudio,
    bool HasPrioritySupport,
    bool AdFree,
    string? FeatureListJson
);

public record UserSubscriptionDto(
    Guid SubscriptionId,
    Guid PlanId,
    string PlanCode,
    string PlanName,
    DateTime StartDate,
    DateTime? EndDate,
    string BillingCycle,
    bool IsActive,
    bool AutoRenew,
    DateTime? CancelledAt,
    DateTime? NextBillingDate,
    decimal Amount
);

public record SubscribeRequest(
    Guid PlanId,
    string? BillingCycle = "Monthly",
    bool AutoRenew = true,
    string? PaymentMethod = "Card"
);

public record WalletInfoDto(
    decimal Balance,
    string Currency,
    List<WalletTransactionSummary> RecentTransactions
);

public record WalletTransactionSummary(
    string TransactionType,
    decimal Amount,
    DateTime TransactionDate
);

public record WalletTransactionDto(
    Guid TransactionId,
    string TransactionType,
    decimal Amount,
    decimal Balance,
    string? Description,
    DateTime TransactionDate
);

public record AddFundsRequest(
    decimal Amount,
    string? PaymentReference = null
);

public record PaymentTransactionDto(
    Guid TransactionId,
    string TransactionType,
    decimal Amount,
    string Currency,
    string Status,
    string? PaymentMethod,
    string? Description,
    DateTime TransactionDate
);

// ==================== Influencer DTOs ====================
public record InfluencerProfileDto(
    Guid UserId,
    string FullName,
    string Handle,
    string? ProfileImage,
    bool IsVerified,
    string? Bio,
    int FollowersCount,
    int FollowingCount,
    int PostCount,
    long LikesCount,
    string[]? Specialties,
    DateTime JoinedAt,
    int Rank,
    decimal? EngagementRate,
    Guid? StationId
);

// ==================== Product Summary DTO ====================
public record ProductSummaryDto(
    Guid ProductId,
    string Name,
    string? ImageUrl,
    decimal Price,
    decimal? OriginalPrice,
    decimal? DiscountPercent,
    decimal AverageRating,
    int TotalReviews,
    bool InStock,
    string BrandName
);

// ==================== Post Comment DTO ====================
public record PostCommentDto(
    Guid CommentId,
    Guid PostId,
    Guid UserId,
    string? UserName,
    string? UserProfileImage,
    Guid? ParentCommentId,
    string Content,
    int LikeCount,
    DateTime CreatedAt,
    List<PostCommentDto>? Replies
);

public record CreateCommentRequest(
    string Content,
    Guid? ParentCommentId = null
);

// ==================== Community Create/Update DTOs ====================
public record UpdatePostRequest(
    string? Title,
    string? Content,
    string[]? Tags,
    string[]? Hashtags,
    string? Status
);

public record CreateStationRequest(
    string StationName,
    string StationSlug,
    string? Bio,
    string? Description,
    string? Location,
    string[]? Specialties,
    string? Theme
);

public record UpdateStationRequest(
    string? StationName,
    string? Bio,
    string? Description,
    string? Location,
    string? InstagramUrl,
    string? YoutubeUrl,
    string? TikTokUrl,
    string? TwitterUrl,
    string? WebsiteUrl,
    string? Email,
    string[]? Specialties,
    string[]? Certifications,
    string? Experience,
    string? ContentFrequency,
    string? Theme,
    bool? IsPublished,
    bool? AllowComments,
    bool? AllowMessages
);

// ==================== Routine Additional DTOs ====================
public record UpdateRoutineRequest(
    string? RoutineName,
    string? RoutineType,
    bool? IsActive
);

public record CreateRoutineStepRequest(
    int StepOrder,
    string StepName,
    string? Instructions,
    int? DurationMinutes,
    Guid? ProductId
);

public record CompleteRoutineRequest(
    Guid RoutineId,
    string? Notes
);

public record CreateReminderRequest(
    Guid RoutineId,
    TimeOnly ReminderTime,
    int[] DaysOfWeek,
    bool SoundEnabled = true,
    bool VibrationEnabled = true
);

public record UpdateReminderRequest(
    TimeOnly? ReminderTime,
    int[]? DaysOfWeek,
    bool? IsEnabled,
    bool? SoundEnabled,
    bool? VibrationEnabled
);

public record RoutineCompletionDto(
    long CompletionId,
    Guid RoutineId,
    string RoutineName,
    DateOnly CompletionDate,
    DateTime CompletedAt,
    string? Notes
);
