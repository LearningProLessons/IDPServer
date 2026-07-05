namespace IDPServer.Pages.Account.AdminLogin;

public sealed class ViewModel
{
    public bool AllowRememberLogin { get; init; }

    public bool EnableLocalLogin { get; init; }

    public bool AwaitingTwoFactor { get; set; }

    public IEnumerable<ExternalProvider> VisibleExternalProviders =>
        ExternalProviders.Where(x => !string.IsNullOrWhiteSpace(x.DisplayName));

    public bool IsExternalLoginOnly =>
        EnableLocalLogin == false &&
        ExternalProviders.Count() == 1;

    public string? ExternalLoginScheme =>
        IsExternalLoginOnly
            ? ExternalProviders.Single().AuthenticationScheme
            : null;

    public IEnumerable<ExternalProvider> ExternalProviders { get; init; }
        = Enumerable.Empty<ExternalProvider>();

    public sealed record ExternalProvider(
        string AuthenticationScheme,
        string? DisplayName = null);
}