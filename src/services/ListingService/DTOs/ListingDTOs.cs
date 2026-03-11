using ListingService.Models;

namespace ListingService.DTOs;

public record CreateListingRequest(
    string Make,
    string Model,
    int Year,
    string Description,
    decimal PricePerDay,
    decimal Deposit,
    double Latitude,
    double Longitude,
    string Address,
    string City,
    string Country,
    List<string> Features,
    TransmissionType Transmission,
    FuelType FuelType,
    int Seats,
    int? MileageLimit
);

public record UpdateListingRequest(
    string Description,
    decimal PricePerDay,
    decimal Deposit,
    List<string> Features,
    int? MileageLimit
);

public record SearchListingsRequest(
    string? City,
    double? Latitude,
    double? Longitude,
    double? RadiusKm,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? MinPrice,
    decimal? MaxPrice,
    TransmissionType? Transmission,
    FuelType? FuelType,
    int? MinSeats,
    int Page = 1,
    int PageSize = 20
);

public record BlockDatesRequest(DateTime Start, DateTime End, string Reason = "");

public record ListingResponse(
    string Id,
    string OwnerId,
    string Make,
    string Model,
    int Year,
    string Description,
    decimal PricePerDay,
    decimal Deposit,
    double Latitude,
    double Longitude,
    string Address,
    string City,
    string Country,
    List<string> Photos,
    List<string> Features,
    TransmissionType Transmission,
    FuelType FuelType,
    int Seats,
    int? MileageLimit,
    ListingStatus Status,
    double AverageRating,
    int TotalRatings,
    DateTime CreatedAt
);
