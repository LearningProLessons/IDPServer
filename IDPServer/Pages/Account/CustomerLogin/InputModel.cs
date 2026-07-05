using System.ComponentModel.DataAnnotations;

namespace IDPServer.Pages.Account.CustomerLogin;

public sealed class InputModel
{
    public string? ReturnUrl { get; set; }

    public string? Button { get; set; }

    [Display(Name = "Mobile Number")]
    [Required(ErrorMessage = "Mobile number is required.")]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Verification Code")]
    [StringLength(6, MinimumLength = 6)]
    public string? OtpCode { get; set; }

    public bool RememberLogin { get; set; }
}