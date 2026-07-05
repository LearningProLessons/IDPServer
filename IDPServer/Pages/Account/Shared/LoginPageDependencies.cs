using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using IDPServer.Data;
using IDPServer.Models;
using IDPServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace IDPServer.Pages.Account.Shared;
public sealed class LoginPageDependencies
{
    public required UserManager<ApplicationUser> UserManager { get; init; }

    public required SignInManager<ApplicationUser> SignInManager { get; init; }

    public required IIdentityServerInteractionService Interaction { get; init; }

    public required IEventService Events { get; init; }

    public required IAuthenticationSchemeProvider SchemeProvider { get; init; }

    public required IIdentityProviderStore IdentityProviderStore { get; init; }

    public required ApplicationDbContext DbContext { get; init; }

    public required IOtpService OtpService { get; init; }

    public required ISessionManagementService SessionManagement { get; init; }
}