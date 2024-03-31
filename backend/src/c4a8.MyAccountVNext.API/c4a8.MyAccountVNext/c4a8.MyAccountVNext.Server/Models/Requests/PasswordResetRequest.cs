using c4a8.MyAccountVNext.Server.Models.Validation;

namespace c4a8.MyAccountVNext.Server.Models.Requests
{
    public class PasswordResetRequest
    {
        [ValidatePassword]
        public string NewPassword { get; set; }
    }
}
