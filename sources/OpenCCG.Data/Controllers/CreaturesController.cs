using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenCCG.Proto;

namespace OpenCCG.Data.Controllers;

[ApiController]
[Route("[controller]")]
public class CreaturesController : ControllerBase
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly ILogger<CreaturesController> _logger;

    public CreaturesController(
        ApplicationDbContext applicationDbContext,
        ILogger<CreaturesController> logger
    )
    {
        _applicationDbContext = applicationDbContext;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CreatureOutline>), 200)]
    public async Task<ActionResult> GetAsync(
        [FromQuery] string[] ids,
        [FromQuery] [Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery] [Range(1, 50)] int pageSize = 50
    )
    {
        var keys = ids.ToImmutableHashSet();

        var creatures = keys.Any()
            ? await _applicationDbContext.Creatures
                                         .Where(x => keys.Contains(x.Id))
                                         .ToArrayAsync()
            : await _applicationDbContext.Creatures
                                         .Skip((pageNumber - 1) * pageSize)
                                         .Take(pageSize)
                                         .ToArrayAsync();

        return Ok(creatures);
    }

    [HttpGet("{id:maxlength(32)}")]
    [ProducesResponseType(typeof(IEnumerable<CreatureOutline>), 200)]
    public async Task<ActionResult> GetAsync(string id)
    {
        var creature = await _applicationDbContext.Creatures.FindAsync(id);

        return Ok(creature);
    }
}