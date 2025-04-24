using System.Text;
using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;



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
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Domyœlnie u¿ywaj ciasteczek do uwierzytelniania
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;   // Domyœlnie u¿ywaj ciasteczek do wyzwania
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Twoje API", Version = "v1" });

            // Opcjonalnie: Dodaj definicjê zabezpieczeñ dla JWT Bearer
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Wstaw token JWT Bearer w to pole",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
        });

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
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Twoje API v1");
            // Opcjonalnie: Ustaw œcie¿kê do Swagger UI na root
            // c.RoutePrefix = string.Empty;
        });

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRazorPages();

        app.MapControllerRoute(
            name: "login",
            pattern: "/",
            defaults: new { area = "Identity", page = "/Account/Login" });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");



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