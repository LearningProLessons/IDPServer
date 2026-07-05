namespace IDPServer.Models;

/// <summary>
/// Supports a "remember this device" pattern — mainly for the customer OTP flow
/// (skip re-sending an OTP on a device that already proved itself recently), but
/// usable for the admin's optional 2FA the same way. One row per device per user.
/// </summary>
public sealed class TrustedDevice
{
    public long Id { get; set; }

    public long UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Stable client-generated identifier — an installation id on the React Native
    /// app, or a signed persistent cookie value on the Next.js admin app. Never a
    /// raw hardware/device serial number.
    /// </summary>
    public string DeviceId { get; set; } = null!;

    /// <summary>Human-friendly label, e.g. "iPhone 14 Pro" or "Chrome on Windows".</summary>
    public string? DeviceName { get; set; }

    public DevicePlatform Platform { get; set; }

    /// <summary>
    /// While this is in the future, OTP/2FA can be skipped for this device.
    /// Null or past = device is not (or no longer) trusted, and the full
    /// login challenge (OTP / 2FA) is required again.
    /// </summary>
    public DateTimeOffset? TrustedUntil { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastUsedAt { get; set; }

    /// <summary>Lets a user (or an admin, for a customer) manually revoke a device, e.g. after a phone is lost.</summary>
    public bool IsRevoked { get; set; }
}

public enum DevicePlatform
{
    Unknown = 0,
    Ios = 1,
    Android = 2,
    Web = 3
}
