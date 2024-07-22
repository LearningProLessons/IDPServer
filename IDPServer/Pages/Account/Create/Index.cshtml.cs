// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;
using IDPServer.Data;
using IDPServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Formats.Asn1.AsnWriter;

namespace IDPServer.Pages.Account.Create
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class Index : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IIdentityServerInteractionService _interaction;

        [BindProperty]
        public InputModel Input { get; set; } = default!;


        public Index(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IIdentityServerInteractionService interaction)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _interaction = interaction;
        }

        public IActionResult OnGet(string? returnUrl)
        {
            Input = new InputModel { ReturnUrl = returnUrl };
            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            // Check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

            // The user clicked the "cancel" button
            if (Input.Button != "create")
            {
                if (context != null)
                {
                    // If the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // This will send back an access denied OIDC error response to the client.
                    await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                    // We can trust Input.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage(Input.ReturnUrl);
                    }

                    return Redirect(Input.ReturnUrl ?? "~/");
                }
                else
                {
                    // Since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Username,
                    Email = Input.Email,
                    NormalizedUserName = Input.Name,
                };

                // Create the user asynchronously
                var createResult = await _userManager.CreateAsync(user, Input.Password!);

                if (!createResult.Succeeded)
                {
                    // Handle errors from user creation
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                var companyUserRole = await _roleManager.FindByNameAsync("company_user");
                if (companyUserRole == null)
                {
                    companyUserRole = new IdentityRole("company_user");
                    var roleResult = await _roleManager.CreateAsync(companyUserRole);
                    if (!roleResult.Succeeded)
                    {
                        // Handle errors from role creation
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return Page();
                    }
                }

                // Assign the 'company_user' role to the newly created user
                var userRoleResult = await _userManager.AddToRoleAsync(user, "company_user");
                if (!userRoleResult.Succeeded)
                {
                    // Handle errors from role assignment
                    foreach (var error in userRoleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                // User created and role assigned successfully
                var isUser = new IdentityServerUser(user.Id)
                {
                    DisplayName = user.UserName
                };

                await HttpContext.SignInAsync(isUser);

                if (context != null)
                {
                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage(Input.ReturnUrl);
                    }

                    // We can trust Input.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(Input.ReturnUrl ?? "~/");
                }

                // Request for a local page
                if (Url.IsLocalUrl(Input.ReturnUrl))
                {
                    return Redirect(Input.ReturnUrl);
                }
                else if (string.IsNullOrEmpty(Input.ReturnUrl))
                {
                    return Redirect("~/");
                }
                else
                {
                    // User might have clicked on a malicious link - should be logged
                    throw new ArgumentException("invalid return URL");
                }
            }

            // If the model state is not valid, re-display the form
            return Page();
        }


    }
}
