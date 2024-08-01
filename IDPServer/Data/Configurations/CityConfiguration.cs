using IDPServer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace IDPServer.Data.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("County", "Global");

        builder.Property(e => e.BusinessId)
            .HasDefaultValueSql("(newid())");

        builder.Property(e => e.CreatedByUserId)
            .HasMaxLength(50);

        builder.Property(e => e.ModifiedByUserId)
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .HasMaxLength(80)
            .IsUnicode(false);
    }
}