using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using IDPServer.Data;
using IDPServer.Models;
using IDPServer.Pages.Account.Login;
using IDPServer.Pages.Account.Shared;
using IDPServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IDPServer.Pages.Account.CustomerLogin;

[SecurityHeaders]
[AllowAnonymous]
public sealed class Index : LoginPageBase
{
    private readonly IOtpService _otpService;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IIdentityProviderStore _identityProviderStore;

    [BindProperty] public InputModel Input { get; set; } = new();

    public ViewModel View { get; private set; } = new();

    public Index(
        IOtpService otpService,
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
        _otpService = otpService;
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
            await Interaction.GetAuthorizationContextAsync(
                Input.ReturnUrl);

        return Input.Button switch
        {
            "request-otp"
                => await HandleRequestOtpAsync(context),

            "verify-otp"
                => await HandleVerifyOtpAsync(context),

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

    private async Task<IActionResult> HandleRequestOtpAsync(
        AuthorizationRequest? context)
    {
        View = await BuildViewModelAsync(context);

        if (string.IsNullOrWhiteSpace(Input.PhoneNumber))
        {
            ModelState.AddModelError(
                string.Empty,
                "Enter your mobile number.");

            return Page();
        }

        var result = await _otpService.RequestOtpAsync(Input.PhoneNumber);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.RetryAfterSeconds is { } seconds
                    ? $"Please wait {seconds} seconds before requesting another code."
                    : "Unable to send the verification code. Please try again.");

            return Page();
        }

        View.OtpSent = true;
        View.PhoneNumber = Input.PhoneNumber;

        return Page();
    }

    private async Task<IActionResult> HandleVerifyOtpAsync(
        AuthorizationRequest? context)
    {
        View = await BuildViewModelAsync(context);

        View.OtpSent = true;
        View.PhoneNumber = Input.PhoneNumber;

        if (string.IsNullOrWhiteSpace(Input.PhoneNumber) ||
            string.IsNullOrWhiteSpace(Input.OtpCode))
        {
            ModelState.AddModelError(
                string.Empty,
                "Enter the verification code.");

            return Page();
        }

        var result = await _otpService.VerifyOtpAsync(
            Input.PhoneNumber,
            Input.OtpCode);

        if (!result.Succeeded)
        {
            if (result.User != null)
            {
                await RecordLoginAuditAsync(
                    result.User,
                    LoginMethod.Otp,
                    succeeded: false,
                    failureReason: result.Error,
                    context);
            }

            ModelState.AddModelError(
                string.Empty,
                result.Error switch
                {
                    "too_many_attempts"
                        => "Too many incorrect attempts. Please request a new verification code.",

                    "account_blocked"
                        => LoginOptions.AccountBlockedErrorMessage,

                    "expired"
                        => "The verification code has expired.",

                    _ => "Invalid verification code."
                });

            return Page();
        }

        var user = result.User!;

        await SignInManager.SignInAsync(
            user,
            isPersistent: true);

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
            LoginMethod.Otp,
            succeeded: true,
            failureReason: null,
            context);

        await UpdateLastLoginAsync(user);

        return RedirectAfterLogin(
            context,
            Input.ReturnUrl);
    }

    private async Task BuildModelAsync(string? returnUrl)
    {
        Input.ReturnUrl = returnUrl;

        var context =
            await Interaction.GetAuthorizationContextAsync(returnUrl);

        View = await BuildViewModelAsync(context);

        // Restore OTP state after a successful POST (PRG pattern)
        if (TempData.TryGetValue("OtpSent", out var otpSent))
        {
            View.OtpSent = bool.TryParse(
                otpSent?.ToString(),
                out var sent) && sent;
        }

        if (TempData.TryGetValue("PhoneNumber", out var phoneNumber))
        {
            var value = phoneNumber?.ToString();

            View.PhoneNumber = value;
            Input.PhoneNumber = value;
        }
    }

    private async Task<ViewModel> BuildViewModelAsync(
        AuthorizationRequest? context)
    {
        // Client explicitly requested an external identity provider.
        if (context?.IdP != null &&
            await _schemeProvider.GetSchemeAsync(context.IdP) != null)
        {
            var local =
                context.IdP == IdentityServerConstants.LocalIdentityProvider;

            return new ViewModel
            {
                EnableLocalLogin = local,

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
                .Select(x => new ViewModel.ExternalProvider(
                    x.Name,
                    x.DisplayName))
                .ToList();

        var dynamicProviders =
            (await _identityProviderStore.GetAllSchemeNamesAsync())
            .Where(x => x.Enabled)
            .Select(x => new ViewModel.ExternalProvider(
                x.Scheme,
                x.DisplayName ?? x.Scheme));

        providers.AddRange(dynamicProviders);

        var allowLocal = true;

        var client = context?.Client;

        if (client != null)
        {
            allowLocal = client.EnableLocalLogin;

            if (client.IdentityProviderRestrictions is { Count: > 0 })
            {
                providers = providers
                    .Where(x =>
                        client.IdentityProviderRestrictions.Contains(
                            x.AuthenticationScheme))
                    .ToList();
            }
        }

        return new ViewModel
        {
            EnableLocalLogin =
                allowLocal &&
                LoginOptions.AllowLocalLogin,

            ExternalProviders = providers,

            OtpSent = false,
            PhoneNumber = null
        };
    }
}