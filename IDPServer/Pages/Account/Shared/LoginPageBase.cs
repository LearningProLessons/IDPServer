using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IDPServer.Data;
using IDPServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IDPServer.Pages.Account.Shared;

public abstract class LoginPageBase : PageModel
{
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly SignInManager<ApplicationUser> SignInManager;
    protected readonly IIdentityServerInteractionService Interaction;
    protected readonly IEventService Events;
    protected readonly ApplicationDbContext DbContext;

    protected LoginPageBase(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IIdentityServerInteractionService interaction,
        IEventService events,
        ApplicationDbContext dbContext)
    {
        UserManager = userManager;
        SignInManager = signInManager;
        Interaction = interaction;
        Events = events;
        DbContext = dbContext;
    }

    protected async Task<IActionResult> HandleCancelAsync(
        AuthorizationRequest? context,
        string? returnUrl)
    {
        if (context != null)
        {
            ArgumentNullException.ThrowIfNull(returnUrl);

            await Interaction.DenyAuthorizationAsync(
                context,
                AuthorizationError.AccessDenied);

            if (context.IsNativeClient())
            {
                return this.LoadingPage(returnUrl);
            }

            return Redirect(returnUrl);
        }

        return Redirect("~/");
    }

    protected IActionResult RedirectAfterLogin(
        AuthorizationRequest? context,
        string? returnUrl)
    {
        if (context != null)
        {
            ArgumentNullException.ThrowIfNull(returnUrl);

            if (context.IsNativeClient())
            {
                return this.LoadingPage(returnUrl);
            }

            return Redirect(returnUrl);
        }

        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl!);
        }

        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return Redirect("~/");
        }

        throw new ArgumentException("Invalid return url.");
    }

    protected async Task UpdateLastLoginAsync(ApplicationUser user)
    {
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await UserManager.UpdateAsync(user);
    }

    protected async Task RecordLoginAuditAsync(
        ApplicationUser? user,
        LoginMethod method,
        bool succeeded,
        string? failureReason,
        AuthorizationRequest? context)
    {
        if (user == null)
            return;

        DbContext.LoginAudits.Add(new LoginAudit
        {
            UserId = user.Id,
            ClientId = context?.Client.ClientId,
            Method = method,
            Succeeded = succeeded,
            FailureReason = failureReason,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            OccurredAt = DateTimeOffset.UtcNow
        });

        await DbContext.SaveChangesAsync();
    }
}