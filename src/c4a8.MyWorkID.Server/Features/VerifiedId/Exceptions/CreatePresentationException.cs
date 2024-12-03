namespace c4a8.MyWorkID.Server.Features.VerifiedId.Exceptions
{
    public class CreatePresentationException : Exception
    {
        private const string DefaultMessage = Strings.ERROR_INVALID_BODY;

        public CreatePresentationException()
        : base(DefaultMessage)
        {
        }

        public CreatePresentationException(string message)
         : base(string.IsNullOrEmpty(message) ? DefaultMessage : message)
        {
        }
    }
}
