using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIIR.Models;

namespace SIIR.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Representative> Representatives { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<UniformCatalog> UniformCatalog { get; set; }
        public DbSet<DocumentCatalog> DocumentCatalog { get; set; }

    }
}
