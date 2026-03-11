using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using ListingService.Models;

namespace ListingService.Repositories;

public interface IListingRepository
{
    Task<CarListing?> GetByIdAsync(string id);
    Task<(List<CarListing> Items, int Total)> SearchAsync(string? city, double? lat, double? lng, double? radiusKm,
        DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice, int page, int pageSize);
    Task<List<CarListing>> GetByOwnerAsync(string ownerId);
    Task<CarListing> CreateAsync(CarListing listing);
    Task<CarListing> UpdateAsync(CarListing listing);
    Task DeleteAsync(string id);
}

public class ListingRepository : IListingRepository
{
    private readonly Container _container;

    public ListingRepository(CosmosClient client, IConfiguration config)
    {
        var db = client.GetDatabase(config["Cosmos:Database"]);
        _container = db.GetContainer("Listings");
    }

    public async Task<CarListing?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<CarListing>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<(List<CarListing> Items, int Total)> SearchAsync(
        string? city, double? lat, double? lng, double? radiusKm,
        DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice,
        int page, int pageSize)
    {
        var query = _container.GetItemLinqQueryable<CarListing>(true)
            .Where(l => l.Status == ListingStatus.Active);

        if (!string.IsNullOrEmpty(city))
            query = query.Where(l => l.City.ToLower() == city.ToLower());

        if (minPrice.HasValue) query = query.Where(l => l.PricePerDay >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(l => l.PricePerDay <= maxPrice.Value);

        var total = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        // Filter out blocked dates
        if (startDate.HasValue && endDate.HasValue)
        {
            items = items.Where(l => !l.BlockedDates.Any(b =>
                b.Start <= endDate.Value && b.End >= startDate.Value)).ToList();
        }

        return (items, total);
    }

    public async Task<List<CarListing>> GetByOwnerAsync(string ownerId)
    {
        var query = _container.GetItemLinqQueryable<CarListing>(true)
            .Where(l => l.OwnerId == ownerId).ToList();
        return query;
    }

    public async Task<CarListing> CreateAsync(CarListing listing)
    {
        var response = await _container.CreateItemAsync(listing, new PartitionKey(listing.Id));
        return response.Resource;
    }

    public async Task<CarListing> UpdateAsync(CarListing listing)
    {
        listing.UpdatedAt = DateTime.UtcNow;
        var response = await _container.UpsertItemAsync(listing, new PartitionKey(listing.Id));
        return response.Resource;
    }

    public async Task DeleteAsync(string id)
    {
        var listing = await GetByIdAsync(id);
        if (listing != null)
        {
            listing.Status = ListingStatus.Inactive;
            await UpdateAsync(listing);
        }
    }
}
