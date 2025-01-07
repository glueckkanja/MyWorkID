namespace MyWorkID.Server.Features.ResetPassword.Entities
{
    /// <summary>
    /// Represents a request to reset a user's password.
    /// </summary>
    public class PasswordResetRequest
    {
        /// <summary>
        /// Gets or sets the new password for the user.
        /// </summary>
        public string? NewPassword { get; set; }
    }
}
