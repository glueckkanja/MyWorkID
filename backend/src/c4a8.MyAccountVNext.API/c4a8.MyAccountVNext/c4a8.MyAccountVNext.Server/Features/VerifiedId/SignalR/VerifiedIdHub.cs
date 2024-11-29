using Microsoft.AspNetCore.SignalR;

namespace c4a8.MyAccountVNext.Server.Features.VerifiedId.SignalR
{
    public class VerifiedIdHub : Hub<IVerifiedIdHub>
    {
        private readonly VerifiedIdSignalRRepository _verifiedIdSignalRRepository;
        public VerifiedIdHub(VerifiedIdSignalRRepository verifiedIdSignalRRepository)
        {
            _verifiedIdSignalRRepository = verifiedIdSignalRRepository;
        }

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
