using IDPServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDPServer.Data;

public sealed class LoginAuditConfiguration : IEntityTypeConfiguration<LoginAudit>
{
    public void Configure(EntityTypeBuilder<LoginAudit> builder)
    {
        builder.ToTable("LoginAudits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientId).HasMaxLength(200);
        builder.Property(x => x.FailureReason).HasMaxLength(200);
        builder.Property(x => x.IpAddress).HasMaxLength(45); // fits IPv6
        builder.Property(x => x.UserAgent).HasMaxLength(512);

        builder.HasOne(x => x.User)
            .WithMany(u => u.LoginAudits)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Main query pattern: "recent login history for this user, newest first".
        builder.HasIndex(x => new { x.UserId, x.OccurredAt });
    }
}
