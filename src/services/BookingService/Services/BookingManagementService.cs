using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BookingService.Data;
using BookingService.DTOs;
using BookingService.Models;
using SharedKernel.Events;

namespace BookingService.Services;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(Guid renterId, CreateBookingRequest request);
    Task<BookingResponse?> GetByIdAsync(Guid id);
    Task<List<BookingResponse>> GetByRenterAsync(Guid renterId);
    Task<List<BookingResponse>> GetByOwnerAsync(Guid ownerId);
    Task<BookingResponse> ConfirmAsync(Guid id, Guid ownerId);
    Task<BookingResponse> CancelAsync(Guid id, Guid userId, CancelBookingRequest request);
    Task<BookingResponse> ConfirmPickupAsync(Guid id, Guid ownerId);
    Task<BookingResponse> ConfirmReturnAsync(Guid id, Guid ownerId);
}

public class BookingManagementService : IBookingService
{
    private readonly BookingDbContext _context;
    private readonly ServiceBusClient _sbClient;
    private readonly IConfiguration _config;

    public BookingManagementService(BookingDbContext context, ServiceBusClient sbClient, IConfiguration config)
    {
        _context = context;
        _sbClient = sbClient;
        _config = config;
    }

    public async Task<BookingResponse> CreateAsync(Guid renterId, CreateBookingRequest req)
    {
        // Check for conflicts
        var conflict = await _context.Bookings.AnyAsync(b =>
            b.CarId == req.CarId &&
            b.Status != BookingStatus.CancelledByRenter &&
            b.Status != BookingStatus.CancelledByOwner &&
            b.StartDate < req.EndDate && b.EndDate > req.StartDate);

        if (conflict)
            throw new InvalidOperationException("Car is not available for the selected dates.");

        var days = (req.EndDate - req.StartDate).Days;
        var booking = new Booking
        {
            CarId = req.CarId,
            RenterId = renterId,
            OwnerId = req.OwnerId,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            PricePerDay = req.PricePerDay,
            TotalPrice = req.PricePerDay * days,
            Deposit = req.Deposit,
            SpecialRequests = req.SpecialRequests,
            Status = BookingStatus.Pending,
            RenterEmail = req.RenterEmail,
            OwnerEmail = req.OwnerEmail,
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return MapToResponse(booking);
    }

    public async Task<BookingResponse?> GetByIdAsync(Guid id)
    {
        var b = await _context.Bookings.FindAsync(id);
        return b == null ? null : MapToResponse(b);
    }

    public async Task<List<BookingResponse>> GetByRenterAsync(Guid renterId) =>
        (await _context.Bookings.Where(b => b.RenterId == renterId)
            .OrderByDescending(b => b.CreatedAt).ToListAsync()).Select(MapToResponse).ToList();

    public async Task<List<BookingResponse>> GetByOwnerAsync(Guid ownerId) =>
        (await _context.Bookings.Where(b => b.OwnerId == ownerId)
            .OrderByDescending(b => b.CreatedAt).ToListAsync()).Select(MapToResponse).ToList();

    public async Task<BookingResponse> ConfirmAsync(Guid id, Guid ownerId)
    {
        var booking = await GetBookingOrThrow(id);
        if (booking.OwnerId != ownerId) throw new UnauthorizedAccessException();
        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Booking is not in pending state.");

        booking.Status = BookingStatus.Confirmed;
        booking.SetUpdated();
        await _context.SaveChangesAsync();

        await PublishEventAsync("booking-confirmed", new BookingConfirmedEvent(
            booking.Id, Guid.Parse(booking.CarId), booking.RenterId, booking.OwnerId,
            booking.StartDate, booking.EndDate, booking.TotalPrice,
            booking.RenterEmail, booking.OwnerEmail));

        return MapToResponse(booking);
    }

    public async Task<BookingResponse> CancelAsync(Guid id, Guid userId, CancelBookingRequest req)
    {
        var booking = await GetBookingOrThrow(id);
        var isRenter = booking.RenterId == userId;
        var isOwner = booking.OwnerId == userId;
        if (!isRenter && !isOwner) throw new UnauthorizedAccessException();

        booking.Status = isRenter ? BookingStatus.CancelledByRenter : BookingStatus.CancelledByOwner;
        booking.CancellationReason = req.Reason;
        booking.SetUpdated();
        await _context.SaveChangesAsync();

        await PublishEventAsync("booking-cancelled", new BookingCancelledEvent(
            booking.Id, booking.RenterId, booking.OwnerId,
            req.Reason, booking.RenterEmail, booking.OwnerEmail));

        return MapToResponse(booking);
    }

    public async Task<BookingResponse> ConfirmPickupAsync(Guid id, Guid ownerId)
    {
        var booking = await GetBookingOrThrow(id);
        if (booking.OwnerId != ownerId) throw new UnauthorizedAccessException();
        booking.Status = BookingStatus.Active;
        booking.PickupConfirmedAt = DateTime.UtcNow;
        booking.SetUpdated();
        await _context.SaveChangesAsync();
        return MapToResponse(booking);
    }

    public async Task<BookingResponse> ConfirmReturnAsync(Guid id, Guid ownerId)
    {
        var booking = await GetBookingOrThrow(id);
        if (booking.OwnerId != ownerId) throw new UnauthorizedAccessException();
        booking.Status = BookingStatus.Completed;
        booking.ReturnConfirmedAt = DateTime.UtcNow;
        booking.SetUpdated();
        await _context.SaveChangesAsync();
        return MapToResponse(booking);
    }

    private async Task<Booking> GetBookingOrThrow(Guid id) =>
        await _context.Bookings.FindAsync(id) ?? throw new KeyNotFoundException("Booking not found.");

    private async Task PublishEventAsync<T>(string topic, T evt)
    {
        var sender = _sbClient.CreateSender(topic);
        var message = new ServiceBusMessage(JsonSerializer.Serialize(evt));
        await sender.SendMessageAsync(message);
    }

    private static BookingResponse MapToResponse(Booking b) => new(
        b.Id, b.CarId, b.RenterId, b.OwnerId, b.StartDate, b.EndDate,
        b.TotalDays, b.PricePerDay, b.TotalPrice, b.Deposit, b.Status,
        b.SpecialRequests, b.CancellationReason, b.PickupConfirmedAt,
        b.ReturnConfirmedAt, b.CreatedAt);
}
