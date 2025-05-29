using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static AuthSystem.Entities.Database;

namespace AuthSystem.Data 
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser> 
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        public DbSet<Quote> Quotes { get; set; }

        public DbSet<Courses> Courses { get; set; }

        public DbSet<Payments> Payments { get; set; }

        public DbSet<Purchases> Purchases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

      
        }
    }
}