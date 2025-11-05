using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var allActors = await _context.Actors
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.FirstName} {a.LastName}" })
                .ToListAsync();

            var vm = new MovieEditViewModel
            {
                AllActors = allActors,
                SelectedActorIds = Array.Empty<int>()
            };
            return View(vm);
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(MovieEditViewModel vm, string saveType)
        {
            if (!ModelState.IsValid)
            {
                vm.AllActors = await _context.Actors
                    .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.FirstName} {a.LastName}" })
                    .ToListAsync();
                return View(vm);
            }

            if (saveType == "draft")
            {
                TempData["DraftSaved"] = "Чернетка збережена!";
                TempData["MovieDraft"] = JsonSerializer.Serialize(vm);
                return RedirectToAction(nameof(Index));
            }

            var movie = new Movie
            {
                Title = vm.Title,
                Genre = vm.Genre,
                ReleaseDate = vm.ReleaseDate,
                Description = vm.Description,
                DurationMinutes = vm.DurationMinutes,
                MovieActors = vm.SelectedActorIds?.Select(id => new MovieActor { ActorId = id }).ToList()
            };

            _context.Add(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Movies/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            var allActors = await _context.Actors
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.FirstName} {a.LastName}" })
                .ToListAsync();

            var vm = new MovieEditViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                ReleaseDate = movie.ReleaseDate,
                Description = movie.Description,
                DurationMinutes = movie.DurationMinutes,
                AllActors = allActors,
                SelectedActorIds = movie.MovieActors.Select(ma => ma.ActorId).ToArray()
            };

            // Якщо є чернетка у TempData
            if (TempData["MovieDraft"] != null)
            {
                var draft = JsonSerializer.Deserialize<MovieEditViewModel>((string)TempData["MovieDraft"]!);
                if (draft != null && draft.Id == movie.Id)
                {
                    vm = draft;
                    TempData["DraftLoaded"] = "Завантажено чернетку!";
                }
            }

            return View(vm);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(MovieEditViewModel vm, string saveType)
        {
            if (!ModelState.IsValid)
            {
                vm.AllActors = await _context.Actors
                    .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.FirstName} {a.LastName}" })
                    .ToListAsync();
                return View(vm);
            }

            if (saveType == "draft")
            {
                TempData["DraftSaved"] = "Чернетка збережена!";
                TempData["MovieDraft"] = JsonSerializer.Serialize(vm);
                return RedirectToAction(nameof(Index));
            }

            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                .FirstOrDefaultAsync(m => m.Id == vm.Id);

            if (movie == null) return NotFound();

            movie.Title = vm.Title;
            movie.Genre = vm.Genre;
            movie.ReleaseDate = vm.ReleaseDate;
            movie.Description = vm.Description;
            movie.DurationMinutes = vm.DurationMinutes;

            movie.MovieActors.Clear();
            if (vm.SelectedActorIds != null)
            {
                movie.MovieActors = vm.SelectedActorIds
                    .Select(actorId => new MovieActor { ActorId = actorId, MovieId = movie.Id })
                    .ToList();
            }

            try
            {
                _context.Update(movie);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Movies.Any(m => m.Id == vm.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Movies/ApplyDraft
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ApplyDraft(string vmJson)
        {
            if (string.IsNullOrEmpty(vmJson)) return RedirectToAction(nameof(Index));

            var vm = JsonSerializer.Deserialize<MovieEditViewModel>(vmJson);
            if (vm == null) return RedirectToAction(nameof(Index));

            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                .FirstOrDefaultAsync(m => m.Id == vm.Id);

            if (movie == null)
            {
                movie = new Movie
                {
                    Title = vm.Title,
                    Genre = vm.Genre,
                    ReleaseDate = vm.ReleaseDate,
                    Description = vm.Description,
                    DurationMinutes = vm.DurationMinutes,
                    MovieActors = vm.SelectedActorIds?.Select(id => new MovieActor { ActorId = id }).ToList()
                };
                _context.Add(movie);
            }
            else
            {
                movie.Title = vm.Title;
                movie.Genre = vm.Genre;
                movie.ReleaseDate = vm.ReleaseDate;
                movie.Description = vm.Description;
                movie.DurationMinutes = vm.DurationMinutes;
                movie.MovieActors.Clear();
                if (vm.SelectedActorIds != null)
                {
                    movie.MovieActors = vm.SelectedActorIds
                        .Select(actorId => new MovieActor { ActorId = actorId, MovieId = movie.Id })
                        .ToList();
                }
                _context.Update(movie);
            }

            await _context.SaveChangesAsync();
            TempData.Remove("MovieDraft");
            TempData["DraftSaved"] = "Зміни з чернетки збережено у базу!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Movies/Delete/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie != null)
            {
                _context.MovieActors.RemoveRange(movie.MovieActors);
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
