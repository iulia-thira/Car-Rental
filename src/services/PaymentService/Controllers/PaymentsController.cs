using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;
using System.Security.Claims;
using PaymentService.DTOs;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;
    public PaymentsController(IPaymentService service) => _service = service;
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("intent")]
    public async Task<ActionResult<ApiResponse<PaymentIntentResponse>>> CreateIntent([FromBody] CreatePaymentIntentRequest req)
    {
        var result = await _service.CreatePaymentIntentAsync(CurrentUserId, req);
        return Ok(ApiResponse<PaymentIntentResponse>.Ok(result));
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> Confirm([FromBody] ConfirmPaymentRequest req)
    {
        var result = await _service.ConfirmPaymentAsync(req);
        return Ok(ApiResponse<PaymentResponse>.Ok(result));
    }

    [HttpPost("refund")]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> Refund([FromBody] RefundRequest req)
    {
        var result = await _service.RefundAsync(req);
        return Ok(ApiResponse<PaymentResponse>.Ok(result));
    }

    [HttpGet("booking/{bookingId:guid}")]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> GetByBooking(Guid bookingId)
    {
        var result = await _service.GetByBookingIdAsync(bookingId);
        return result == null
            ? NotFound(ApiResponse<PaymentResponse>.Fail("Payment not found."))
            : Ok(ApiResponse<PaymentResponse>.Ok(result));
    }
}
