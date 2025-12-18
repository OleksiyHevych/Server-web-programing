using ActorsApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ActorsApi.Data;

public class ActorsContext(DbContextOptions<ActorsContext> options) : DbContext(options)
{
    public DbSet<Actor> Actors => Set<Actor>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Actor>().Property(a => a.FirstName).HasMaxLength(50).IsRequired();
        b.Entity<Actor>().Property(a => a.LastName).HasMaxLength(50).IsRequired();
        b.Entity<Actor>().Property(a => a.Country).HasMaxLength(50).IsRequired();
        b.Entity<Actor>().Property(a => a.Biography).HasMaxLength(1000).IsRequired();

        // Seed example actors
        b.Entity<Actor>().HasData(
            new Actor
            {
                Id = 1,
                FirstName = "Oksana",
                LastName = "Petrenko",
                BirthDate = new DateTime(1985, 4, 12),
                Country = "Ukraine",
                Biography = "Stage and film actress. Known for several leading roles."
            },
            new Actor
            {
                Id = 2,
                FirstName = "Ivan",
                LastName = "Kravchuk",
                BirthDate = new DateTime(1979, 11, 2),
                Country = "Ukraine",
                Biography = "Character actor with long career in theatre."
            }
        );
    }
}
