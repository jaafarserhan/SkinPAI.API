using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Models.Entities;
using SkinPAI.API.Repositories;

namespace SkinPAI.API.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<GuestLoginResponse> GuestLoginAsync();
    Task<SocialAuthResponse> SocialLoginAsync(SocialLoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(Guid userId, string? ipAddress = null);
}

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogDebug("🔐 AUTH SERVICE: Registration started for email: {Email}", request.Email);
        
        // Check if user already exists
        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
        if (existingUser != null)
        {
            _logger.LogWarning("⚠️ AUTH SERVICE: Registration failed - email already exists: {Email}", request.Email);
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create new user - registered users get Member tier (not Guest)
        var user = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = !string.IsNullOrEmpty(request.DateOfBirth) ? DateOnly.Parse(request.DateOfBirth) : null,
            Gender = request.Gender,
            MembershipType = "Member",  // Registered users start as Member, not Guest
            LockoutEnabled = false,     // Members should not be locked out by default
            SecurityStamp = Guid.NewGuid().ToString()
        };

        await _unitOfWork.Users.AddAsync(user);
        _logger.LogDebug("🔐 AUTH SERVICE: User entity created with UserId: {UserId}", user.UserId);

        // Create skin profile
        var skinProfile = new SkinProfile
        {
            UserId = user.UserId,
            QuestionnaireCompleted = false
        };
        await _unitOfWork.SkinProfiles.AddAsync(skinProfile);
        _logger.LogDebug("🔐 AUTH SERVICE: SkinProfile created for UserId: {UserId}", user.UserId);

        // Assign default user role
        var userRole = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
        if (userRole != null)
        {
            await _unitOfWork.UserRoles.AddAsync(new UserRole
            {
                UserId = user.UserId,
                RoleId = userRole.RoleId
            });
            _logger.LogDebug("🔐 AUTH SERVICE: User role assigned for UserId: {UserId}", user.UserId);
        }

        await _unitOfWork.SaveChangesAsync();

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user.UserId);
        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("✅ AUTH SERVICE: Registration completed | UserId: {UserId} | Email: {Email} | MembershipType: {MembershipType}", 
            user.UserId, user.Email, user.MembershipType);

        return new AuthResponse(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.MembershipType,
            user.QuestionnaireCompleted,
            accessToken,
            refreshToken.Token,
            refreshToken.ExpiresAt
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogDebug("🔐 AUTH SERVICE: Login attempt for email: {Email}", request.Email);
        
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower() && !u.IsDeleted);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("⚠️ AUTH SERVICE: Login failed - invalid credentials for email: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Check lockout
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            _logger.LogWarning("⚠️ AUTH SERVICE: Login failed - account locked | UserId: {UserId} | Email: {Email} | LockoutEnd: {LockoutEnd}", 
                user.UserId, user.Email, user.LockoutEnd);
            throw new UnauthorizedAccessException("Account is locked. Please try again later.");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        user.AccessFailedCount = 0;
        _unitOfWork.Users.Update(user);

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user.UserId);
        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("✅ AUTH SERVICE: Login successful | UserId: {UserId} | Email: {Email} | MembershipType: {MembershipType}", 
            user.UserId, user.Email, user.MembershipType);

        return new AuthResponse(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.MembershipType,
            user.QuestionnaireCompleted,
            accessToken,
            refreshToken.Token,
            refreshToken.ExpiresAt
        );
    }

    public async Task<GuestLoginResponse> GuestLoginAsync()
    {
        _logger.LogDebug("🔐 AUTH SERVICE: Guest login requested");
        
        var guestUser = new User
        {
            Email = $"guest_{Guid.NewGuid():N}@skinpai.temp",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
            FirstName = "Guest",
            LastName = "User",
            MembershipType = "Guest",
            SecurityStamp = Guid.NewGuid().ToString()
        };

        await _unitOfWork.Users.AddAsync(guestUser);
        await _unitOfWork.SaveChangesAsync();

        var accessToken = GenerateAccessToken(guestUser);
        var expiresAt = DateTime.UtcNow.AddHours(24); // Guest tokens expire in 24 hours

        _logger.LogInformation("✅ AUTH SERVICE: Guest session created | GuestId: {GuestId} | ExpiresAt: {ExpiresAt}", 
            guestUser.UserId, expiresAt);

        return new GuestLoginResponse(
            guestUser.UserId,
            "Guest",
            1, // 1 scan for guests
            accessToken,
            expiresAt
        );
    }

    public async Task<SocialAuthResponse> SocialLoginAsync(SocialLoginRequest request)
    {
        _logger.LogDebug("🔐 AUTH SERVICE: Social login attempt | Provider: {Provider} | Email: {Email}", 
            request.Provider, request.Email ?? "N/A");
        
        // Validate provider
        var validProviders = new[] { "google", "apple", "facebook" };
        if (!validProviders.Contains(request.Provider.ToLower()))
        {
            _logger.LogWarning("⚠️ AUTH SERVICE: Invalid auth provider: {Provider}", request.Provider);
            throw new InvalidOperationException($"Invalid auth provider: {request.Provider}");
        }

        // In production, you would validate the IdToken with the provider
        // For now, we trust the token and extract user info from the request
        // TODO: Add Google/Apple/Facebook token validation

        var email = request.Email?.ToLower() ?? throw new InvalidOperationException("Email is required from social provider");
        
        // Check if user exists with this provider ID
        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => (u.AuthProvider == request.Provider.ToLower() && u.AuthProviderId == request.IdToken) 
                 || u.Email == email);

        bool isNewUser = false;
        User user;

        if (existingUser != null)
        {
            _logger.LogDebug("🔐 AUTH SERVICE: Existing user found for social login | UserId: {UserId} | Provider: {Provider}", 
                existingUser.UserId, request.Provider);
            
            // Existing user - update last login
            user = existingUser;
            user.LastLoginAt = DateTime.UtcNow;
            
            // Update auth provider if not set (e.g., user registered with email, now logging in with social)
            if (string.IsNullOrEmpty(user.AuthProvider))
            {
                user.AuthProvider = request.Provider.ToLower();
                user.AuthProviderId = request.IdToken;
            }
            
            _unitOfWork.Users.Update(user);
        }
        else
        {
            // New user - create account
            isNewUser = true;
            user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Random password for social users
                FirstName = request.FirstName ?? "User",
                LastName = request.LastName ?? "",
                ProfileImageUrl = request.ProfileImageUrl,
                MembershipType = "Member", // Social login users get Member tier
                AuthProvider = request.Provider.ToLower(),
                AuthProviderId = request.IdToken,
                EmailConfirmed = true, // Social login = verified email
                SecurityStamp = Guid.NewGuid().ToString(),
                LastLoginAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
        }

        await _unitOfWork.SaveChangesAsync();

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user.UserId);
        
        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new SocialAuthResponse(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.MembershipType,
            accessToken,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            isNewUser,
            user.QuestionnaireCompleted
        );
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await _unitOfWork.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token == null || !token.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(token.UserId);
        if (user == null || user.IsDeleted)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Revoke old token
        token.RevokedAt = DateTime.UtcNow;
        _unitOfWork.RefreshTokens.Update(token);

        // Generate new tokens
        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken(user.UserId);
        token.ReplacedByToken = newRefreshToken.Token;
        await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.MembershipType,
            user.QuestionnaireCompleted,
            newAccessToken,
            newRefreshToken.Token,
            newRefreshToken.ExpiresAt
        );
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var token = await _unitOfWork.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token != null && token.IsActive)
        {
            token.RevokedAt = DateTime.UtcNow;
            _unitOfWork.RefreshTokens.Update(token);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Secret not configured"));

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Secret not configured"));
        var expiresInMinutes = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("MembershipType", user.MembershipType),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes),
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(Guid userId, string? ipAddress = null)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshTokenDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "30");

        return new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
            CreatedByIp = ipAddress
        };
    }
}
