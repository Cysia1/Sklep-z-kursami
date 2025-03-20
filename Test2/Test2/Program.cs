using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Test2.Data;
using Test2.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Dodanie obs³ugi uwierzytelniania opartego na JWT (JSON Web Token)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Konfiguracja walidacji tokena JWT
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Sprawdzanie poprawnoœci wydawcy (issuer)
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            // Sprawdzanie poprawnoœci odbiorcy (audience)
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            // Sprawdzanie wa¿noœci tokena (czy nie wygas³)
            ValidateLifetime = true,
            // Klucz u¿ywany do podpisywania tokenów - musi byæ zgodny z tym, który zosta³ u¿yty do ich generowania
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
            // Sprawdzanie poprawnoœci klucza podpisuj¹cego token
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddDbContext<UserDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("UserDatabase")));

builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
