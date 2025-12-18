using MovieActorsService.Models;

namespace MovieActorsService.Repositories
{
    public interface IMovieActorRepository
    {
        Task<IEnumerable<MovieActor>> GetAllAsync();
        Task<MovieActor?> GetAsync(int movieId, int actorId);
        Task<MovieActor> CreateAsync(MovieActor entity);
        Task DeleteAsync(MovieActor entity);

        // Нові методи
        Task<IEnumerable<MovieActor>> GetByActorAsync(int actorId);
        Task<IEnumerable<MovieActor>> GetByMovieAsync(int movieId);
    }
}
