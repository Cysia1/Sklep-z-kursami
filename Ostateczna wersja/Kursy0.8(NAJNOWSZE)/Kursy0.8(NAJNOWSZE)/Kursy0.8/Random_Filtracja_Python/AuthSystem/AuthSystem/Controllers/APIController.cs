// Controllers/Api/AuthController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthSystem.Models; 
using AuthSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization; // Przestrzeń nazw dla ApplicationUser
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AuthSystem.Areas.Identity.Pages.Account;


[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthSystem.Models.RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddClaimAsync(user, new Claim("FirstName", model.FirstName ?? ""));
            await _userManager.AddClaimAsync(user, new Claim("LastName", model.LastName ?? ""));

            // Po rejestracji, zamiast tylko zwracać token, możesz opcjonalnie
            // od razu zalogować użytkownika poprzez ciasteczka i przekierować.
            // Jeśli chcesz to zrobić, odkomentuj poniższy blok kodu.
            // var token = GenerateJwtToken(user);
            // await SignInUserAsync(user);
            // return Ok(new { Token = token, RedirectUrl = "/" }); // Przekieruj na stronę główną po rejestracji

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token }); // Zwróć tylko token po rejestracji
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthSystem.Models.LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Użyj UserManager do sprawdzenia danych, nie SignInManager, jeśli nie chcesz ciasteczek tu
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized(new { Message = "Nieprawidłowa nazwa użytkownika lub hasło." });
        }

        // Jeśli uwierzytelnienie przebiegło pomyślnie, zwróć JWT
        var token = GenerateJwtToken(user);
        return Ok(new { Token = token }); // TYLKO token, bez RedirectUrl
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("FirstName", user.FirstName ?? ""),
            new Claim("LastName", user.LastName ?? ""),
            // Możesz dodać tutaj inne claimy, np. role użytkownika
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(int.Parse(_configuration["Jwt:ExpiresInHours"])), // Czas wygaśnięcia tokenu
            SigningCredentials = creds,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private async Task SignInUserAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("FirstName", user.FirstName ?? ""),
            new Claim("LastName", user.LastName ?? ""),
            // Dodaj inne claimy użytkownika, które chcesz przechowywać w ciasteczku
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true, // Możesz dostosować czy ciasteczko ma być trwałe
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) // Możesz dostosować czas wygaśnięcia ciasteczka
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}