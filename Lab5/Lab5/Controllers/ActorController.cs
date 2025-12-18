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
    public class ActorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ActorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: всі авторизовані користувачі
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var actors = await _context.Actors
                .Include(a => a.MovieActors)
                    .ThenInclude(ma => ma.Movie)
                .ToListAsync();

            var dtos = actors.Select(actor => new ActorDto
            {
                Id = actor.Id,
                FirstName = actor.FirstName,
                LastName = actor.LastName,
                BirthDate = actor.BirthDate,
                Country = actor.Country,
                Biography = actor.Biography,
                Movies = actor.MovieActors.Select(ma => new ActorMovieDto
                {
                    MovieId = ma.MovieId,
                    Title = ma.Movie?.Title,
                    RoleName = ma.RoleName
                }).ToList()
            }).ToList();

            return Ok(dtos);
        }

        // GET by id
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var actor = await _context.Actors
                .Include(a => a.MovieActors)
                    .ThenInclude(ma => ma.Movie)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null) return NotFound();

            var dto = new ActorDto
            {
                Id = actor.Id,
                FirstName = actor.FirstName,
                LastName = actor.LastName,
                BirthDate = actor.BirthDate,
                Country = actor.Country,
                Biography = actor.Biography,
                Movies = actor.MovieActors.Select(ma => new ActorMovieDto
                {
                    MovieId = ma.MovieId,
                    Title = ma.Movie?.Title,
                    RoleName = ma.RoleName
                }).ToList()
            };

            return Ok(dto);
        }

        // POST: Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Actor actor)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Actors.Add(actor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = actor.Id }, actor);
        }

        // PUT: Admin only
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, Actor actor)
        {
            if (id != actor.Id) return BadRequest();

            _context.Entry(actor).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: Admin only
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor == null) return NotFound();

            _context.Actors.Remove(actor);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
