namespace SkinPAI.API.Models.DTOs;

// ==================== Auth DTOs ====================
public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    string? DateOfBirth = null,
    string? Gender = null
);

public record LoginRequest(
    string Email,
    string Password
);

// Social Login DTOs
public record SocialLoginRequest(
    string Provider,        // "google", "apple", "facebook"
    string IdToken,         // OAuth ID token from provider
    string? AccessToken = null,
    string? FirstName = null,
    string? LastName = null,
    string? Email = null,
    string? ProfileImageUrl = null
);

public record SocialAuthResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string MembershipType,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    bool IsNewUser,
    bool QuestionnaireCompleted
);

public record AuthResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string MembershipType,
    bool QuestionnaireCompleted,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);

public record RefreshTokenRequest(string RefreshToken);

public record GuestLoginResponse(
    Guid UserId,
    string MembershipType,
    int MaxScans,
    string AccessToken,
    DateTime ExpiresAt
);

// ==================== User DTOs ====================
public record UserDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? ProfileImageUrl,
    string MembershipType,
    string MembershipStatus,
    DateTime? MembershipStartDate,
    DateTime? MembershipEndDate,
    decimal WalletBalance,
    int TotalScansUsed,
    bool IsVerified,
    bool IsCreator,
    bool QuestionnaireCompleted,
    SkinProfileDto? SkinProfile,
    DateTime CreatedAt
);

public record UpdateUserRequest(
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null,
    DateOnly? DateOfBirth = null,
    string? Gender = null,
    string? Bio = null,
    string? ProfileImageUrl = null
);

public record UpdateProfileImageRequest(string ImageBase64);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

// ==================== SkinProfile DTOs ====================
public record SkinProfileDto(
    string? SkinType,
    string? SkinConcerns,
    string? CurrentRoutine,
    string? SunExposure,
    string? Lifestyle,
    bool QuestionnaireCompleted
);

public record UpdateSkinProfileRequest(
    string? SkinType,
    string? SkinConcerns,
    string? CurrentRoutine,
    string? SunExposure,
    string? Lifestyle
);
