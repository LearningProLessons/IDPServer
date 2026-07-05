// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace IDPServer.Pages.Account.Login
{
    public static class LoginOptions
    {
        public static readonly bool AllowLocalLogin = true;
        public static readonly bool AllowRememberLogin = true;
        public static readonly TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

        public static readonly string InvalidCredentialsErrorMessage = "Invalid username or password";
        public static readonly string LockedOutErrorMessage = "This account is temporarily locked due to repeated failed attempts.";

        // Deliberately identical to the generic invalid-credentials message —
        // don't reveal to an anonymous caller *why* a login failed.
        public static readonly string AccountBlockedErrorMessage = "Invalid username or password";
    }
}
