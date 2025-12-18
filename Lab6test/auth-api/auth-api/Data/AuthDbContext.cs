using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Seed admin
        builder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Email = "admin@local",
                PasswordHash = PasswordHasher.Hash("admin123"),
                Role = "Admin"
            }
        );
    }
}
