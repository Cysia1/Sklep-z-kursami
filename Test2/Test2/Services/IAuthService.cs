using Test2.Entities;
using Test2.Models;

namespace Test2.Services
{
    // Interfejs dla serwisu autoryzacji użytkowników
    public interface IAuthService
    {
        // Metoda do rejestracji nowego użytkownika
        // Przyjmuje obiekt UserDto jako dane wejściowe i zwraca obiekt User lub null w przypadku niepowodzenia
        Task<User?> RegisterAsync(UserDto request);
        // Metoda do logowania użytkownika
        // Przyjmuje obiekt UserDto i zwraca token autoryzacyjny jako string lub null w przypadku błędu
        Task<string?> LoginAsync(UserDto request);
    }
}
