using DutyFinderService.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DutyFinderService.Db.Mappings;

public class ImageMapping : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(256);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.ImageUrl).HasMaxLength(256);
        builder.Property(x => x.LastUpdatedPatch).HasMaxLength(8);
    }
}