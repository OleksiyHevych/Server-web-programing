using MoviesService.Models;
using MoviesService.Data;

namespace MoviesService.Services
{
    public class MoviesSeeder
    {
        public static void Seed(MoviesDbContext context)
        {
            if (!context.Movies.Any())
            {
                context.Movies.AddRange(
                    new Movie { Title = "Inception", Genre = "Sci-Fi", ReleaseYear = 2010 },
                    new Movie { Title = "Interstellar", Genre = "Sci-Fi", ReleaseYear = 2014 },
                    new Movie { Title = "The Dark Knight", Genre = "Action", ReleaseYear = 2008 }
                );
                context.SaveChanges();
            }
        }
    }
}
