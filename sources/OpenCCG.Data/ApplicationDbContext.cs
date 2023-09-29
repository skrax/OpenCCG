using Microsoft.EntityFrameworkCore;
using OpenCCG.Proto;

namespace OpenCCG.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<CreatureOutline> Creatures { get; set; } = null!;

    public DbSet<SpellOutline> Spells { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}