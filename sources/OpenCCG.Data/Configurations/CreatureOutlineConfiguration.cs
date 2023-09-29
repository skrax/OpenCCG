using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OpenCCG.Data.Configurations;

public class CreatureOutlineConfiguration : IEntityTypeConfiguration<Proto.CreatureOutline>
{
    public void Configure(EntityTypeBuilder<Proto.CreatureOutline> builder)
    {
        builder.ToTable("Creatures");

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