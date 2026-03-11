using SharedKernel.Models;

namespace ReviewService.Models;

public enum ReviewTargetType { Car, User }

public class Review : BaseEntity
{
    public Guid BookingId { get; set; }
    public Guid AuthorId { get; set; }
    public string TargetId { get; set; } = string.Empty; // Car ID or User ID
    public ReviewTargetType TargetType { get; set; }
    public int Rating { get; set; } // 1-5
    public string Comment { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
}
