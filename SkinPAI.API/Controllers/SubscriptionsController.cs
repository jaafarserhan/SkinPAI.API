using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Services;
using System.Security.Claims;

namespace SkinPAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(ISubscriptionService subscriptionService, ILogger<SubscriptionsController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get all subscription plans
    /// </summary>
    [AllowAnonymous]
    [HttpGet("plans")]
    [ProducesResponseType(typeof(List<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SubscriptionPlanDto>>> GetPlans()
    {
        var plans = await _subscriptionService.GetPlansAsync();
        return Ok(plans);
    }

    /// <summary>
    /// Get plan by ID
    /// </summary>
    [AllowAnonymous]
    [HttpGet("plans/{planId}")]
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubscriptionPlanDto>> GetPlan(Guid planId)
    {
        var plan = await _subscriptionService.GetPlanByIdAsync(planId);
        if (plan == null) return NotFound();
        return Ok(plan);
    }

    /// <summary>
    /// Get current user's subscription
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserSubscriptionDto>> GetMySubscription()
    {
        var subscription = await _subscriptionService.GetUserSubscriptionAsync(GetUserId());
        if (subscription == null) return NotFound(new { message = "No active subscription" });
        return Ok(subscription);
    }

    /// <summary>
    /// Subscribe to a plan
    /// </summary>
    [HttpPost("subscribe")]
    [ProducesResponseType(typeof(UserSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserSubscriptionDto>> Subscribe([FromBody] SubscribeRequest request)
    {
        try
        {
            var subscription = await _subscriptionService.SubscribeAsync(GetUserId(), request);
            return Ok(subscription);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel subscription
    /// </summary>
    [HttpPost("cancel")]
    [ProducesResponseType(typeof(UserSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserSubscriptionDto>> CancelSubscription()
    {
        try
        {
            var subscription = await _subscriptionService.CancelSubscriptionAsync(GetUserId());
            return Ok(subscription);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reactivate cancelled subscription
    /// </summary>
    [HttpPost("reactivate")]
    [ProducesResponseType(typeof(UserSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserSubscriptionDto>> ReactivateSubscription()
    {
        try
        {
            var subscription = await _subscriptionService.ReactivateSubscriptionAsync(GetUserId());
            return Ok(subscription);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ==================== Wallet ====================

    /// <summary>
    /// Get wallet information
    /// </summary>
    [HttpGet("wallet")]
    [ProducesResponseType(typeof(WalletInfoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WalletInfoDto>> GetWallet()
    {
        try
        {
            var wallet = await _subscriptionService.GetWalletInfoAsync(GetUserId());
            return Ok(wallet);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Add funds to wallet
    /// </summary>
    [HttpPost("wallet/add-funds")]
    [ProducesResponseType(typeof(WalletTransactionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WalletTransactionDto>> AddFunds([FromBody] AddFundsRequest request)
    {
        try
        {
            var transaction = await _subscriptionService.AddFundsAsync(GetUserId(), request);
            return Ok(transaction);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get wallet transaction history
    /// </summary>
    [HttpGet("wallet/history")]
    [ProducesResponseType(typeof(List<WalletTransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WalletTransactionDto>>> GetWalletHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var transactions = await _subscriptionService.GetWalletHistoryAsync(GetUserId(), page, pageSize);
        return Ok(transactions);
    }

    // ==================== Payments ====================

    /// <summary>
    /// Get payment history
    /// </summary>
    [HttpGet("payments")]
    [ProducesResponseType(typeof(List<PaymentTransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PaymentTransactionDto>>> GetPaymentHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var payments = await _subscriptionService.GetPaymentHistoryAsync(GetUserId(), page, pageSize);
        return Ok(payments);
    }
}
