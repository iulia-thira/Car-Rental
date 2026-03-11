using Azure.Storage.Blobs;
using ListingService.DTOs;
using ListingService.Models;
using ListingService.Repositories;

namespace ListingService.Services;

public interface IListingService
{
    Task<ListingResponse> CreateAsync(string ownerId, CreateListingRequest request);
    Task<ListingResponse?> GetByIdAsync(string id);
    Task<(List<ListingResponse> Items, int Total)> SearchAsync(SearchListingsRequest request);
    Task<List<ListingResponse>> GetByOwnerAsync(string ownerId);
    Task<ListingResponse> UpdateAsync(string id, string ownerId, UpdateListingRequest request);
    Task DeleteAsync(string id, string ownerId);
    Task<string> UploadPhotoAsync(string id, IFormFile photo);
    Task BlockDatesAsync(string id, string ownerId, BlockDatesRequest request);
}

public class ListingManagementService : IListingService
{
    private readonly IListingRepository _repo;
    private readonly BlobServiceClient _blobClient;
    private readonly IConfiguration _config;

    public ListingManagementService(IListingRepository repo, BlobServiceClient blobClient, IConfiguration config)
    {
        _repo = repo;
        _blobClient = blobClient;
        _config = config;
    }

    public async Task<ListingResponse> CreateAsync(string ownerId, CreateListingRequest req)
    {
        var listing = new CarListing
        {
            OwnerId = ownerId,
            Make = req.Make,
            Model = req.Model,
            Year = req.Year,
            Description = req.Description,
            PricePerDay = req.PricePerDay,
            Deposit = req.Deposit,
            Location = new GeoLocation { Coordinates = new[] { req.Longitude, req.Latitude } },
            Address = req.Address,
            City = req.City,
            Country = req.Country,
            Features = req.Features,
            Transmission = req.Transmission,
            FuelType = req.FuelType,
            Seats = req.Seats,
            MileageLimit = req.MileageLimit,
            Status = ListingStatus.PendingReview,
        };

        var created = await _repo.CreateAsync(listing);
        return MapToResponse(created);
    }

    public async Task<ListingResponse?> GetByIdAsync(string id)
    {
        var listing = await _repo.GetByIdAsync(id);
        return listing == null ? null : MapToResponse(listing);
    }

    public async Task<(List<ListingResponse> Items, int Total)> SearchAsync(SearchListingsRequest req)
    {
        var (items, total) = await _repo.SearchAsync(
            req.City, req.Latitude, req.Longitude, req.RadiusKm,
            req.StartDate, req.EndDate, req.MinPrice, req.MaxPrice,
            req.Page, req.PageSize);

        return (items.Select(MapToResponse).ToList(), total);
    }

    public async Task<List<ListingResponse>> GetByOwnerAsync(string ownerId)
    {
        var listings = await _repo.GetByOwnerAsync(ownerId);
        return listings.Select(MapToResponse).ToList();
    }

    public async Task<ListingResponse> UpdateAsync(string id, string ownerId, UpdateListingRequest req)
    {
        var listing = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Listing not found.");

        if (listing.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Not authorized to update this listing.");

        listing.Description = req.Description;
        listing.PricePerDay = req.PricePerDay;
        listing.Deposit = req.Deposit;
        listing.Features = req.Features;
        listing.MileageLimit = req.MileageLimit;

        var updated = await _repo.UpdateAsync(listing);
        return MapToResponse(updated);
    }

    public async Task DeleteAsync(string id, string ownerId)
    {
        var listing = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Listing not found.");

        if (listing.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Not authorized.");

        await _repo.DeleteAsync(id);
    }

    public async Task<string> UploadPhotoAsync(string id, IFormFile photo)
    {
        var listing = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Listing not found.");

        var container = _blobClient.GetBlobContainerClient("car-photos");
        await container.CreateIfNotExistsAsync();

        var blobName = $"{id}/{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
        var blob = container.GetBlobClient(blobName);

        using var stream = photo.OpenReadStream();
        await blob.UploadAsync(stream, overwrite: true);

        var url = blob.Uri.ToString();
        listing.Photos.Add(url);
        await _repo.UpdateAsync(listing);
        return url;
    }

    public async Task BlockDatesAsync(string id, string ownerId, BlockDatesRequest req)
    {
        var listing = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Listing not found.");

        if (listing.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Not authorized.");

        listing.BlockedDates.Add(new BlockedDate { Start = req.Start, End = req.End, Reason = req.Reason });
        await _repo.UpdateAsync(listing);
    }

    private static ListingResponse MapToResponse(CarListing l) => new(
        l.Id, l.OwnerId, l.Make, l.Model, l.Year, l.Description,
        l.PricePerDay, l.Deposit,
        l.Location.Coordinates[1], l.Location.Coordinates[0],
        l.Address, l.City, l.Country,
        l.Photos, l.Features, l.Transmission, l.FuelType,
        l.Seats, l.MileageLimit, l.Status,
        l.AverageRating, l.TotalRatings, l.CreatedAt
    );
}
