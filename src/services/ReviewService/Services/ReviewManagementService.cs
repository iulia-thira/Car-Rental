using Microsoft.EntityFrameworkCore;
using ReviewService.Data;
using ReviewService.DTOs;
using ReviewService.Models;

namespace ReviewService.Services;

public interface IReviewService
{
    Task<ReviewResponse> CreateAsync(Guid authorId, CreateReviewRequest request);
    Task<List<ReviewResponse>> GetForTargetAsync(string targetId, ReviewTargetType type);
    Task<RatingSummary> GetRatingSummaryAsync(string targetId, ReviewTargetType type);
}

public class ReviewManagementService : IReviewService
{
    private readonly ReviewDbContext _context;

    public ReviewManagementService(ReviewDbContext context) => _context = context;

    public async Task<ReviewResponse> CreateAsync(Guid authorId, CreateReviewRequest req)
    {
        if (req.Rating < 1 || req.Rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.");

        var exists = await _context.Reviews.AnyAsync(r =>
            r.BookingId == req.BookingId && r.AuthorId == authorId && r.TargetType == req.TargetType);

        if (exists)
            throw new InvalidOperationException("You have already reviewed this.");

        var review = new Review
        {
            BookingId = req.BookingId,
            AuthorId = authorId,
            TargetId = req.TargetId,
            TargetType = req.TargetType,
            Rating = req.Rating,
            Comment = req.Comment,
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return MapToResponse(review);
    }

    public async Task<List<ReviewResponse>> GetForTargetAsync(string targetId, ReviewTargetType type) =>
        (await _context.Reviews
            .Where(r => r.TargetId == targetId && r.TargetType == type && r.IsVisible)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync()).Select(MapToResponse).ToList();

    public async Task<RatingSummary> GetRatingSummaryAsync(string targetId, ReviewTargetType type)
    {
        var reviews = await _context.Reviews
            .Where(r => r.TargetId == targetId && r.TargetType == type && r.IsVisible)
            .ToListAsync();

        return new RatingSummary(
            targetId,
            reviews.Any() ? reviews.Average(r => r.Rating) : 0,
            reviews.Count);
    }

    private static ReviewResponse MapToResponse(Review r) => new(
        r.Id, r.BookingId, r.AuthorId, r.TargetId, r.TargetType, r.Rating, r.Comment, r.CreatedAt);
}
