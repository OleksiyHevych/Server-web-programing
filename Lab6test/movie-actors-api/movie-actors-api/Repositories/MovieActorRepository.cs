using Microsoft.EntityFrameworkCore;
using MovieActorsService.Data;
using MovieActorsService.Models;

namespace MovieActorsService.Repositories
{
    public class MovieActorRepository : IMovieActorRepository
    {
        private readonly MovieActorsDbContext _context;

        public MovieActorRepository(MovieActorsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MovieActor>> GetAllAsync()
            => await _context.MovieActors.ToListAsync();

        public async Task<MovieActor?> GetAsync(int movieId, int actorId)
            => await _context.MovieActors
                .FirstOrDefaultAsync(x => x.MovieId == movieId && x.ActorId == actorId);

        public async Task<MovieActor> CreateAsync(MovieActor entity)
        {
            _context.MovieActors.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(MovieActor entity)
        {
            _context.MovieActors.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<MovieActor>> GetByActorAsync(int actorId)
            => await _context.MovieActors.Where(x => x.ActorId == actorId).ToListAsync();

        public async Task<IEnumerable<MovieActor>> GetByMovieAsync(int movieId)
            => await _context.MovieActors.Where(x => x.MovieId == movieId).ToListAsync();
    }
}
