using Microsoft.AspNetCore.SignalR;

namespace c4a8.MyWorkID.Server.Features.VerifiedId.SignalR
{
    /// <summary>
    /// SignalR hub for managing Verified ID operations and user connections.
    /// </summary>
    public class VerifiedIdHub : Hub<IVerifiedIdHub>
    {
        private readonly VerifiedIdSignalRRepository _verifiedIdSignalRRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifiedIdHub"/> class with the specified repository.
        /// </summary>
        /// <param name="verifiedIdSignalRRepository">The repository used to manage user connections.</param>
        public VerifiedIdHub(VerifiedIdSignalRRepository verifiedIdSignalRRepository)
        {
            _verifiedIdSignalRRepository = verifiedIdSignalRRepository;
        }

        /// <summary>
        /// Called when a new connection is established with the hub.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null || !httpContext.Request.Query.TryGetValue("access_token", out var userObjectId))
            {
                throw new InvalidOperationException("User object id is missing");
            }

            _verifiedIdSignalRRepository.AddUser(userObjectId!, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a connection with the hub is terminated.
        /// </summary>
        /// <param name="exception">The exception that occurred, if any.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            string? userObjectId = null;
            if (httpContext != null && httpContext.Request.Query.TryGetValue("access_token", out var userObjectIdRaw))
            {
                userObjectId = userObjectIdRaw.ToString();
            }
            _verifiedIdSignalRRepository.RemoveUser(userObjectId, Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
