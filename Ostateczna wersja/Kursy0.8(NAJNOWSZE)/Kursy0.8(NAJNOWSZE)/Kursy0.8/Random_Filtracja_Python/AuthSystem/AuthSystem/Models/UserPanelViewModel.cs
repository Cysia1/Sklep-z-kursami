using System.Collections.Generic;

namespace AuthSystem.Models
{
    public class UserPanelViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public List<CoursesItemViewModel> MyCourses { get; set; }
        public List<CoursesItemViewModel> PurchasedCourses { get; set; }
    }
}
