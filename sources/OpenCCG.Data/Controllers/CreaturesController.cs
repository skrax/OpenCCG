using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenCCG.Proto;

namespace OpenCCG.Data.Controllers;

public enum CreaturesOrderBy
{
     Cost,
     Name,
     Atk,
     Def
}

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
        [FromQuery] CreaturesOrderBy? orderBy,
        [FromQuery] [Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery] [Range(1, 50)] int pageSize = 50
    )
    {
        var keys = ids.ToImmutableHashSet();

        var query = keys.Any()
            ? _applicationDbContext.Creatures
                                   .Where(x => keys.Contains(x.Id))
            : _applicationDbContext.Creatures
                                   .Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize);

        if (orderBy.HasValue)
        {
            switch (orderBy.Value)
            {
                case CreaturesOrderBy.Cost:
                    query = query.OrderBy(x => x.Cost);
                    break;
                case CreaturesOrderBy.Name:
                    query = query.OrderBy(x => x.Name);
                    break;
                case CreaturesOrderBy.Atk:
                    query = query.OrderBy(x => x.Atk);
                    break;
                case CreaturesOrderBy.Def:
                    query = query.OrderBy(x => x.Def);
                    break;
                default:
                    return BadRequest();
            }
        }

        var creatures = await query.ToArrayAsync();

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