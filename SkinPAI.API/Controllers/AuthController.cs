using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Services;

namespace SkinPAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("📝 REGISTER: New registration attempt for email: {Email}", request.Email);
        
        try
        {
            var result = await _authService.RegisterAsync(request);
            _logger.LogInformation("✅ REGISTER SUCCESS: User registered successfully | UserId: {UserId} | Email: {Email} | MembershipType: {MembershipType}", 
                result.UserId, result.Email, result.MembershipType);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ REGISTER FAILED: Registration failed for email: {Email} | Reason: {ErrorMessage}", 
                request.Email, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("🔐 LOGIN: Login attempt for email: {Email}", request.Email);
        
        try
        {
            var result = await _authService.LoginAsync(request);
            _logger.LogInformation("✅ LOGIN SUCCESS: User logged in | UserId: {UserId} | Email: {Email} | MembershipType: {MembershipType}", 
                result.UserId, result.Email, result.MembershipType);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("⚠️ LOGIN FAILED: Invalid credentials for email: {Email} | Reason: {ErrorMessage}", 
                request.Email, ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Login as a guest user
    /// </summary>
    [HttpPost("guest")]
    [ProducesResponseType(typeof(GuestLoginResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GuestLoginResponse>> GuestLogin()
    {
        _logger.LogInformation("👤 GUEST LOGIN: New guest session requested");
        
        var result = await _authService.GuestLoginAsync();
        _logger.LogInformation("✅ GUEST LOGIN SUCCESS: Guest session created | UserId: {UserId} | ExpiresAt: {ExpiresAt}", 
            result.UserId, result.ExpiresAt);
        return Ok(result);
    }

    /// <summary>
    /// Social login (Google, Apple, Facebook)
    /// </summary>
    [HttpPost("social")]
    [ProducesResponseType(typeof(SocialAuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SocialAuthResponse>> SocialLogin([FromBody] SocialLoginRequest request)
    {
        _logger.LogInformation("🌐 SOCIAL LOGIN: Social authentication attempt | Provider: {Provider} | Email: {Email}", 
            request.Provider, request.Email ?? "N/A");
        
        try
        {
            var result = await _authService.SocialLoginAsync(request);
            _logger.LogInformation("✅ SOCIAL LOGIN SUCCESS: User authenticated via {Provider} | UserId: {UserId} | IsNewUser: {IsNewUser}", 
                request.Provider, result.UserId, result.IsNewUser);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ SOCIAL LOGIN FAILED: Social auth failed | Provider: {Provider} | Reason: {ErrorMessage}", 
                request.Provider, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogDebug("🔄 TOKEN REFRESH: Token refresh requested");
        
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            _logger.LogInformation("✅ TOKEN REFRESH SUCCESS: Token refreshed for UserId: {UserId}", result.UserId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("⚠️ TOKEN REFRESH FAILED: Invalid or expired refresh token | Reason: {ErrorMessage}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Logout and revoke refresh token
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "Unknown";
        _logger.LogInformation("🚪 LOGOUT: User logout requested | UserId: {UserId}", userId);
        
        await _authService.RevokeTokenAsync(request.RefreshToken);
        _logger.LogInformation("✅ LOGOUT SUCCESS: User logged out | UserId: {UserId}", userId);
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Validate current access token
    /// </summary>
    [Authorize]
    [HttpGet("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult ValidateToken()
    {
        var userId = User.FindFirst("sub")?.Value ?? "Unknown";
        _logger.LogDebug("🔍 TOKEN VALIDATE: Token validation requested | UserId: {UserId}", userId);
        return Ok(new { valid = true });
    }
}

public record RefreshTokenRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);
