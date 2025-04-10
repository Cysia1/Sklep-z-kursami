using static AuthSystem.Entities.Database;
namespace AuthSystem.Models
{
    public class CoursesDetailViewModel
    {
       
            public int CourseId { get; set; }
            public string CourseName { get; set; }
            public string CourseDescription { get; set; }
            public CourseType CourseType { get; set; }
            public decimal CoursePrice { get; set; }
            public string? Thumbnail { get; set; }

     

    }
}
