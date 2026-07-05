using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using IDPServer.Data;
using IDPServer.Models;
using IDPServer.Pages.Account.Login;
using IDPServer.Pages.Account.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IDPServer.Pages.Account.AdminLogin;

[SecurityHeaders]
[AllowAnonymous]
public sealed class Index : LoginPageBase
{
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IIdentityProviderStore _identityProviderStore;
    private readonly ISessionManagementService _sessionManagementService;

    [BindProperty] public InputModel Input { get; set; } = new();

    public ViewModel View { get; private set; } = new();

    public Index(
        ISessionManagementService sessionManagementService,
        IIdentityServerInteractionService interaction,
        IAuthenticationSchemeProvider schemeProvider,
        IIdentityProviderStore identityProviderStore,
        IEventService events,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext dbContext)
        : base(
            userManager,
            signInManager,
            interaction,
            events,
            dbContext)
    {
        _sessionManagementService = sessionManagementService;
        _schemeProvider = schemeProvider;
        _identityProviderStore = identityProviderStore;
    }

    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        await BuildModelAsync(returnUrl);

        if (View.IsExternalLoginOnly)
        {
            return RedirectToPage(
                "/ExternalLogin/Challenge",
                new { scheme = View.ExternalLoginScheme, returnUrl });
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var context =
            await Interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        return Input.Button switch
        {
            "login"
                => await HandlePasswordLoginAsync(context),

            "verify-2fa"
                => await HandleTwoFactorAsync(context),

            "cancel"
                => await HandleCancelAsync(
                    context,
                    Input.ReturnUrl),

            _
                => await HandleCancelAsync(
                    context,
                    Input.ReturnUrl)
        };
    }

    private async Task<IActionResult> HandlePasswordLoginAsync(
        AuthorizationRequest? context)
    {
        if (string.IsNullOrWhiteSpace(Input.Username) ||
            string.IsNullOrWhiteSpace(Input.Password))
        {
            await BuildModelAsync(Input.ReturnUrl);

            ModelState.AddModelError(
                string.Empty,
                LoginOptions.InvalidCredentialsErrorMessage);

            return Page();
        }

        var user =
            await UserManager.FindByNameAsync(Input.Username);

        if (user is not null && user.IsBlocked)
        {
            await RecordLoginAuditAsync(
                user,
                LoginMethod.Password,
                succeeded: false,
                failureReason: "account_blocked",
                context);

            await BuildModelAsync(Input.ReturnUrl);

            ModelState.AddModelError(
                string.Empty,
                LoginOptions.AccountBlockedErrorMessage);

            return Page();
        }

        var result =
            await SignInManager.PasswordSignInAsync(
                Input.Username,
                Input.Password,
                Input.RememberLogin,
                lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
        {
            View = await BuildViewModelAsync(context);

            View.AwaitingTwoFactor = true;

            Input.Password = string.Empty;

            return Page();
        }

        if (result.Succeeded)
        {
            if (user == null)
            {
                throw new InvalidOperationException(
                    $"User '{Input.Username}' authenticated successfully but could not be loaded.");
            }

            await Events.RaiseAsync(
                new UserLoginSuccessEvent(
                    user.UserName,
                    user.Id.ToString(),
                    user.UserName,
                    clientId: context?.Client.ClientId));

            Telemetry.Metrics.UserLogin(
                context?.Client.ClientId,
                IdentityServerConstants.LocalIdentityProvider);

            await RecordLoginAuditAsync(
                user,
                LoginMethod.Password,
                succeeded: true,
                failureReason: null,
                context);

            await UpdateLastLoginAsync(user);

            return RedirectAfterLogin(
                context,
                Input.ReturnUrl);
        }

        var reason =
            result.IsLockedOut
                ? "locked_out"
                : "invalid_credentials";

        await RecordLoginAuditAsync(
            user,
            LoginMethod.Password,
            succeeded: false,
            failureReason: reason,
            context);

        await Events.RaiseAsync(
            new UserLoginFailureEvent(
                Input.Username,
                reason,
                clientId: context?.Client.ClientId));

        Telemetry.Metrics.UserLoginFailure(
            context?.Client.ClientId,
            IdentityServerConstants.LocalIdentityProvider,
            reason);

        await BuildModelAsync(Input.ReturnUrl);

        ModelState.AddModelError(
            string.Empty,
            result.IsLockedOut
                ? LoginOptions.LockedOutErrorMessage
                : LoginOptions.InvalidCredentialsErrorMessage);

        return Page();
    }

    private async Task<IActionResult> HandleTwoFactorAsync(
        AuthorizationRequest? context)
    {
        var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            await BuildModelAsync(Input.ReturnUrl);

            ModelState.AddModelError(
                string.Empty,
                "Your sign-in session has expired. Please sign in again.");

            return Page();
        }

        if (string.IsNullOrWhiteSpace(Input.TwoFactorCode))
        {
            View = await BuildViewModelAsync(context);

            View.AwaitingTwoFactor = true;

            ModelState.AddModelError(
                string.Empty,
                "Enter the code from your authenticator application.");

            return Page();
        }

        var result =
            await SignInManager.TwoFactorAuthenticatorSignInAsync(
                Input.TwoFactorCode,
                isPersistent: Input.RememberLogin,
                rememberClient: Input.RememberMachine);

        if (result.Succeeded)
        {
            await Events.RaiseAsync(
                new UserLoginSuccessEvent(
                    user.UserName,
                    user.Id.ToString(),
                    user.UserName,
                    clientId: context?.Client.ClientId));

            Telemetry.Metrics.UserLogin(
                context?.Client.ClientId,
                IdentityServerConstants.LocalIdentityProvider);

            await RecordLoginAuditAsync(
                user,
                LoginMethod.Password,
                succeeded: true,
                failureReason: null,
                context);

            await UpdateLastLoginAsync(user);

            return RedirectAfterLogin(
                context,
                Input.ReturnUrl);
        }

        var reason =
            result.IsLockedOut
                ? "locked_out"
                : "invalid_totp";

        await RecordLoginAuditAsync(
            user,
            LoginMethod.Password,
            succeeded: false,
            failureReason: reason,
            context);

        View = await BuildViewModelAsync(context);

        View.AwaitingTwoFactor = !result.IsLockedOut;

        ModelState.AddModelError(
            string.Empty,
            result.IsLockedOut
                ? LoginOptions.LockedOutErrorMessage
                : "Invalid authenticator code.");

        return Page();
    }

