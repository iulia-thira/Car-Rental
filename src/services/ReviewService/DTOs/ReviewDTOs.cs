using ReviewService.Models;

namespace ReviewService.DTOs;

public record CreateReviewRequest(
    Guid BookingId,
    string TargetId,
    ReviewTargetType TargetType,
    int Rating,
    string Comment
);

public record ReviewResponse(
    Guid Id,
    Guid BookingId,
    Guid AuthorId,
    string TargetId,
    ReviewTargetType TargetType,
    int Rating,
    string Comment,
    DateTime CreatedAt
);

public record RatingSummary(string TargetId, double AverageRating, int TotalReviews);
