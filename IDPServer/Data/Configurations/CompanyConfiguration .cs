using IDPServer.Models.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace IDPServer.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies", "Sso");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasIndex(o => o.Name)
               .IsUnique();

        // Add any additional configuration here
    }
}
