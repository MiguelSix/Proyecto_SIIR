﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
        public DbSet<Document> Document { get; set; }
        public DbSet<RepresentativeUniformCatalog> RepresentativeUniformCatalogs { get; set; }
        public DbSet<Uniform> Uniform { get; set; }
        public DbSet<Notification> Notification { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Coach)
                .WithMany()
                .HasForeignKey(t => t.CoachId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Representative)
                .WithMany()
                .HasForeignKey(t => t.RepresentativeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepresentativeUniformCatalog>()
                .HasKey(ruc => new { ruc.RepresentativeId, ruc.UniformCatalogId });

			//Borrar en cascada uniformes. 
			modelBuilder.Entity<Uniform>()
		        .HasOne(u => u.RepresentativeUniformCatalog)
		        .WithMany()
		        .HasForeignKey(u => new { u.RepresentativeId, u.UniformCatalogId })
		        .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Student)
                .WithMany()
                .HasForeignKey(n => n.StudentId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Document)
                .WithMany()
                .HasForeignKey(n => n.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
