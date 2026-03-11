using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;
using System.Security.Claims;
using ReviewService.DTOs;
using ReviewService.Models;
using ReviewService.Services;

namespace ReviewService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _service;
    public ReviewsController(IReviewService service) => _service = service;

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> Create([FromBody] CreateReviewRequest request)
    {
        var authorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.CreateAsync(authorId, request);
        return Ok(ApiResponse<ReviewResponse>.Ok(result));
    }

    [HttpGet("car/{carId}")]
    public async Task<ActionResult<ApiResponse<List<ReviewResponse>>>> GetCarReviews(string carId)
    {
        var result = await _service.GetForTargetAsync(carId, ReviewTargetType.Car);
        return Ok(ApiResponse<List<ReviewResponse>>.Ok(result));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<List<ReviewResponse>>>> GetUserReviews(string userId)
    {
        var result = await _service.GetForTargetAsync(userId, ReviewTargetType.User);
        return Ok(ApiResponse<List<ReviewResponse>>.Ok(result));
    }

    [HttpGet("summary/car/{carId}")]
    public async Task<ActionResult<ApiResponse<RatingSummary>>> GetCarRatingSummary(string carId)
    {
        var result = await _service.GetRatingSummaryAsync(carId, ReviewTargetType.Car);
        return Ok(ApiResponse<RatingSummary>.Ok(result));
    }
}
