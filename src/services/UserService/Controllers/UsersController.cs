using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;
using System.Security.Claims;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service) => _service = service;

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _service.RegisterAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Registration successful."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _service.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetMe()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.GetByIdAsync(userId);
        return Ok(ApiResponse<UserResponse>.Ok(result));
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<UserResponse>.Ok(result));
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.UpdateProfileAsync(userId, request);
        return Ok(ApiResponse<UserResponse>.Ok(result));
    }

    [Authorize]
    [HttpPost("me/change-password")]
    public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.ChangePasswordAsync(userId, request);
        return Ok(ApiResponse<string>.Ok("Password changed successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/verify")]
    public async Task<ActionResult<ApiResponse<string>>> VerifyUser(Guid id)
    {
        var success = await _service.VerifyUserAsync(id);
        return success
            ? Ok(ApiResponse<string>.Ok("User verified."))
            : NotFound(ApiResponse<string>.Fail("User not found."));
    }
}
