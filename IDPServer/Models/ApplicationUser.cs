// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Identity;

namespace IDPServer.Models;
// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? UserTypeId { get; set; }
    public int? ShopCategoryId { get; set; }
    public int? ShopActivityId { get; set; }
    public int? GenderId { get; set; }
    public string? IdentityId { get; set; }
    public string? NationalCode { get; set; }
    public string? ShopName { get; set; }
    public DateTime? Birthday { get; set; }
    public string? CcPhoto { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? CreatedDateTime { get; set; }
    public int? ModifiedByUserId { get; set; }
    public DateTime? ModifiedDateTime { get; set; }
    public Guid? BusinessId { get; set; }
}
