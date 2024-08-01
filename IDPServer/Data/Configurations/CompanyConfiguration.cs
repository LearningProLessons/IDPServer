using IDPServer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace IDPServer.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Company", "Sales");

        builder.Property(e => e.Id)
            .HasColumnName("ID");

        builder.Property(e => e.BusinessId)
            .HasDefaultValueSql("(newid())");

        builder.Property(e => e.CalculationCollectionPeriod)
            .HasComment("نوع محاسبه مدت وصول 1-مشتری 2- محاسباتی");

        builder.Property(e => e.CreatedByUserId)
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(250);

        builder.Property(e => e.Email)
            .HasMaxLength(100);

        builder.Property(e => e.MinGradeId)
            .HasDefaultValue(0)
            .HasComment("شناسه سازمان");

        builder.Property(e => e.ModifiedByUserId)
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .HasMaxLength(50);

        builder.Property(e => e.NationalId)
            .HasMaxLength(11)
            .HasColumnName("NationalID");

        builder.Property(e => e.Photo)
            .HasMaxLength(100);

        builder.Property(e => e.ShortName)
            .HasMaxLength(15);
    }
}
