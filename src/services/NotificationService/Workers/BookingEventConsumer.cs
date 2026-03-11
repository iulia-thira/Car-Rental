using Azure.Messaging.ServiceBus;
using System.Text.Json;
using NotificationService.Services;
using SharedKernel.Events;

namespace NotificationService.Workers;

public class BookingEventConsumer : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingEventConsumer> _logger;
    private ServiceBusProcessor? _confirmedProcessor;
    private ServiceBusProcessor? _cancelledProcessor;

    public BookingEventConsumer(ServiceBusClient client, IServiceScopeFactory scopeFactory,
        ILogger<BookingEventConsumer> logger)
    {
        _client = client;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _confirmedProcessor = _client.CreateProcessor("booking-confirmed");
        _cancelledProcessor = _client.CreateProcessor("booking-cancelled");

        _confirmedProcessor.ProcessMessageAsync += HandleBookingConfirmed;
        _confirmedProcessor.ProcessErrorAsync += HandleError;
        _cancelledProcessor.ProcessMessageAsync += HandleBookingCancelled;
        _cancelledProcessor.ProcessErrorAsync += HandleError;

        await _confirmedProcessor.StartProcessingAsync(stoppingToken);
        await _cancelledProcessor.StartProcessingAsync(stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleBookingConfirmed(ProcessMessageEventArgs args)
    {
        var evt = JsonSerializer.Deserialize<BookingConfirmedEvent>(args.Message.Body.ToString());
        if (evt == null) return;

        using var scope = _scopeFactory.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var carName = $"Car #{evt.CarId.ToString()[..8]}";
        await emailService.SendBookingConfirmedRenterAsync(evt.RenterEmail, "Renter",
            carName, evt.StartDate, evt.EndDate, evt.TotalPrice);
        await emailService.SendBookingConfirmedOwnerAsync(evt.OwnerEmail, "Owner",
            "Renter", carName, evt.StartDate, evt.EndDate);

        await args.CompleteMessageAsync(args.Message);
    }

    private async Task HandleBookingCancelled(ProcessMessageEventArgs args)
    {
        var evt = JsonSerializer.Deserialize<BookingCancelledEvent>(args.Message.Body.ToString());
        if (evt == null) return;

        using var scope = _scopeFactory.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        await emailService.SendBookingCancelledAsync(evt.RenterEmail, "Renter", "your car", evt.Reason);
        await emailService.SendBookingCancelledAsync(evt.OwnerEmail, "Owner", "your car", evt.Reason);

        await args.CompleteMessageAsync(args.Message);
    }

    private Task HandleError(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus processing error");
        return Task.CompletedTask;
    }
}
