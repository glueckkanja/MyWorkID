namespace c4a8.MyAccountVNext.Server
{
    public static class Strings
    {
        public const string ERROR_INSUFFICIENT_CLAIMS = "The presented access tokens had insufficient claims. Please request for claims requested in the WWW-Authentication header and try again.";
        public const string ERROR_UNABLE_TO_GENERATE_TAP = "Unable to generate TAP";
        public const string VERIFIED_ID_CALLBACK_SCHEMA = "VerifiedIdCallbackSchema";
        public const string VERIFIED_ID_CALLBACK_POLICY = "VerifiedIdCallbackPolicy";
        public const string SIGNALR_WEBHOOK_SCHEMA = "SignalRWebhookSchema";
        public const string USERRISKSTATE_OPENAPI_TAG = "Risk State";
        public const string VERIFIEDID_OPENAPI_TAG = "Verified Id";
        public const string RESET_PASSWORD_OPENAPI_TAG = "Reset Password";
        public const string CREATE_TAP_ROLE = "MyAccount.VNext.CreateTAP";
        public const string RESET_PASSWORD_ROLE = "MyAccount.VNext.PasswordReset";
        public const string DISMISS_USER_RISK_ROLE = "MyAccount.VNext.DismissUserRisk";
        public const string VALIDATE_IDENTITY_ROLE = "MyAccount.VNext.ValidateIdentity";
        public const string PASSWORD_VALIDATION_MISSING_ERROR = "Password is required.";
        public const string PASSWORD_VALIDATION_LENGTH_ERROR = "Password must be between 8 and 255 characters.";
        public const string PASSWORD_VALIDATION_SYMBOLS_ERROR = "Please use characters from at least 3 of these groups: lowercase, uppercase, digits, special symbols.";
        public const string JWT_SIGNING_KEY_DEFAULT = "GodDamnitYouForgottToSpecifyASigningKey";
        public const string ERROR_INVALID_BODY = "Invalid body.";
    }
}
