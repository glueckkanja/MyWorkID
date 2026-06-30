using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MyWorkID.Server.Features.VerifiedId
{
    /// <summary>
    /// Provides methods for generating JWT tokens.
    /// </summary>
    public static class JwtTokenProvider
    {
        /// <summary>
        /// Generates a JWT token for the specified user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="jwtSigningKey">The key used to sign the JWT token.</param>
        /// <returns>A JWT token as a string.</returns>
        public static string GenerateToken(string userId, string jwtSigningKey)
        {
            // generate token that is valid for 30 minutes
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.UTF8.GetBytes(jwtSigningKey);
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("userId", userId) }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
