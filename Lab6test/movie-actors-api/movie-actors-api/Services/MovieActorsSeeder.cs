using MovieActorsService.Data;
using MovieActorsService.Models;

namespace MovieActorsService.Services
{
    public class MovieActorsSeeder
    {
        public static void Seed(MovieActorsDbContext context)
        {
            if (!context.MovieActors.Any())
            {
                context.MovieActors.AddRange(
                    new MovieActor { MovieId = 1, ActorId = 1, CharacterName = "Cobb", JoinedAt = DateTime.UtcNow },
                    new MovieActor { MovieId = 2, ActorId = 1, CharacterName = "Cooper", JoinedAt = DateTime.UtcNow },
                    new MovieActor { MovieId = 3, ActorId = 2, CharacterName = "Joker", JoinedAt = DateTime.UtcNow }
                );
                context.SaveChanges();
            }
        }
    }
}
