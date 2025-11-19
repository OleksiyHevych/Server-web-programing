using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Lab1.Data;
using Lab1.Models;
using Lab1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Lab1.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const string DraftSessionKey = "MovieDraft";
        private const string DraftExpirySessionKey = "MovieDraftExpiryUtc";

        public MoviesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .ToListAsync();

            // draft in session check
            var draftJson = HttpContext.Session.GetString(DraftSessionKey);
            if (!string.IsNullOrEmpty(draftJson))
            {
                var expiryTicks = HttpContext.Session.GetString(DraftExpirySessionKey);
                if (long.TryParse(expiryTicks, out var ticks))
                {
                    var expiry = new DateTime(ticks, DateTimeKind.Utc);
                    if (expiry > DateTime.UtcNow)
                    {
                        ViewBag.ActiveDraftJson = draftJson;
                        ViewBag.ActiveDraftExpiryUtc = expiry.ToString("o");
                    }
                    else
                    {
                        HttpContext.Session.Remove(DraftSessionKey);
                        HttpContext.Session.Remove(DraftExpirySessionKey);
                    }
                }
            }

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
                var json = JsonSerializer.Serialize(vm);
                HttpContext.Session.SetString(DraftSessionKey, json);
                var expiry = DateTime.UtcNow.AddSeconds(30).Ticks.ToString();
                HttpContext.Session.SetString(DraftExpirySessionKey, expiry);

                TempData["DraftSaved"] = "Чернетка збережена. Маєте 30 секунд, щоб застосувати її.";
                return RedirectToAction(nameof(Index));
            }

            var movie = new Movie
            {
                Title = vm.Title,
                Genre = vm.Genre,
                ReleaseDate = vm.ReleaseDate,
                Description = vm.Description,
                DurationMinutes = vm.DurationMinutes,
                MovieActors = vm.SelectedActorIds?.Select(id => new MovieActor { ActorId = id }).ToList() ?? new List<MovieActor>()
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
                var json = JsonSerializer.Serialize(vm);
                HttpContext.Session.SetString(DraftSessionKey, json);
                var expiry = DateTime.UtcNow.AddSeconds(30).Ticks.ToString();
                HttpContext.Session.SetString(DraftExpirySessionKey, expiry);

                TempData["DraftSaved"] = "Чернетка збережена. Маєте 30 секунд, щоб застосувати її.";
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
        public async Task<IActionResult> ApplyDraft()
        {
            var draftJson = HttpContext.Session.GetString(DraftSessionKey);
            var expiryStr = HttpContext.Session.GetString(DraftExpirySessionKey);
            if (string.IsNullOrEmpty(draftJson) || !long.TryParse(expiryStr, out var ticks))
            {
                TempData["DraftError"] = "Чернетка не знайдена або прострочена.";
                return RedirectToAction(nameof(Index));
            }

            var expiry = new DateTime(ticks, DateTimeKind.Utc);
            if (expiry < DateTime.UtcNow)
            {
                HttpContext.Session.Remove(DraftSessionKey);
                HttpContext.Session.Remove(DraftExpirySessionKey);
                TempData["DraftError"] = "Час на застосування чернетки минув.";
                return RedirectToAction(nameof(Index));
            }

            var vm = JsonSerializer.Deserialize<MovieEditViewModel>(draftJson);
            if (vm == null)
            {
                TempData["DraftError"] = "Не вдалося прочитати чернетку.";
                return RedirectToAction(nameof(Index));
            }

            if (vm.Id == 0)
            {
                var movie = new Movie
                {
                    Title = vm.Title,
                    Genre = vm.Genre,
                    ReleaseDate = vm.ReleaseDate,
                    Description = vm.Description,
                    DurationMinutes = vm.DurationMinutes,
                    MovieActors = vm.SelectedActorIds?.Select(id => new MovieActor { ActorId = id }).ToList() ?? new List<MovieActor>()
                };
                _context.Add(movie);
            }
            else
            {
                var movie = await _context.Movies
                    .Include(m => m.MovieActors)
                    .FirstOrDefaultAsync(m => m.Id == vm.Id);

                if (movie == null)
                {
                    TempData["DraftError"] = "Фільм не знайдено.";
                    return RedirectToAction(nameof(Index));
                }

                movie.Title = vm.Title;
                movie.Genre = vm.Genre;
                movie.ReleaseDate = vm.ReleaseDate;
                movie.Description = vm.Description;
                movie.DurationMinutes = vm.DurationMinutes;
                movie.MovieActors.Clear();
                if (vm.SelectedActorIds != null)
                {
                    movie.MovieActors = vm.SelectedActorIds
                        .Select(id => new MovieActor { ActorId = id, MovieId = movie.Id })
                        .ToList();
                }
                _context.Update(movie);
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(DraftSessionKey);
            HttpContext.Session.Remove(DraftExpirySessionKey);

            TempData["DraftApplied"] = "Чернетка успішно застосована і збережена у базу.";
            return RedirectToAction(nameof(Index));
        }

        // File upload endpoint (saves to wwwroot/uploads) - for chat files
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { error = "No file" });

            var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var uniqueName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var savePath = Path.Combine(uploads, uniqueName);

            await using (var fs = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            var url = Url.Content($"~/uploads/{uniqueName}");
            return Json(new { url, fileName = file.FileName });
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
