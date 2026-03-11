using BookingService.Models;

namespace BookingService.DTOs;

public record CreateBookingRequest(
    string CarId,
    Guid OwnerId,
    DateTime StartDate,
    DateTime EndDate,
    decimal PricePerDay,
    decimal Deposit,
    string? SpecialRequests,
    string RenterEmail,
    string OwnerEmail
);

public record CancelBookingRequest(string Reason);

public record BookingResponse(
    Guid Id,
    string CarId,
    Guid RenterId,
    Guid OwnerId,
    DateTime StartDate,
    DateTime EndDate,
    int TotalDays,
    decimal PricePerDay,
    decimal TotalPrice,
    decimal Deposit,
    BookingStatus Status,
    string? SpecialRequests,
    string? CancellationReason,
    DateTime? PickupConfirmedAt,
    DateTime? ReturnConfirmedAt,
    DateTime CreatedAt
);
