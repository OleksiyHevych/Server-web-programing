using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab1.Data;
using Lab1.Models;
using Lab1.ViewModels;

namespace Lab1.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .ToListAsync();
            return View(movies);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            var viewModel = new MovieEditViewModel
            {
                Movie = new Movie(),
                AllActors = _context.Actors
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Name
                    }).ToList()
            };
            return View(viewModel);
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.AllActors = _context.Actors
                    .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
                    .ToList();
                return View(viewModel);
            }

            _context.Movies.Add(viewModel.Movie);
            await _context.SaveChangesAsync();

            if (viewModel.SelectedActorIds != null && viewModel.SelectedActorIds.Any())
            {
                foreach (var actorId in viewModel.SelectedActorIds)
                {
                    _context.MovieActors.Add(new MovieActor
                    {
                        MovieId = viewModel.Movie.Id,
                        ActorId = actorId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            var viewModel = new MovieEditViewModel
            {
                Movie = movie,
                AllActors = _context.Actors
                    .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
                    .ToList(),
                SelectedActorIds = movie.MovieActors.Select(ma => ma.ActorId).ToArray()
            };

            return View(viewModel);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MovieEditViewModel viewModel)
        {
            if (id != viewModel.Movie.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                viewModel.AllActors = _context.Actors
                    .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
                    .ToList();
                return View(viewModel);
            }

            try
            {
                _context.Update(viewModel.Movie);
                await _context.SaveChangesAsync();

                // Очищаємо старі зв'язки
                var oldMovieActors = _context.MovieActors.Where(ma => ma.MovieId == id);
                _context.MovieActors.RemoveRange(oldMovieActors);

                if (viewModel.SelectedActorIds != null && viewModel.SelectedActorIds.Any())
                {
                    foreach (var actorId in viewModel.SelectedActorIds)
                    {
                        _context.MovieActors.Add(new MovieActor
                        {
                            MovieId = id,
                            ActorId = actorId
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(viewModel.Movie.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie != null)
            {
                var relatedMovieActors = _context.MovieActors.Where(ma => ma.MovieId == id);
                _context.MovieActors.RemoveRange(relatedMovieActors);

                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id) => _context.Movies.Any(e => e.Id == id);
    }
}
