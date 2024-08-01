using IDPServer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace IDPServer.Data.Configurations;

public class AddressUserConfiguration : IEntityTypeConfiguration<AddressUser>
{
    public void Configure(EntityTypeBuilder<AddressUser> builder)
    {
        builder.ToTable("AddressUser", "Global");

        builder.Property(e => e.BusinessId)
            .HasDefaultValueSql("(newid())");

        builder.Property(e => e.CreatedByUserId)
            .HasMaxLength(50);

        builder.Property(e => e.IsDefault)
            .HasDefaultValue(0);

        builder.Property(e => e.ModifiedByUserId)
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .HasDefaultValue(1);
    }
}
