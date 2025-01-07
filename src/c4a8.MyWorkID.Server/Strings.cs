namespace c4a8.MyWorkID.Server
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
        public const string CREATE_TAP_ROLE = "MyWorkID.CreateTAP";
        public const string RESET_PASSWORD_ROLE = "MyWorkID.PasswordReset";
        public const string DISMISS_USER_RISK_ROLE = "MyWorkID.DismissUserRisk";
        public const string VALIDATE_IDENTITY_ROLE = "MyWorkID.ValidateIdentity";
        public const string PASSWORD_VALIDATION_MISSING_ERROR = "Password is required.";
        public const string PASSWORD_VALIDATION_LENGTH_ERROR = "Password must be between 8 and 255 characters.";
        public const string PASSWORD_VALIDATION_SYMBOLS_ERROR = "Please use characters from at least 3 of these groups: lowercase, uppercase, digits, special symbols.";
        public const string JWT_SIGNING_KEY_DEFAULT = "GodDamnitYouForgottToSpecifyASigningKey";
        public const string ERROR_INVALID_BODY = "Parsed response resulted in error or null object.";
        public const string ERROR_MISSING_OR_INVALID_SETTINGS_GENERATE_TAP = "Missing or invalid AppFunctions:GenerateTap auth context setting.";
        public const string ERROR_MISSING_OR_INVALID_SETTINGS_DISMISS_USER_RISK = "Missing or invalid AppFunctions:DismissUserRisk auth context setting.";
        public const string ERROR_MISSING_OR_INVALID_SETTINGS_RESET_PASSWORD = "Missing or invalid AppFunctions:ResetPassword auth context setting.";
        public const string ERROR_MISSING_OR_INVALID_SETTINGS_VERIFIED_ID = "Missing or invalid configuration for VerifiedId.";
    }
}
