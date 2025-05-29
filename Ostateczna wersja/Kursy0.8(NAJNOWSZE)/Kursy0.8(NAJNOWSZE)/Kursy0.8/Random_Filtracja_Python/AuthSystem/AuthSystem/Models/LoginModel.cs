// Models/LoginModel.cs
using Microsoft.AspNetCore.Authorization;

namespace AuthSystem.Models
{
    [AllowAnonymous]
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}