namespace c4a8.MyWorkID.Server.Features.VerifiedId.Exceptions
{
    public class CreatePresentationException : Exception
    {
        private const string DEFAULT_MESSAGE = Strings.ERROR_INVALID_BODY;

        public CreatePresentationException()
        : base(DEFAULT_MESSAGE)
        {
        }

        public CreatePresentationException(string message)
         : base(string.IsNullOrEmpty(message) ? DEFAULT_MESSAGE : message)
        {
        }
    }
}
