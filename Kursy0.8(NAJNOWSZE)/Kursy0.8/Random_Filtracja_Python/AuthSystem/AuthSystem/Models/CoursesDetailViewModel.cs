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

    }
}