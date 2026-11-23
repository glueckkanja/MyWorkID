using Microsoft.Graph.Models;

namespace MyWorkID.Server.Features.VerifiedId
{
    /// <summary>
    /// Builds the Graph request body that sets the custom security attribute holding the
    /// Verified ID verification state of a user.
    /// </summary>
    public static class TargetSecurityAttributeRequestBody
    {
        /// <summary>
        /// Creates a user patch that sets <paramref name="attributeName"/> in
        /// <paramref name="attributeSetName"/> to <paramref name="value"/>.
        /// </summary>
        /// <param name="attributeSetName">Name of the custom security attribute set.</param>
        /// <param name="attributeName">Name of the custom security attribute inside the set.</param>
        /// <param name="value">The value to store.</param>
        /// <returns>The user object with the custom security attribute set.</returns>
        public static User Create(string attributeSetName, string attributeName, string value)
        {
            return new User
            {
                CustomSecurityAttributes = new CustomSecurityAttributeValue
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        {
                            attributeSetName,
                            new CustomSecurityAttributeValue()
                            {
                                OdataType =
                                    "#Microsoft.DirectoryServices.CustomSecurityAttributeValue",
                                AdditionalData = new Dictionary<string, object>
                                {
                                    { attributeName, value },
                                },
                            }
                        },
                    },
                },
            };
        }
    }
}
