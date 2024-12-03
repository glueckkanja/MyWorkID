using System.Diagnostics.CodeAnalysis;

namespace c4a8.MyWorkID.Server.Features.VerifiedId.SignalR
{
    /// <summary>
    /// Defines the repository interface for managing SignalR connections for Verified ID operations.
    /// </summary>
    public interface IVerifiedIdSignalRRepository
    {
        /// <summary>
        /// Adds a user connection to the repository.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="connectionId">The connection ID to add.</param>
        void AddUser(string userId, string connectionId);

        /// <summary>
        /// Removes a user connection from the repository.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="connectionId">The connection ID to remove.</param>
        void RemoveUser(string? userId, string connectionId);

        /// <summary>
        /// Tries to get the connections associated with a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="connections">The set of connection IDs associated with the user.</param>
        /// <returns>True if the user has connections; otherwise, false.</returns>
        bool TryGetConnections(string userId, [MaybeNullWhen(false)] out HashSet<string> connections);
    }

    /// <summary>
    /// Repository for managing SignalR connections for Verified ID operations.
    /// </summary>
    public class VerifiedIdSignalRRepository : IVerifiedIdSignalRRepository
    {
        private readonly Dictionary<string, HashSet<string>> _connections = new();

        /// <summary>
        /// Adds a user connection to the repository.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="connectionId">The connection ID to add.</param>
        public void AddUser(string userId, string connectionId)
        {
            if (!_connections.TryGetValue(userId, out var connectionsOfUser))
            {
                connectionsOfUser = new HashSet<string>();
                _connections.Add(userId, connectionsOfUser);
            }

            connectionsOfUser.Add(connectionId);
        }

        /// <summary>
        /// Removes a user connection from the repository.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="connectionId">The connection ID to remove.</param>
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

        /// <summary>
        /// Tries to get the connections associated with a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="connections">The set of connection IDs associated with the user.</param>
        /// <returns>True if the user has connections; otherwise, false.</returns>
        public bool TryGetConnections(string userId, [MaybeNullWhen(false)] out HashSet<string> connections)
        {
            return _connections.TryGetValue(userId, out connections);
        }
    }
}
