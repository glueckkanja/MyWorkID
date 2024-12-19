namespace c4a8.MyWorkID.Server.Features.VerifiedId.Exceptions
{
    /// <summary>
    /// Represents an exception that occurs during the creation of a presentation request.
    /// </summary>
    public class CreatePresentationException : Exception
    {
        private const string DEFAULT_MESSAGE = Strings.ERROR_INVALID_BODY;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePresentationException"/> class with a default error message.
        /// </summary>
        public CreatePresentationException()
        : base(DEFAULT_MESSAGE)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePresentationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public CreatePresentationException(string message)
         : base(string.IsNullOrEmpty(message) ? DEFAULT_MESSAGE : message)
        {
        }
    }
}
