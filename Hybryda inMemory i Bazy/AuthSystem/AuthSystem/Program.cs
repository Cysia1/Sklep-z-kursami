using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// Upewnij siê, ¿e te przestrzenie nazw istniej¹ w Twoim projekcie
using static AuthSystem.Entities.Database;
using static AuthSystem.Repositories.CourseRepository;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var kursyConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        var authConnectionString = builder.Configuration.GetConnectionString("AuthDbContextConnection") ?? throw new InvalidOperationException("Connection string 'AuthDbContextConnection' not found.");

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Konfiguracja dla bazy danych w pamiêci (tylko dla "kursów")
        builder.Services.AddDbContext<InMemoryDbContext>(options =>
            options.UseInMemoryDatabase("KursyTestDatabase"));

        // U¿ywamy naszego specjalnego repozytorium dla kursów, które dzia³a z InMemoryDbContext
        builder.Services.AddScoped<IRepositoryService<Courses, int>, InMemoryCourseRepositoryService>();

        // Konfiguracja dla bazy danych SQL Server (dla u¿ytkowników i autoryzacji)
        builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(authConnectionString));
        builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddEntityFrameworkStores<AuthDbContext>();

        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }
}

// Nowy kontekst dla bazy danych w pamiêci
public class InMemoryDbContext : IdentityDbContext<IdentityUser> // Zostawiamy IdentityDbContext, ale nie u¿ywamy go do logowania
{
    public InMemoryDbContext(DbContextOptions<InMemoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Courses> Courses { get; set; } // Tabela z kursami
}