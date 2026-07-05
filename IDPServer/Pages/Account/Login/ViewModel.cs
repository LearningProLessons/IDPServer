// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace IDPServer.Pages.Account.Login
{
    public class ViewModel
    {
        /// <summary>
        /// "admin" or "otp" — read from the requesting Client's
        /// Properties["login_ui"] in Config.cs. Decides which form renders.
        /// </summary>
        public string LoginUi { get; set; } = "admin";

        public bool AllowRememberLogin { get; set; } = true;
        public bool EnableLocalLogin { get; set; } = true;

        /// <summary>Admin flow reached the optional TOTP second step.</summary>
        public bool AwaitingTwoFactor { get; set; }

        /// <summary>Customer flow: an OTP was just sent, show the code-entry form.</summary>
        public bool OtpSent { get; set; }
        public string? OtpPhoneNumber { get; set; }

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();
        public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        // Only short-circuit straight to an external IdP for the admin login_ui —
        // the customer OTP client never sets up external providers in this scenario.
        public bool IsExternalLoginOnly => LoginUi == "admin" && EnableLocalLogin == false && ExternalProviders?.Count() == 1;
        public string? ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;

        public class ExternalProvider
        {
            public ExternalProvider(string authenticationScheme, string? displayName = null)
            {
                AuthenticationScheme = authenticationScheme;
                DisplayName = displayName;
            }

            public string? DisplayName { get; set; }
            public string AuthenticationScheme { get; set; }
        }
    }
}
