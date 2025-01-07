using System.Runtime.Serialization;

namespace MyWorkID.Server.Exceptions
{
    public class ConfigurationMissingException : Exception
    {
        public string ConfigurationKey { get; private set; }
        public ConfigurationMissingException(string configurationKey)
        {
            ConfigurationKey = configurationKey;
        }

        public ConfigurationMissingException(string configurationKey, string? message) : base(message)
        {
            ConfigurationKey = configurationKey;
        }

        public ConfigurationMissingException(string configurationKey, string? message, Exception? innerException) : base(message, innerException)
        {
            ConfigurationKey = configurationKey;
        }
    }
}

