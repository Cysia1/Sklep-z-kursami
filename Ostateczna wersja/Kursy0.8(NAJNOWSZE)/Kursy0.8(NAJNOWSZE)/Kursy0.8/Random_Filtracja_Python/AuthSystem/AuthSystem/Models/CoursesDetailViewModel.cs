using Stripe;
using static AuthSystem.Entities.Database;
namespace AuthSystem.Models
{
    public class CoursesDetailViewModel
    {

        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public string? CourseType { get; set; }
        public decimal CoursePrice { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? ThumbnailUrlForUpdate { get; set; }
        public string? VideoUrl { get; set; }
        public IFormFile? ThumbnailFile { get; set; }
        public byte[]? PdfFile { get; set; }
        public bool PdfAvailable { get; set; }
        public string? OwnerId { get; set; }
        public bool IsPurchased { get; set; }
    }
}