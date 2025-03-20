namespace Test2.Models
{
    // Klasa UserDto służy do reprezentacji danych użytkownika.
    // Jest to tzw. Data Transfer Object (DTO), czyli obiekt przeznaczony do przesyłania danych między warstwami aplikacji.
    public class UserDto
    {
        // Właściwość przechowująca adres e-mail użytkownika.
        // Domyślnie przypisujemy pusty string, aby uniknąć wartości null.
        public string Email { get; set; } = string.Empty;
        // Właściwość przechowująca hasło użytkownika.
        // Również ustawiona na pusty string, aby zapobiec wartości null.
        public string Password { get; set; } = string.Empty;
    }
}
