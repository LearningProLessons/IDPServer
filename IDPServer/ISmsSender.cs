namespace IDPServer.Services;

/// <summary>
/// Port for sending SMS messages. Swap the registered implementation (see
/// HostingExtensions.cs) for a real gateway (Kavenegar, Ghasedak, Twilio, ...)
/// later — nothing in the OTP/login flow needs to change, since callers only
/// ever depend on this interface.
/// </summary>
public interface ISmsSender
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
