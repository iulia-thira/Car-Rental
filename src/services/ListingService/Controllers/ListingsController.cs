using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;
using System.Security.Claims;
using ListingService.DTOs;
using ListingService.Services;

namespace ListingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingsController : ControllerBase
{
    private readonly IListingService _service;
    public ListingsController(IListingService service) => _service = service;

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PagedResult<ListingResponse>>>> Search([FromQuery] SearchListingsRequest request)
    {
        var (items, total) = await _service.SearchAsync(request);
        var result = new PagedResult<ListingResponse>
        {
            Items = items, TotalCount = total,
            Page = request.Page, PageSize = request.PageSize
        };
        return Ok(ApiResponse<PagedResult<ListingResponse>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ListingResponse>>> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null
            ? NotFound(ApiResponse<ListingResponse>.Fail("Listing not found."))
            : Ok(ApiResponse<ListingResponse>.Ok(result));
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<List<ListingResponse>>>> GetMyListings()
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.GetByOwnerAsync(ownerId);
        return Ok(ApiResponse<List<ListingResponse>>.Ok(result));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ListingResponse>>> Create([FromBody] CreateListingRequest request)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.CreateAsync(ownerId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ListingResponse>.Ok(result));
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ListingResponse>>> Update(string id, [FromBody] UpdateListingRequest request)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _service.UpdateAsync(id, ownerId, request);
        return Ok(ApiResponse<ListingResponse>.Ok(result));
    }

    [Authorize(Roles = "Owner")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.DeleteAsync(id, ownerId);
        return NoContent();
    }

    [Authorize(Roles = "Owner")]
    [HttpPost("{id}/photos")]
    public async Task<ActionResult<ApiResponse<string>>> UploadPhoto(string id, IFormFile photo)
    {
        var url = await _service.UploadPhotoAsync(id, photo);
        return Ok(ApiResponse<string>.Ok(url));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost("{id}/block-dates")]
    public async Task<ActionResult> BlockDates(string id, [FromBody] BlockDatesRequest request)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _service.BlockDatesAsync(id, ownerId, request);
        return Ok();
    }
}
