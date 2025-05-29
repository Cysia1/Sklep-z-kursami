using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using static AuthSystem.Entities.Database;
using static AuthSystem.Repositories.CourseRepository;
using AuthSystem.Repositories;
using Stripe;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var kursyConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        var authConnectionString = builder.Configuration.GetConnectionString("AuthDbContextConnection")
            ?? throw new InvalidOperationException("AuthDbContextConnection string 'AuthDbContextConnection' not found.");

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<ICourseRepositoryService, SqlCourseRepositoryService>();

        builder.Services.AddScoped<IRepositoryService<Courses, int>, SqlCourseRepositoryService>();

        builder.Services.AddDbContext<KursyDbContext>(options =>
            options.UseSqlServer(kursyConnectionString));

        builder.Services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(authConnectionString));

        builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<AuthDbContext>();


        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        builder.Services.AddHttpClient();

        StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe")["SecretKey"];

        builder.Services.AddAuthentication()
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
           });


        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Twoje API", Version = "v1" });


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
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Twoje API v1");
            });
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

public class KursyDbContext : DbContext
{
    public KursyDbContext(DbContextOptions<KursyDbContext> options) : base(options) { }

    public DbSet<Courses> Courses { get; set; }
}