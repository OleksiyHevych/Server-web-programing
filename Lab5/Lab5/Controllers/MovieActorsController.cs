using Lab5.Data;
using Lab5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MovieActorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public MovieActorsController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.MovieActors
                .Include(ma => ma.Movie)
                .Include(ma => ma.Actor)
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(MovieActor model)
        {
            var movie = await _context.Movies.FindAsync(model.MovieId);
            if (movie == null) return BadRequest("Movie not found");

            var actor = await _context.Actors.FindAsync(model.ActorId);
            if (actor == null) return BadRequest("Actor not found");

            _context.MovieActors.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }
    }
}
