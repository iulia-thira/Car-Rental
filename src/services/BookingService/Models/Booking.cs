using SharedKernel.Models;

namespace BookingService.Models;

public enum BookingStatus
{
    Pending,
    Confirmed,
    Active,
    Completed,
    CancelledByRenter,
    CancelledByOwner,
    Disputed
}

public class Booking : BaseEntity
{
    public string CarId { get; set; } = string.Empty;
    public Guid RenterId { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays => (EndDate - StartDate).Days;
    public decimal PricePerDay { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal Deposit { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? CancellationReason { get; set; }
    public string? SpecialRequests { get; set; }
    public DateTime? PickupConfirmedAt { get; set; }
    public DateTime? ReturnConfirmedAt { get; set; }
    public string RenterEmail { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
}
