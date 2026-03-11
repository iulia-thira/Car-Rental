using Azure.Messaging.ServiceBus;
using SendGrid.Extensions.DependencyInjection;
using Serilog;
using NotificationService.Services;
using NotificationService.Workers;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).WriteTo.Console().CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddSendGrid(o => o.ApiKey = builder.Configuration["SendGrid:ApiKey"]!);
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
builder.Services.AddSingleton(s => new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));
builder.Services.AddHostedService<BookingEventConsumer>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapControllers();
app.Run();
