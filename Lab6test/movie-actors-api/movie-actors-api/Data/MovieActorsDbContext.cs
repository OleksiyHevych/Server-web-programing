using Microsoft.EntityFrameworkCore;
using MovieActorsService.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MovieActorsService.Data
{
    public class MovieActorsDbContext : DbContext
    {
        public MovieActorsDbContext(DbContextOptions<MovieActorsDbContext> options) : base(options) { }

        public DbSet<MovieActor> MovieActors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MovieActor>()
                .HasKey(x => new { x.MovieId, x.ActorId });
        }
    }
}
