using IDPServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDPServer.Data;

public sealed class TrustedDeviceConfiguration : IEntityTypeConfiguration<TrustedDevice>
{
    public void Configure(EntityTypeBuilder<TrustedDevice> builder)
    {
        builder.ToTable("TrustedDevices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DeviceId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.DeviceName).HasMaxLength(200);

        builder.HasOne(x => x.User)
            .WithMany(u => u.TrustedDevices)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // A given device is only ever trusted once per user.
        builder.HasIndex(x => new { x.UserId, x.DeviceId }).IsUnique();
    }
}
