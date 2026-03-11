using UserService.Models;

namespace UserService.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    UserRole Role
);

public record LoginRequest(string Email, string Password);

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? LicenseNumber,
    DateTime? LicenseExpiry
);

public record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string? PhoneNumber,
    UserRole Role,
    bool IsVerified,
    string? ProfileImageUrl,
    double AverageRating,
    int TotalRatings,
    DateTime CreatedAt
);

public record AuthResponse(string Token, string RefreshToken, UserResponse User, DateTime ExpiresAt);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
