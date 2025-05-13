using System.ComponentModel.DataAnnotations;
using static AuthSystem.Repositories.CourseRepository;


namespace AuthSystem.Entities
{
    public class Database
    {
        public class Courses : IEntity<int>
        {
            [Key]
            public int CourseId { get; set; }
            public string CourseName { get; set; }
            public string CourseDescription { get; set; }
            public string? CourseType { get; set; }
            public decimal CoursePrice { get; set; }

            public byte[] Thumbnail { get; set; }

            // Jawna implementacja właściwości Id z interfejsu IEntity<int>
            int IEntity<int>.Id
            {
                get => CourseId;
                set => CourseId = value;
            }
        }

        public class UserCards : IEntity<int>
        {
            [Key]
            public int Id { get; set; }
            public int cardNumber { get; set; }
            public string expiryDate { get; set; }
        }

    }
}