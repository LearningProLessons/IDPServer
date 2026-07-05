using Microsoft.AspNetCore.Identity;

namespace IDPServer.Models;

public sealed class ApplicationRole : IdentityRole<long>
{
    public ApplicationRole() : base() {}
    public ApplicationRole(string roleName) : base(roleName) {}
}