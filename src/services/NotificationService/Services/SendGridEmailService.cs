using SendGrid;
using SendGrid.Helpers.Mail;

namespace NotificationService.Services;

public interface IEmailService
{
    Task SendBookingConfirmedRenterAsync(string to, string renterName, string carName, DateTime start, DateTime end, decimal total);
    Task SendBookingConfirmedOwnerAsync(string to, string ownerName, string renterName, string carName, DateTime start, DateTime end);
    Task SendBookingCancelledAsync(string to, string name, string carName, string reason);
    Task SendGenericAsync(string to, string name, string subject, string body);
}

public class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _client;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailService(ISendGridClient client, IConfiguration config)
    {
        _client = client;
        _fromEmail = config["SendGrid:FromEmail"]!;
        _fromName = config["SendGrid:FromName"] ?? "DriveShare";
    }

    public async Task SendBookingConfirmedRenterAsync(string to, string renterName, string carName,
        DateTime start, DateTime end, decimal total)
    {
        var subject = $"Your booking for {carName} is confirmed!";
        var body = $@"
            <h2>Booking Confirmed 🚗</h2>
            <p>Hi {renterName},</p>
            <p>Great news! Your booking for <strong>{carName}</strong> has been confirmed.</p>
            <ul>
                <li><strong>Pickup:</strong> {start:dddd, MMMM d yyyy}</li>
                <li><strong>Return:</strong> {end:dddd, MMMM d yyyy}</li>
                <li><strong>Total paid:</strong> ${total:F2}</li>
            </ul>
            <p>Have a great trip!</p>
            <p>– The DriveShare Team</p>";

        await SendAsync(to, renterName, subject, body);
    }

    public async Task SendBookingConfirmedOwnerAsync(string to, string ownerName, string renterName,
        string carName, DateTime start, DateTime end)
    {
        var subject = $"New booking for your {carName}";
        var body = $@"
            <h2>New Booking 📅</h2>
            <p>Hi {ownerName},</p>
            <p><strong>{renterName}</strong> has booked your <strong>{carName}</strong>.</p>
            <ul>
                <li><strong>Pickup:</strong> {start:dddd, MMMM d yyyy}</li>
                <li><strong>Return:</strong> {end:dddd, MMMM d yyyy}</li>
            </ul>
            <p>Log in to your dashboard to manage the booking.</p>
            <p>– The DriveShare Team</p>";

        await SendAsync(to, ownerName, subject, body);
    }

    public async Task SendBookingCancelledAsync(string to, string name, string carName, string reason)
    {
        var subject = $"Booking for {carName} has been cancelled";
        var body = $@"
            <h2>Booking Cancelled</h2>
            <p>Hi {name},</p>
            <p>Your booking for <strong>{carName}</strong> has been cancelled.</p>
            <p><strong>Reason:</strong> {reason}</p>
            <p>If you have any questions, please contact support.</p>
            <p>– The DriveShare Team</p>";

        await SendAsync(to, name, subject, body);
    }

    public async Task SendGenericAsync(string to, string name, string subject, string body) =>
        await SendAsync(to, name, subject, body);

    private async Task SendAsync(string to, string toName, string subject, string htmlContent)
    {
        var msg = MailHelper.CreateSingleEmail(
            new EmailAddress(_fromEmail, _fromName),
            new EmailAddress(to, toName),
            subject, null, htmlContent);

        var response = await _client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"SendGrid error: {response.StatusCode}");
    }
}
