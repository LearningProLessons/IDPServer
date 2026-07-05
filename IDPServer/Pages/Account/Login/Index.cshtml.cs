using Duende.IdentityServer.Services;
using IDPServer.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDPServer.Pages.Account.Login;

[SecurityHeaders]
[AllowAnonymous]
public sealed class Index : PageModel
{
    private readonly IIdentityServerInteractionService _interaction;

    public Index(
        IIdentityServerInteractionService interaction)
    {
        _interaction = interaction;
    }

    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        var context =
            await _interaction.GetAuthorizationContextAsync(returnUrl);

        if (context == null)
        {
            return Redirect("~/");
        }

        var authenticationFlow =
            context.Client.Properties.TryGetValue(
                ClientPropertyNames.AuthenticationFlow,
                out var value)
                ? value
                : AuthenticationFlows.Password;

        return authenticationFlow switch
        {
            AuthenticationFlows.Password =>
                RedirectToPage(
                    "/Account/AdminLogin/Index",
                    new { returnUrl }),

            AuthenticationFlows.Otp =>
                RedirectToPage(
                    "/Account/CustomerLogin/Index",
                    new { returnUrl }),

            _ => throw new InvalidOperationException(
                $"Unsupported authentication flow '{authenticationFlow}' for client '{context.Client.ClientId}'.")
        };
    }
}