    private async Task BuildModelAsync(string? returnUrl)
    {
        Input.ReturnUrl = returnUrl;

        var context =
            await Interaction.GetAuthorizationContextAsync(returnUrl);

        View = await BuildViewModelAsync(context);

        if (context?.IdP != null)
        {
            var scheme =
                await _schemeProvider.GetSchemeAsync(context.IdP);

            if (scheme != null)
            {
                Input.Username = context.LoginHint ?? string.Empty;
            }
        }
    }

    private async Task<ViewModel> BuildViewModelAsync(
        AuthorizationRequest? context)
    {
        // External IdP explicitly requested
        if (context?.IdP != null &&
            await _schemeProvider.GetSchemeAsync(context.IdP) != null)
        {
            var local =
                context.IdP == IdentityServerConstants.LocalIdentityProvider;

            return new ViewModel
            {
                EnableLocalLogin = local,
                AllowRememberLogin = LoginOptions.AllowRememberLogin,
                ExternalProviders = local
                    ? Enumerable.Empty<ViewModel.ExternalProvider>()
                    : new[]
                    {
                        new ViewModel.ExternalProvider(
                            context.IdP)
                    }
            };
        }

        var schemes =
            await _schemeProvider.GetAllSchemesAsync();

        var providers =
            schemes
                .Where(x => x.DisplayName != null)
                .Select(x =>
                    new ViewModel.ExternalProvider(
                        x.Name,
                        x.DisplayName))
                .ToList();

        var dynamicProviders =
            (await _identityProviderStore.GetAllSchemeNamesAsync())
            .Where(x => x.Enabled)
            .Select(x =>
                new ViewModel.ExternalProvider(
                    x.Scheme,
                    x.DisplayName ?? x.Scheme));

        providers.AddRange(dynamicProviders);

        var allowLocal = true;

        var client = context?.Client;

        if (client != null)
        {
            allowLocal = client.EnableLocalLogin;

            if (client.IdentityProviderRestrictions.Count > 0)
            {
                providers =
                    providers
                        .Where(x =>
                            client.IdentityProviderRestrictions.Contains(
                                x.AuthenticationScheme))
                        .ToList();
            }
        }

        return new ViewModel
        {
            AllowRememberLogin = LoginOptions.AllowRememberLogin,
            EnableLocalLogin =
                allowLocal &&
                LoginOptions.AllowLocalLogin,
            ExternalProviders = providers
        };
    }
}