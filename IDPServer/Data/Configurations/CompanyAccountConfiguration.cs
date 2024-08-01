using IDPServer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace IDPServer.Data.Configurations;

public class CompanyAccountConfiguration : IEntityTypeConfiguration<CompanyAccount>
{
    public void Configure(EntityTypeBuilder<CompanyAccount> builder)
    {
        builder.ToTable("CompanyAccount", "Sales");

        builder.Property(e => e.EndDate)
            .HasColumnType("datetime");

        builder.Property(e => e.FromDate)
            .HasColumnType("datetime");
    }
}