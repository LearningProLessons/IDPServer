namespace IDPServer.Models;

/// <summary>
/// Coarse-grained account category. Kept intentionally minimal for the B2C phase —
/// fine-grained staff permissions still live in AspNetRoles (Admin, CatalogManager, ...),
/// this enum only answers "which login/claims pipeline does this account belong to".
///
/// Backed by int, so adding a value later (e.g. a future B2B business-customer type)
/// is a non-breaking, additive change — no migration required for existing rows.
/// </summary>
public enum UserType
{
    Admin = 1,
    Customer = 2

    // Reserved for the B2B phase, e.g.:
    // BusinessCustomer = 3,
}
