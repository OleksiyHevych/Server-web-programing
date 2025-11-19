using Lab1.Data;
using Lab1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Lab1.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _db;

        public FilesController(IWebHostEnvironment env, ApplicationDbContext db)
        {
            _env = env;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { error = "No file" });

            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var unique = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploads, unique);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            var url = Url.Content($"~/uploads/{unique}");
            return Ok(new { url, fileName = file.FileName });
        }
    }
}
