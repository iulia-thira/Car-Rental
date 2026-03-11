namespace SharedKernel.Events;

public record BookingConfirmedEvent(
    Guid BookingId,
    Guid CarId,
    Guid RenterId,
    Guid OwnerId,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalPrice,
    string RenterEmail,
    string OwnerEmail
);

public record BookingCancelledEvent(
    Guid BookingId,
    Guid RenterId,
    Guid OwnerId,
    string Reason,
    string RenterEmail,
    string OwnerEmail
);

public record PaymentProcessedEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Status
);

public record PaymentRefundedEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount
);

public record CarListingCreatedEvent(
    Guid ListingId,
    Guid OwnerId,
    string Make,
    string Model,
    decimal PricePerDay
);

public record UserVerifiedEvent(
    Guid UserId,
    string Email,
    string FullName
);
