using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace IDPServer.Models;


public sealed class ApplicationUser : IdentityUser<long>
{
    /// <summary>
    /// Coarse-grained distinction between staff (Admin panel) and Customer accounts.
    /// Roles (AspNetRoles) are only assigned to staff; customers rely on this flag
    /// plus their scopes, keeping the roles table reserved for back-office use.
    /// </summary>
    public UserType UserType { get; set; }

    /// <summary>
    /// Nullable on purpose: stays empty/unused during the current B2C-only phase.
    /// Once B2B rolls out, this becomes the FK to the future Organization/Tenant
    /// table and is the source of the "tenant_id" claim already wired in Config.cs
    /// (IdentityResource "tenant") — no breaking change to clients or tokens needed.
    /// </summary>
    public long? TenantId { get; set; }

    public bool IsBlocked { get; set; }

    public bool ForcePasswordChange { get; set; }

    /// <summary>Account creation timestamp (basic audit trail, separate from LoginAudits).</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastLoginAt { get; set; }

    

    public ICollection<LoginAudit> LoginAudits { get; set; } = [];

    public ICollection<TrustedDevice> TrustedDevices { get; set; } = [];
}