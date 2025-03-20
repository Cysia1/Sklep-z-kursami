using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Test2.Entities;
using Test2.Models;
using Test2.Services;

namespace Test2.Controllers
{
    [Route("api/[controller]")] // Definiuje trasę dla kontrolera (np. /api/auth)
    [ApiController] // Oznacza, że jest to kontroler API
    public class AuthController(IAuthService authService) : ControllerBase // Konstruktor przyjmuje serwis uwierzytelniania
    {
        
        [HttpPost("register")] // Endpoint do rejestracji użytkownika
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await authService.RegisterAsync(request); // Wywołuje metodę rejestracji z serwisu autoryzacji
            if (user is null)
            
                return BadRequest("Username already exists.");// Zwraca błąd, jeśli użytkownik już istnieje
            

            return Ok(user); //Zwraca obiekt użytkownika
        }
        [HttpPost("login")] // Endpoint do logowania użytkownika
        public async Task<ActionResult<string>> LoginAsync(UserDto request)
        {
            var token = await authService.LoginAsync(request); // Wywołuje metodę logowania, która zwraca token
            if (token is null)

                return BadRequest("Invalid username or password"); // Zwraca błąd, jeśli dane logowania są niepoprawne


            return Ok(token); //Zwraca token JWT w odpowiedzi
        }

        [Authorize] // Wymaga uwierzytelniwnia użytkownika
        [HttpGet] // Definiuje metodę HTTP GET
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("You are authenticated!"); //Zawraca komunikat, jeśli użytkownik jest zalogowany
        }
        

    }
}
