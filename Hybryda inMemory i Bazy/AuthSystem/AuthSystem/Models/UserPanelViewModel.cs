using System.Collections.Generic;

namespace AuthSystem.Models
{
    public class UserPanelViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public bool IsSubscribed { get; set; }

        public List<CoursesItemViewModel> OwnCourses { get; set; } = new();
    }
}
