using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserService.DTOs;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services;

public interface IUserService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserResponse> GetByIdAsync(Guid id);
    Task<UserResponse> UpdateProfileAsync(Guid id, UpdateProfileRequest request);
    Task ChangePasswordAsync(Guid id, ChangePasswordRequest request);
    Task<bool> VerifyUserAsync(Guid id);
}

public class UserAccountService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IConfiguration _config;

    public UserAccountService(IUserRepository repo, IConfiguration config)
    {
        _repo = repo;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _repo.EmailExistsAsync(request.Email))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Role = request.Role,
        };

        await _repo.AddAsync(user);
        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _repo.GetByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated.");

        return GenerateAuthResponse(user);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var user = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("User not found.");
        return MapToResponse(user);
    }

    public async Task<UserResponse> UpdateProfileAsync(Guid id, UpdateProfileRequest request)
    {
        var user = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("User not found.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.LicenseNumber = request.LicenseNumber;
        user.LicenseExpiry = request.LicenseExpiry;
        user.SetUpdated();

        await _repo.UpdateAsync(user);
        return MapToResponse(user);
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordRequest request)
    {
        var user = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.SetUpdated();
        await _repo.UpdateAsync(user);
    }

    public async Task<bool> VerifyUserAsync(Guid id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return false;
        user.IsVerified = true;
        user.SetUpdated();
        await _repo.UpdateAsync(user);
        return true;
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("fullName", user.FullName),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            Guid.NewGuid().ToString(),
            MapToResponse(user),
            expiry
        );
    }

    private static UserResponse MapToResponse(User u) => new(
        u.Id, u.Email, u.FirstName, u.LastName, u.FullName,
        u.PhoneNumber, u.Role, u.IsVerified, u.ProfileImageUrl,
        u.AverageRating, u.TotalRatings, u.CreatedAt
    );
}
