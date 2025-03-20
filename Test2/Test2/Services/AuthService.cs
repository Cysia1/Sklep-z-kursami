using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Test2.Data;
using Test2.Entities;
using Test2.Models;

namespace Test2.Services
{
    public class AuthService(UserDbContext context, IConfiguration configuration) : IAuthService
    {
        // Metoda do logowania użytkownika
        public async Task<string?> LoginAsync(UserDto request)
        {
            // Wyszukuje użytkownika w bazie danych na podstawie adresu e-mail
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user is null) //Sprawdzenie istnienia użytkownika
            {
                return null;
            }
            // Weryfikacja poprawności hasła
            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed) 
            {
                return null;
            }
            return CreateToken(user); //Zwrócenie wygenerowanego tokenu 
        }
        //Metoda do rejestrowania nowego użytkownika
        public async Task<User?> RegisterAsync(UserDto request)
        {
            // Sprawdzenie, czy użytkownik o podanym e-mailu już istnieje
            if (await context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return null;
            }
            // Tworzenie nowego użytkownika
            var user = new User();
            var hashedPassword = new PasswordHasher<User>()
                .HashPassword(user, request.Password);

            user.Email = request.Email;
            user.PasswordHash = hashedPassword;
            // Dodanie użytkownika do bazy danych
            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;// Zwrócenie nowo utworzonego użytkownika
        }
        // Metoda do tworzenia tokenu JWT dla zalogowanego użytkownika
        private string CreateToken(User user) 
        {
            // Tworzenie listy roszczeń (claims) dla tokenu
            var claims = new List<Claim> //Tworzenie listy roszczeń (claims)
            {
                new Claim(ClaimTypes.Name, user.Email),
                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            // Pobranie klucza JWT z pliku konfiguracyjnego (appsettings.json)
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!)); //Pobiera klucz JWT z AppSettings:Token (przechowywany w appsettings.json)
            // Użycie klucza do podpisania tokenu za pomocą algorytmu HMAC SHA512
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            // Tworzenie tokenu JWT
            var tokenDescriptor = new JwtSecurityToken( //Tworzenie token JWT
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), //Data wygaśnięcia (+7 dzień)
                signingCredentials: creds); //Podpisanie tokenu przy użyciu HmacSha512
            // Zwrócenie wygenerowanego tokenu jako string
           return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor); 
        }
    }
}
