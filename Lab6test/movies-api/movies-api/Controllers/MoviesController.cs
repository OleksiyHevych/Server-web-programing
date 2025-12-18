using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesService.DTOs;
using MoviesService.Models;
using MoviesService.Repositories;

namespace MoviesService.Controllers
{
    [ApiController]
    [Route("api/movies")]
    [Authorize]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieRepository _repository;

        public MoviesController(IMovieRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movie>>> GetAll()
        {
            var movies = await _repository.GetAllAsync();
            return Ok(movies);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Movie>> GetById(int id)
        {
            var movie = await _repository.GetByIdAsync(id);
            if (movie == null) return NotFound();
            return Ok(movie);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Movie>> Create([FromBody] CreateMovieDto dto)
        {
            var movie = new Movie
            {
                Title = dto.Title,
                Genre = dto.Genre,
                ReleaseYear = dto.ReleaseYear
            };

            await _repository.CreateAsync(movie);
            return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMovieDto dto)
        {
            var movie = await _repository.GetByIdAsync(id);
            if (movie == null) return NotFound();

            movie.Title = dto.Title ?? movie.Title;
            movie.Genre = dto.Genre ?? movie.Genre;
            movie.ReleaseYear = dto.ReleaseYear ?? movie.ReleaseYear;

            await _repository.UpdateAsync(movie);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _repository.GetByIdAsync(id);
            if (movie == null) return NotFound();

            await _repository.DeleteAsync(movie);
            return NoContent();
        }
    }
}
