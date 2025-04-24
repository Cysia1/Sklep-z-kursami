// Models/RegisterModel.cs
namespace AuthSystem.Models
{
    public class RegisterModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; } // Możesz użyć Email jako Username
        public string Email { get; set; }
        public string Password { get; set; }
    }
}