using Microsoft.EntityFrameworkCore;
using Stripe;
using PaymentService.Data;
using PaymentService.DTOs;
using PaymentService.Models;

namespace PaymentService.Services;

public interface IPaymentService
{
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(Guid renterId, CreatePaymentIntentRequest request);
    Task<PaymentResponse> ConfirmPaymentAsync(ConfirmPaymentRequest request);
    Task<PaymentResponse> RefundAsync(RefundRequest request);
    Task<PaymentResponse?> GetByBookingIdAsync(Guid bookingId);
}

public class StripePaymentService : IPaymentService
{
    private readonly PaymentDbContext _context;
    private readonly IConfiguration _config;
    private const decimal PlatformFeePercent = 0.20m; // 20% commission

    public StripePaymentService(PaymentDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
        StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
    }

    public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(Guid renterId, CreatePaymentIntentRequest req)
    {
        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (long)((req.Amount + req.Deposit) * 100),
            Currency = req.Currency,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
            Metadata = new Dictionary<string, string>
            {
                { "bookingId", req.BookingId.ToString() },
                { "renterId", renterId.ToString() }
            }
        });

        var payment = new Payment
        {
            BookingId = req.BookingId,
            RenterId = renterId,
            OwnerId = req.OwnerId,
            Amount = req.Amount,
            Deposit = req.Deposit,
            PlatformFee = req.Amount * PlatformFeePercent,
            OwnerPayout = req.Amount * (1 - PlatformFeePercent),
            StripePaymentIntentId = intent.Id,
            Status = PaymentStatus.Pending
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return new PaymentIntentResponse(intent.ClientSecret!, intent.Id, req.Amount + req.Deposit);
    }

    public async Task<PaymentResponse> ConfirmPaymentAsync(ConfirmPaymentRequest req)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == req.PaymentIntentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        payment.Status = PaymentStatus.Completed;
        payment.SetUpdated();
        await _context.SaveChangesAsync();
        return MapToResponse(payment);
    }

    public async Task<PaymentResponse> RefundAsync(RefundRequest req)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == req.BookingId)
            ?? throw new KeyNotFoundException("Payment not found.");

        var refundAmount = req.Amount ?? payment.Amount + payment.Deposit;

        var service = new RefundService();
        var refund = await service.CreateAsync(new RefundCreateOptions
        {
            PaymentIntent = payment.StripePaymentIntentId,
            Amount = (long)(refundAmount * 100),
            Reason = "requested_by_customer"
        });

        payment.RefundId = refund.Id;
        payment.RefundAmount = refundAmount;
        payment.Status = req.Amount.HasValue && req.Amount < (payment.Amount + payment.Deposit)
            ? PaymentStatus.PartiallyRefunded
            : PaymentStatus.Refunded;
        payment.SetUpdated();
        await _context.SaveChangesAsync();
        return MapToResponse(payment);
    }

    public async Task<PaymentResponse?> GetByBookingIdAsync(Guid bookingId)
    {
        var p = await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);
        return p == null ? null : MapToResponse(p);
    }

    private static PaymentResponse MapToResponse(Payment p) => new(
        p.Id, p.BookingId, p.Amount, p.Deposit, p.PlatformFee,
        p.OwnerPayout, p.Status, p.StripePaymentIntentId, p.CreatedAt);
}
