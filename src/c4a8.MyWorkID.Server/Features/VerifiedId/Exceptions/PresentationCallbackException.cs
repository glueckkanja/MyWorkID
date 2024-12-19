namespace c4a8.MyWorkID.Server.Features.VerifiedId.Exceptions
{
    /// <summary>
    /// Represents an exception that occurs during the check of the callback of a presentation request.
    /// </summary>
    public class PresentationCallbackException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PresentationCallbackException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public PresentationCallbackException(string? message) : base(message)
        {
        }
    }
}
