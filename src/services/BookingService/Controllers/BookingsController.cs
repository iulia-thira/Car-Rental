using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;
using System.Security.Claims;
using BookingService.DTOs;
using BookingService.Services;

namespace BookingService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;
    public BookingsController(IBookingService service) => _service = service;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Create([FromBody] CreateBookingRequest request)
    {
        var result = await _service.CreateAsync(CurrentUserId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BookingResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null
            ? NotFound(ApiResponse<BookingResponse>.Fail("Booking not found."))
            : Ok(ApiResponse<BookingResponse>.Ok(result));
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<List<BookingResponse>>>> GetMyBookings()
    {
        var result = await _service.GetByRenterAsync(CurrentUserId);
        return Ok(ApiResponse<List<BookingResponse>>.Ok(result));
    }

    [HttpGet("owner")]
    public async Task<ActionResult<ApiResponse<List<BookingResponse>>>> GetOwnerBookings()
    {
        var result = await _service.GetByOwnerAsync(CurrentUserId);
        return Ok(ApiResponse<List<BookingResponse>>.Ok(result));
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Confirm(Guid id)
    {
        var result = await _service.ConfirmAsync(id, CurrentUserId);
        return Ok(ApiResponse<BookingResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Cancel(Guid id, [FromBody] CancelBookingRequest request)
    {
        var result = await _service.CancelAsync(id, CurrentUserId, request);
        return Ok(ApiResponse<BookingResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/confirm-pickup")]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> ConfirmPickup(Guid id)
    {
        var result = await _service.ConfirmPickupAsync(id, CurrentUserId);
        return Ok(ApiResponse<BookingResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/confirm-return")]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> ConfirmReturn(Guid id)
    {
        var result = await _service.ConfirmReturnAsync(id, CurrentUserId);
        return Ok(ApiResponse<BookingResponse>.Ok(result));
    }
}
