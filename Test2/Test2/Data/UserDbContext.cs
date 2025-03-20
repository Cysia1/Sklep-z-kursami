using Microsoft.EntityFrameworkCore;
using Test2.Entities;

namespace Test2.Data
{
    // Definicja klasy kontekstu bazy danych dziedziczącej po DbContext
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        // Definicja DbSet, który reprezentuje tabelę "Users" w bazie danych
        public DbSet<User> Users { get; set; }
    }
}
