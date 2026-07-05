using System.ComponentModel.DataAnnotations;

namespace IDPServer.Pages.Account.AdminLogin;

public sealed class InputModel
{
    public string? ReturnUrl { get; set; }

    public string? Button { get; set; }

    [Display(Name = "Username")]
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Password")]
    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberLogin { get; set; }

    [Display(Name = "Authenticator Code")]
    public string? TwoFactorCode { get; set; }

    [Display(Name = "Remember this device")]
    public bool RememberMachine { get; set; }
}