// Models/Payment.cs
using SharedKernel.Models;

namespace PaymentService.Models;

public enum PaymentStatus { Pending, Completed, Failed, Refunded, PartiallyRefunded }

public class Payment : BaseEntity
{
    public Guid BookingId { get; set; }
    public Guid RenterId { get; set; }
    public Guid OwnerId { get; set; }
    public decimal Amount { get; set; }
    public decimal Deposit { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal OwnerPayout { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? StripePaymentIntentId { get; set; }
    public string? StripeTransferId { get; set; }
    public string? RefundId { get; set; }
    public decimal? RefundAmount { get; set; }
}
