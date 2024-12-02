namespace c4a8.MyWorkID.Server.Features.VerifiedId.Exceptions
{
    public class CreatePresentationException : Exception
    {
        private const string DefaultMessage = "Parsed create presentation response resulted in null object.";

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
