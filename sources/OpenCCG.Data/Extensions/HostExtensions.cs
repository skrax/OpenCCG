using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenCCG.Proto;

namespace OpenCCG.Data.Extensions;

public record Set(CreatureOutline[] Creatures, SpellOutline[] Spells);

public static class HostExtensions
{
    public static void MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();
        var json = File.ReadAllText("TestSet.json");
        var testSet = JsonSerializer.Deserialize<Set>(json);
        if (testSet == null) return;
        
        foreach (var creature in testSet.Creatures)
        {
            if (dbContext.Creatures.Any(x => x.Id == creature.Id))
            {
                dbContext.Creatures.Update(creature);
            }
            else
            {
                dbContext.Creatures.Add(creature);
            }
        }

        foreach (var spell in testSet.Spells)
        {
            if (dbContext.Spells.Any(x => x.Id == spell.Id))
            {
                dbContext.Spells.Update(spell);
            }
            else
            {
                dbContext.Spells.Add(spell);
            }
        }

        dbContext.SaveChanges();
    }
}