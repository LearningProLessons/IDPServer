namespace IDPServer.Models;

/// <summary>
/// One row per login ATTEMPT (both successful and failed), for both login pipelines
/// (admin email+password and customer mobile+OTP). Distinguishing them by ClientId
/// lets a single table serve both audiences without branching schema.
/// </summary>
public sealed class LoginAudit
{
    public long Id { get; set; }

    public long UserId { get; set; }
    public IDPServer.Models.ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// The IdentityServer Client.ClientId that initiated the login, e.g.
    /// "ecommerce-admin-panel" or "ecommerce-customer-app". Nullable to cover
    /// edge cases (e.g. an attempt rejected before the client context was resolved).
    /// </summary>
    public string? ClientId { get; set; }

    public LoginMethod Method { get; set; }

    public bool Succeeded { get; set; }

    /// <summary>
    /// Only populated when Succeeded == false, e.g. "invalid_password", "invalid_otp",
    /// "otp_expired", "account_blocked", "account_locked_out".
    /// </summary>
    public string? FailureReason { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}

public enum LoginMethod
{
    Password = 1,
    Otp = 2
}
