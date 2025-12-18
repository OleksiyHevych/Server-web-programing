using Lab5.Data;
using Lab5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // всі методи вимагають авторизації
    public class MovieController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MovieController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: доступно всім авторизованим
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movies = await _context.Movies
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .ToListAsync();

            var dtos = movies.Select(movie => new MovieDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                ReleaseDate = movie.ReleaseDate,
                DurationMinutes = movie.DurationMinutes,
                Description = movie.Description,
                Actors = movie.MovieActors.Select(ma => new MovieActorDto
                {
                    ActorId = ma.ActorId,
                    Name = $"{ma.Actor?.FirstName} {ma.Actor?.LastName}",
                    RoleName = ma.RoleName
                }).ToList()
            }).ToList();

            return Ok(dtos);
        }

        // GET by id
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            var dto = new MovieDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                ReleaseDate = movie.ReleaseDate,
                DurationMinutes = movie.DurationMinutes,
                Description = movie.Description,
                Actors = movie.MovieActors.Select(ma => new MovieActorDto
                {
                    ActorId = ma.ActorId,
                    Name = $"{ma.Actor?.FirstName} {ma.Actor?.LastName}",
                    RoleName = ma.RoleName
                }).ToList()
            };

            return Ok(dto);
        }

        // POST: тільки для Admin
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Movie movie)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = movie.Id }, movie);
        }

        // PUT: тільки для Admin
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, Movie movie)
        {
            if (id != movie.Id) return BadRequest();

            _context.Entry(movie).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: тільки для Admin
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    // DTOs
    public class MovieDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int DurationMinutes { get; set; }
        public string? Description { get; set; }
        public List<MovieActorDto> Actors { get; set; } = new();
    }

    public class MovieActorDto
    {
        public int ActorId { get; set; }
        public string? Name { get; set; }
        public string? RoleName { get; set; }
    }
}
