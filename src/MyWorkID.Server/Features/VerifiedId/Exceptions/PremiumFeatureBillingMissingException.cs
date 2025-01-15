namespace MyWorkID.Server.Features.VerifiedId.Exceptions
{
    public class PremiumFeatureBillingMissingException : Exception
    {
        public PremiumFeatureBillingMissingException()
        {
        }

        public PremiumFeatureBillingMissingException(string? message) : base(message)
        {
        }

        public PremiumFeatureBillingMissingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
