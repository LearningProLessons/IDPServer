using Microsoft.AspNetCore.Identity;

namespace IDPServer.Models;

public class ApplicationRole : IdentityRole<int>
{
    // Default constructor for EF
    public ApplicationRole() : base() { }

    // Constructor with role name
    public ApplicationRole(string roleName) : base(roleName) { }
    public int MenuId { get; set; }
    public int CompanyId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}
