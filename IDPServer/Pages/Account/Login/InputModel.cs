// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace IDPServer.Pages.Account.Login
{
    /// <summary>
    /// One shared model bound across every form on this page (admin password,
    /// admin TOTP, customer OTP request/verify). Deliberately NOT decorated with
    /// [Required] anymore — which fields are mandatory depends on which "Button"
    /// was clicked, so each handler in Index.cshtml.cs validates its own fields.
    /// </summary>
    public class InputModel
    {
        // Admin: email + password
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool RememberLogin { get; set; }

        // Admin: optional TOTP second factor
        public string? TwoFactorCode { get; set; }
        public bool RememberMachine { get; set; }

        // Customer: mobile number + OTP
        public string? PhoneNumber { get; set; }
        public string? OtpCode { get; set; }

        public string? ReturnUrl { get; set; }
        public string? Button { get; set; }
    }
}
