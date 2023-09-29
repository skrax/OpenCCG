using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenCCG.Proto;

namespace OpenCCG.Data.Controllers;

[ApiController]
[Route("[controller]")]
public class SpellsController : ControllerBase
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly ILogger<SpellsController> _logger;

    public SpellsController(
        ApplicationDbContext applicationDbContext,
        ILogger<SpellsController> logger
    )
    {
        _applicationDbContext = applicationDbContext;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SpellOutline>), 200)]
    public async Task<ActionResult> GetAsync(
        [FromQuery] string[] ids,
        [FromQuery] [Range(0, int.MaxValue)] int pageNumber = 0,
        [FromQuery] [Range(1, 50)] int pageSize = 50
    )
    {
        var keys = ids.ToImmutableHashSet();

        var spells = keys.Any()
            ? await _applicationDbContext.Spells
                                         .Where(x => keys.Contains(x.Id))
                                         .ToArrayAsync()
            : await _applicationDbContext.Spells
                                         .Skip(pageNumber * pageSize)
                                         .Take(pageSize)
                                         .ToArrayAsync();

        return Ok(spells);
    }

    [HttpGet("{id:maxlength(32)}")]
    [ProducesResponseType(typeof(IEnumerable<SpellOutline>), 200)]
    public async Task<ActionResult> GetAsync(string id)
    {
        var spell = await _applicationDbContext.Spells.FindAsync(id);

        return Ok(spell);
    }
}