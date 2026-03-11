using PaymentService.Models;

namespace PaymentService.DTOs;

public record CreatePaymentIntentRequest(
    Guid BookingId,
    Guid OwnerId,
    decimal Amount,
    decimal Deposit,
    string Currency = "usd"
);

public record ConfirmPaymentRequest(string PaymentIntentId);

public record RefundRequest(Guid BookingId, decimal? Amount, string Reason);

public record PaymentResponse(
    Guid Id,
    Guid BookingId,
    decimal Amount,
    decimal Deposit,
    decimal PlatformFee,
    decimal OwnerPayout,
    PaymentStatus Status,
    string? StripePaymentIntentId,
    DateTime CreatedAt
);

public record PaymentIntentResponse(string ClientSecret, string PaymentIntentId, decimal Amount);
