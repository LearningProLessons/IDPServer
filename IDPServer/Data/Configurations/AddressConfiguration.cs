using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using IDPServer.Models;

namespace IDPServer.Data.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Address", "Global");

        builder.Property(e => e.Address1)
            .HasMaxLength(250)
            .HasColumnName("Address");

        builder.Property(e => e.BusinessId)
            .HasDefaultValueSql("(newid())");

        builder.Property(e => e.CreatedByUserId)
            .HasMaxLength(50);

        builder.Property(e => e.ModifiedByUserId)
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .HasMaxLength(50);

        builder.Property(e => e.PostalCode)
            .HasMaxLength(10)
            .IsUnicode(false);

        builder.Property(e => e.Telephone)
            .HasMaxLength(8)
            .IsUnicode(false);
    }
}
