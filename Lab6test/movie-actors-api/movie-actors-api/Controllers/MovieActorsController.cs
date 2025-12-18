using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieActorsService.DTOs;
using MovieActorsService.Models;
using MovieActorsService.Repositories;
using MovieActorsService.Services;

namespace MovieActorsService.Controllers
{
    [ApiController]
    [Route("api/movie-actors")]
    public class MovieActorsController : ControllerBase
    {
        private readonly IMovieActorRepository _repository;
        private readonly ActorsClient _actors;
        private readonly MoviesClient _movies;

        public MovieActorsController(
            IMovieActorRepository repository,
            ActorsClient actorsClient,
            MoviesClient moviesClient)
        {
            _repository = repository;
            _actors = actorsClient;
            _movies = moviesClient;
        }

        // ---------- Всі записи ----------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieActorDto>>> GetAll()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var list = await _repository.GetAllAsync();
            var result = new List<MovieActorDto>();

            foreach (var ma in list)
            {
                var movie = await _movies.GetMovieAsync(ma.MovieId, token);
                var actor = await _actors.GetActorAsync(ma.ActorId, token);

                result.Add(new MovieActorDto
                {
                    ActorId = ma.ActorId,
                    ActorFullName = actor != null ? $"{actor.FirstName} {actor.LastName}" : "Unknown",
                    MovieId = ma.MovieId,
                    MovieTitle = movie?.Title ?? "Unknown",
                    MovieGenre = movie?.Genre ?? "",
                    MovieReleaseYear = movie?.ReleaseYear ?? 0,
                    CharacterName = ma.CharacterName
                });
            }

            return Ok(result);
        }

        // ---------- Фільми конкретного актора ----------
        [HttpGet("actor/{actorId}")]
        public async Task<ActionResult<IEnumerable<MovieActorDto>>> GetByActor(int actorId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var list = await _repository.GetByActorAsync(actorId);
            var actor = await _actors.GetActorAsync(actorId, token);
            var fullName = actor != null ? $"{actor.FirstName} {actor.LastName}" : "Unknown";

            var result = new List<MovieActorDto>();
            foreach (var ma in list)
            {
                var movie = await _movies.GetMovieAsync(ma.MovieId, token);
                result.Add(new MovieActorDto
                {
                    ActorId = ma.ActorId,
                    ActorFullName = fullName,
                    MovieId = ma.MovieId,
                    MovieTitle = movie?.Title ?? "Unknown",
                    MovieGenre = movie?.Genre ?? "",
                    MovieReleaseYear = movie?.ReleaseYear ?? 0,
                    CharacterName = ma.CharacterName
                });
            }

            return Ok(result);
        }

        // ---------- Актори конкретного фільму ----------
        [HttpGet("movie/{movieId}")]
        public async Task<ActionResult<IEnumerable<ActorInMovieDto>>> GetByMovie(int movieId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var list = await _repository.GetByMovieAsync(movieId);
            var result = new List<ActorInMovieDto>();

            foreach (var ma in list)
            {
                var actor = await _actors.GetActorAsync(ma.ActorId, token);
                if (actor != null)
                {
                    result.Add(new ActorInMovieDto
                    {
                        Id = ma.ActorId,
                        FirstName = actor.FirstName,
                        LastName = actor.LastName,
                        CharacterName = ma.CharacterName
                    });
                }
            }

            return Ok(result);
        }

        // ---------- Конкретний запис ----------
        [HttpGet("{movieId}/{actorId}")]
        public async Task<ActionResult<MovieActorDto>> Get(int movieId, int actorId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var entity = await _repository.GetAsync(movieId, actorId);
            if (entity == null) return NotFound();

            var movie = await _movies.GetMovieAsync(entity.MovieId, token);
            var actor = await _actors.GetActorAsync(entity.ActorId, token);

            return new MovieActorDto
            {
                ActorId = entity.ActorId,
                ActorFullName = actor != null ? $"{actor.FirstName} {actor.LastName}" : "Unknown",
                MovieId = entity.MovieId,
                MovieTitle = movie?.Title ?? "Unknown",
                MovieGenre = movie?.Genre ?? "",
                MovieReleaseYear = movie?.ReleaseYear ?? 0,
                CharacterName = entity.CharacterName
            };
        }

        // ---------- Додати актор-фільм ----------
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<MovieActorDto>> Create(CreateMovieActorDto dto)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (!await _actors.ActorExists(dto.ActorId, token))
                return BadRequest("Actor does not exist!");
            if (!await _movies.MovieExists(dto.MovieId, token))
                return BadRequest("Movie does not exist!");

            var entity = new MovieActor
            {
                MovieId = dto.MovieId,
                ActorId = dto.ActorId,
                CharacterName = dto.CharacterName,
                JoinedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(entity);

            var movie = await _movies.GetMovieAsync(entity.MovieId, token);
            var actor = await _actors.GetActorAsync(entity.ActorId, token);

            return CreatedAtAction(nameof(Get), new { movieId = entity.MovieId, actorId = entity.ActorId },
                new MovieActorDto
                {
                    ActorId = entity.ActorId,
                    ActorFullName = actor != null ? $"{actor.FirstName} {actor.LastName}" : "Unknown",
                    MovieId = entity.MovieId,
                    MovieTitle = movie?.Title ?? "Unknown",
                    MovieGenre = movie?.Genre ?? "",
                    MovieReleaseYear = movie?.ReleaseYear ?? 0,
                    CharacterName = entity.CharacterName
                });
        }

        // ---------- Видалити актор-фільм ----------
        [Authorize]
        [HttpDelete("{movieId}/{actorId}")]
        public async Task<IActionResult> Delete(int movieId, int actorId)
        {
            var entity = await _repository.GetAsync(movieId, actorId);
            if (entity == null) return NotFound();

            await _repository.DeleteAsync(entity);
            return NoContent();
        }
    }
}
