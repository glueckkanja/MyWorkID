using System.Diagnostics.CodeAnalysis;

namespace c4a8.MyAccountVNext.Server.Features.VerifiedId.SignalR
{
    public class VerifiedIdSignalRRepository
    {
        private readonly Dictionary<string, HashSet<string>> _connections = new();

        public void AddUser(string userId, string connectionId)
        {
            if (!_connections.TryGetValue(userId, out var connectionsOfUser))
            {
                connectionsOfUser = new HashSet<string>();
                _connections.Add(userId, connectionsOfUser);
            }

            connectionsOfUser.Add(connectionId);
        }

        public void RemoveUser(string? userId, string connectionId)
        {
            if (userId != null && _connections.TryGetValue(userId, out var connectionsOfUser))
            {
                connectionsOfUser.Remove(connectionId);
            }
            else // This should never happen but if the userId is lost (e.g. disconnect without userToken) we will remove the connectionId from all users
            {
                foreach (var connections in _connections.Values)
                {
                    connections.Remove(connectionId);
                }
            }
        }

        public bool TryGetConnections(string userId, [MaybeNullWhen(false)] out HashSet<string> connections)
        {
            return _connections.TryGetValue(userId, out connections);
        }
    }
}
