using Newtonsoft.Json;

namespace ListingService.Models;

public enum ListingStatus { Draft, PendingReview, Active, Inactive, Suspended }
public enum TransmissionType { Manual, Automatic }
public enum FuelType { Petrol, Diesel, Electric, Hybrid }

public class CarListing
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("ownerId")]
    public string OwnerId { get; set; } = string.Empty;

    [JsonProperty("make")]
    public string Make { get; set; } = string.Empty;

    [JsonProperty("model")]
    public string Model { get; set; } = string.Empty;

    [JsonProperty("year")]
    public int Year { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("pricePerDay")]
    public decimal PricePerDay { get; set; }

    [JsonProperty("deposit")]
    public decimal Deposit { get; set; }

    [JsonProperty("location")]
    public GeoLocation Location { get; set; } = new();

    [JsonProperty("address")]
    public string Address { get; set; } = string.Empty;

    [JsonProperty("city")]
    public string City { get; set; } = string.Empty;

    [JsonProperty("country")]
    public string Country { get; set; } = string.Empty;

    [JsonProperty("photos")]
    public List<string> Photos { get; set; } = new();

    [JsonProperty("features")]
    public List<string> Features { get; set; } = new();

    [JsonProperty("transmission")]
    public TransmissionType Transmission { get; set; }

    [JsonProperty("fuelType")]
    public FuelType FuelType { get; set; }

    [JsonProperty("seats")]
    public int Seats { get; set; }

    [JsonProperty("mileageLimit")]
    public int? MileageLimit { get; set; }

    [JsonProperty("status")]
    public ListingStatus Status { get; set; } = ListingStatus.PendingReview;

    [JsonProperty("averageRating")]
    public double AverageRating { get; set; }

    [JsonProperty("totalRatings")]
    public int TotalRatings { get; set; }

    [JsonProperty("blockedDates")]
    public List<BlockedDate> BlockedDates { get; set; } = new();

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}

public class GeoLocation
{
    [JsonProperty("type")]
    public string Type { get; set; } = "Point";

    [JsonProperty("coordinates")]
    public double[] Coordinates { get; set; } = new double[2]; // [longitude, latitude]
}

public class BlockedDate
{
    [JsonProperty("start")]
    public DateTime Start { get; set; }

    [JsonProperty("end")]
    public DateTime End { get; set; }

    [JsonProperty("reason")]
    public string Reason { get; set; } = string.Empty;
}
