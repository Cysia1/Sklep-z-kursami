namespace Test2.Entities
{
    public class User
    {
        // Unikalny identyfikator użytkownika (GUID - Global Unique Identifier)
        public Guid Id { get; set; }
        // Adres e-mail użytkownika
        public string Email { get; set; } = string.Empty;
        // Zaszyfrowane hasło użytkownika (hash hasła)
        public string PasswordHash { get; set; } = string.Empty;

    }
}
