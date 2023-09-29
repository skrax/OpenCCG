using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenCCG.Proto;

namespace OpenCCG.Data.Configurations;

public class SpellOutlineConfiguration : IEntityTypeConfiguration<SpellOutline>
{
    public void Configure(EntityTypeBuilder<SpellOutline> builder)
    {
        builder.ToTable("Spells");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasMaxLength(32);
        
        builder.Property(x => x.Name)
               .HasMaxLength(256);

        builder.Property(x => x.Description)
               .HasMaxLength(512);

        builder.Property(x => x.ImgPath)
               .HasMaxLength(256);
    }
}