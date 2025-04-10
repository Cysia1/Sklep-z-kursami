using System.ComponentModel.DataAnnotations;
using static AuthSystem.Repositories.CourseRepository;

namespace AuthSystem.Entities
{
    public class Database
    {
        public enum CourseType
        {
            Programowanie,
            Biznes,
            Marketing,
            Design

        }
        public class Courses : IEntity<int>
        {
            [Key]
            public int Id { get; set; }
            public string CourseName { get; set; }
            public string CourseDescription { get; set; }
            public CourseType CourseType { get; set; }
            public decimal CoursePrice { get; set; }
            public string Thumbnail { get; set; }


        }
 


    }
}

