using SharedKernel.Models;

namespace UserService.Models;

public enum UserRole { Renter, Owner, Admin }

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ProfileImageUrl { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiry { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public string FullName => $"{FirstName} {LastName}";
}
