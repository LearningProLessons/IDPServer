using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IDPServer.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IDPServer.Services;

public sealed class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await _userManager.GetUserAsync(context.Subject);
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim("tenant_id", user.BranchId.ToString()), new Claim("role", roles.FirstOrDefault() ?? "")
        };

        context.IssuedClaims.AddRange(claims);
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var user = await _userManager.GetUserAsync(context.Subject);
        context.IsActive = user != null;
    }
}