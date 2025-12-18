using ActorsApi.Models;
using ActorsApi.Repositories;
using ActorsApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActorsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActorsController : ControllerBase
{
    private readonly IRepository<Actor> _repo;

    public ActorsController(IRepository<Actor> repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<List<Actor>>> GetAll()
    {
        var actors = await _repo.GetAllAsync();
        return Ok(actors);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Actor>> Get(int id)
    {
        var actor = await _repo.GetAsync(id);
        if (actor == null) return NotFound();
        return Ok(actor);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Actor>> Create([FromBody] Actor actor)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var created = await _repo.AddAsync(actor);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Actor actor)
    {
        if (id != actor.Id) return BadRequest("Route id ≠ body id");

        return await _repo.UpdateAsync(actor) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
        => await _repo.DeleteAsync(id) ? NoContent() : NotFound();
}
