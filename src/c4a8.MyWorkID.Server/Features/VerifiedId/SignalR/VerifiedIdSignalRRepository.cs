﻿using System.Diagnostics.CodeAnalysis;

namespace c4a8.MyWorkID.Server.Features.VerifiedId.SignalR
{
    public interface IVerifiedIdSignalRRepository
    {
        public void AddUser(string userId, string connectionId);
        public void RemoveUser(string? userId, string connectionId);
        public bool TryGetConnections(string userId, [MaybeNullWhen(false)] out HashSet<string> connections);
    }

    public class VerifiedIdSignalRRepository : IVerifiedIdSignalRRepository
    {
        private readonly Dictionary<string, HashSet<string>> _connections = [];

        public void AddUser(string userId, string connectionId)
        {
            if (!_connections.TryGetValue(userId, out var connectionsOfUser))
            {
                connectionsOfUser = [];
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
