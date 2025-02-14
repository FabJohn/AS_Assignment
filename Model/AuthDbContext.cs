using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using WebApplication1.Model; // Ensure this namespace points to where your ApplicationUser is defined

namespace WebApplication1.Model
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser> // Use ApplicationUser here
    {
        private readonly IConfiguration _configuration;

        public AuthDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = _configuration.GetConnectionString("AuthConnectionString");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}