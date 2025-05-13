using static AuthSystem.Entities.Database;
namespace AuthSystem.Models
{
    public class CoursesItemViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string? CourseType { get; set; }
      
        public string? CourseDescription { get; set; } 
        public decimal? CoursePrice { get; set; } 
        public string? ThumbnailUrl { get; set; } 
    }
}