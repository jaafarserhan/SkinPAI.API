using Microsoft.EntityFrameworkCore;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Models.Entities;
using SkinPAI.API.Repositories;

namespace SkinPAI.API.Services;

public interface ISubscriptionService
{
    Task<List<SubscriptionPlanDto>> GetPlansAsync();
    Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid planId);
    Task<UserSubscriptionDto?> GetUserSubscriptionAsync(Guid userId);
    Task<UserSubscriptionDto> SubscribeAsync(Guid userId, SubscribeRequest request);
    Task<UserSubscriptionDto> CancelSubscriptionAsync(Guid userId);
    Task<UserSubscriptionDto> ReactivateSubscriptionAsync(Guid userId);
    
    // Wallet
    Task<WalletInfoDto> GetWalletInfoAsync(Guid userId);
    Task<WalletTransactionDto> AddFundsAsync(Guid userId, AddFundsRequest request);
    Task<List<WalletTransactionDto>> GetWalletHistoryAsync(Guid userId, int page = 1, int pageSize = 20);
    
    // Payments
    Task<List<PaymentTransactionDto>> GetPaymentHistoryAsync(Guid userId, int page = 1, int pageSize = 20);
}

public class SubscriptionService : ISubscriptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(IUnitOfWork unitOfWork, ILogger<SubscriptionService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<SubscriptionPlanDto>> GetPlansAsync()
    {
        var plans = await _unitOfWork.SubscriptionPlans.Query()
            .Where(p => p.IsActive)
            .OrderBy(p => p.PriceMonthly)
            .ToListAsync();

        return plans.Select(p => new SubscriptionPlanDto(
            p.PlanId,
            p.PlanCode,
            p.PlanName,
            p.Description,
            p.PriceMonthly,
            p.PriceYearly,
            p.ScansPerDay,
            p.HasAdvancedAnalysis,
            p.HasProductRecommendations,
            p.HasProgressTracking,
            p.HasCommunityAccess,
            p.HasCreatorStudio,
            p.HasPrioritySupport,
            p.AdFree,
            p.FeatureListJson
        )).ToList();
    }

    public async Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid planId)
    {
        var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(planId);
        if (plan == null) return null;

        return new SubscriptionPlanDto(
            plan.PlanId,
            plan.PlanCode,
            plan.PlanName,
            plan.Description,
            plan.PriceMonthly,
            plan.PriceYearly,
            plan.ScansPerDay,
            plan.HasAdvancedAnalysis,
            plan.HasProductRecommendations,
            plan.HasProgressTracking,
            plan.HasCommunityAccess,
            plan.HasCreatorStudio,
            plan.HasPrioritySupport,
            plan.AdFree,
            plan.FeatureListJson
        );
    }

    public async Task<UserSubscriptionDto?> GetUserSubscriptionAsync(Guid userId)
    {
        var subscription = await _unitOfWork.UserSubscriptions.Query()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

        if (subscription == null) return null;

        return new UserSubscriptionDto(
            subscription.SubscriptionId,
            subscription.PlanId,
            subscription.Plan.PlanCode,
            subscription.Plan.PlanName,
            subscription.StartDate,
            subscription.EndDate,
            subscription.BillingCycle,
            subscription.IsActive,
            subscription.AutoRenew,
            subscription.CancelledAt,
            subscription.NextBillingDate,
            subscription.Amount
        );
    }

    public async Task<UserSubscriptionDto> SubscribeAsync(Guid userId, SubscribeRequest request)
    {
        var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.PlanId);
        if (plan == null)
            throw new KeyNotFoundException("Plan not found");

        // Cancel any existing subscription
        var existingSubscription = await _unitOfWork.UserSubscriptions.FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
        if (existingSubscription != null)
        {
            existingSubscription.IsActive = false;
            existingSubscription.CancelledAt = DateTime.UtcNow;
            _unitOfWork.UserSubscriptions.Update(existingSubscription);
        }

        // Calculate amount and dates
        var isYearly = request.BillingCycle?.ToLower() == "yearly";
        var amount = isYearly ? plan.PriceYearly : plan.PriceMonthly;
        var endDate = isYearly ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMonths(1);

        var subscription = new UserSubscription
        {
            UserId = userId,
            PlanId = request.PlanId,
            StartDate = DateTime.UtcNow,
            EndDate = endDate,
            BillingCycle = isYearly ? "Yearly" : "Monthly",
            IsActive = true,
            AutoRenew = request.AutoRenew,
            NextBillingDate = endDate,
            Amount = amount
        };

        await _unitOfWork.UserSubscriptions.AddAsync(subscription);

        // Create payment transaction
        var payment = new PaymentTransaction
        {
            UserId = userId,
            SubscriptionId = subscription.SubscriptionId,
            TransactionType = "Subscription",
            Amount = amount,
            Currency = "USD",
            Status = "Completed",
            PaymentMethod = request.PaymentMethod ?? "Card",
            Description = $"{plan.PlanName} - {subscription.BillingCycle}"
        };

        await _unitOfWork.PaymentTransactions.AddAsync(payment);

        // Update user membership type
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user != null)
        {
            user.MembershipType = plan.PlanCode;
            _unitOfWork.Users.Update(user);
        }

        await _unitOfWork.SaveChangesAsync();

        return new UserSubscriptionDto(
            subscription.SubscriptionId,
            subscription.PlanId,
            plan.PlanCode,
            plan.PlanName,
            subscription.StartDate,
            subscription.EndDate,
            subscription.BillingCycle,
            subscription.IsActive,
            subscription.AutoRenew,
            subscription.CancelledAt,
            subscription.NextBillingDate,
            subscription.Amount
        );
    }

    public async Task<UserSubscriptionDto> CancelSubscriptionAsync(Guid userId)
    {
        var subscription = await _unitOfWork.UserSubscriptions.Query()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

        if (subscription == null)
            throw new KeyNotFoundException("No active subscription found");

        subscription.AutoRenew = false;
        subscription.CancelledAt = DateTime.UtcNow;
        _unitOfWork.UserSubscriptions.Update(subscription);
        await _unitOfWork.SaveChangesAsync();

        return new UserSubscriptionDto(
            subscription.SubscriptionId,
            subscription.PlanId,
            subscription.Plan.PlanCode,
            subscription.Plan.PlanName,
            subscription.StartDate,
            subscription.EndDate,
            subscription.BillingCycle,
            subscription.IsActive,
            subscription.AutoRenew,
            subscription.CancelledAt,
            subscription.NextBillingDate,
            subscription.Amount
        );
    }

    public async Task<UserSubscriptionDto> ReactivateSubscriptionAsync(Guid userId)
    {
        var subscription = await _unitOfWork.UserSubscriptions.Query()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive && s.CancelledAt != null);

        if (subscription == null)
            throw new KeyNotFoundException("No cancelled subscription found");

        subscription.AutoRenew = true;
        subscription.CancelledAt = null;
        _unitOfWork.UserSubscriptions.Update(subscription);
        await _unitOfWork.SaveChangesAsync();

        return new UserSubscriptionDto(
            subscription.SubscriptionId,
            subscription.PlanId,
            subscription.Plan.PlanCode,
            subscription.Plan.PlanName,
            subscription.StartDate,
            subscription.EndDate,
            subscription.BillingCycle,
            subscription.IsActive,
            subscription.AutoRenew,
            subscription.CancelledAt,
            subscription.NextBillingDate,
            subscription.Amount
        );
    }

    public async Task<WalletInfoDto> GetWalletInfoAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var recentTransactions = await _unitOfWork.WalletTransactions.Query()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.TransactionDate)
            .Take(5)
            .ToListAsync();

        return new WalletInfoDto(
            user.WalletBalance,
            "USD",
            recentTransactions.Select(t => new WalletTransactionSummary(
                t.TransactionType,
                t.Amount,
                t.TransactionDate
            )).ToList()
        );
    }

    public async Task<WalletTransactionDto> AddFundsAsync(Guid userId, AddFundsRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.WalletBalance += request.Amount;
        _unitOfWork.Users.Update(user);

        var transaction = new WalletTransaction
        {
            UserId = userId,
            TransactionType = "Credit",
            Amount = request.Amount,
            Balance = user.WalletBalance,
            Description = "Funds added",
            RelatedReference = request.PaymentReference
        };

        await _unitOfWork.WalletTransactions.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return new WalletTransactionDto(
            transaction.TransactionId,
            transaction.TransactionType,
            transaction.Amount,
            transaction.Balance,
            transaction.Description,
            transaction.TransactionDate
        );
    }

    public async Task<List<WalletTransactionDto>> GetWalletHistoryAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var transactions = await _unitOfWork.WalletTransactions.Query()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return transactions.Select(t => new WalletTransactionDto(
            t.TransactionId,
            t.TransactionType,
            t.Amount,
            t.Balance,
            t.Description,
            t.TransactionDate
        )).ToList();
    }

    public async Task<List<PaymentTransactionDto>> GetPaymentHistoryAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var payments = await _unitOfWork.PaymentTransactions.Query()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return payments.Select(p => new PaymentTransactionDto(
            p.TransactionId,
            p.TransactionType,
            p.Amount,
            p.Currency,
            p.Status,
            p.PaymentMethod,
            p.Description,
            p.TransactionDate
        )).ToList();
    }
}
