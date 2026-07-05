using IDPServer.Models;

namespace IDPServer.Services;

public sealed record OtpRequestResult(bool Succeeded, int? RetryAfterSeconds = null, string? Error = null)
{
    public static OtpRequestResult Success() => new(true);
    public static OtpRequestResult Throttled(int retryAfterSeconds) => new(false, retryAfterSeconds, "throttled");
    public static OtpRequestResult Failure(string error) => new(false, Error: error);
}

public sealed record OtpVerifyResult(bool Succeeded, ApplicationUser? User = null, string? Error = null)
{
    public static OtpVerifyResult Success(ApplicationUser user) => new(true, user);
    public static OtpVerifyResult Failure(string error) => new(false, Error: error);
}

/// <summary>
/// Drives the customer mobile+OTP login flow. Deliberately built on top of
/// ASP.NET Identity's own built-in Phone Number token provider (registered via
/// AddDefaultTokenProviders()) instead of a bespoke OTP table — the only
/// "extra" state kept here is a short-lived, in-memory resend-cooldown and
/// attempt counter (see OtpService), which doesn't need to survive a restart
/// or be shared across instances for this phase of the project.
/// </summary>
public interface IOtpService
{
    Task<OtpRequestResult> RequestOtpAsync(string phoneNumber, CancellationToken ct = default);

    Task<OtpVerifyResult> VerifyOtpAsync(string phoneNumber, string code, CancellationToken ct = default);
}
