namespace c4a8.MyAccountVNext.Server.Features.ResetPassword.Entities
{
    public class PasswordResetRequest
    {
        [ValidatePassword]
        public string NewPassword { get; set; }
    }
}
