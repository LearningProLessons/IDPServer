namespace IDPServer.Services;

/// <summary>
/// DEV-ONLY stand-in for a real SMS gateway. Logs the message instead of
/// actually sending it, so OTP login can be tested end-to-end locally
/// without any external account/credentials.
///
/// Replace the DI registration (services.AddSingleton&lt;ISmsSender, ...&gt;
/// in HostingExtensions.cs) with a real provider before going to production —
/// nothing else needs to change.
/// </summary>
public sealed class ConsoleSmsSender : ISmsSender
{
    private readonly ILogger<ConsoleSmsSender> _logger;

    public ConsoleSmsSender(ILogger<ConsoleSmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[DEV SMS -> {PhoneNumber}] {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}
