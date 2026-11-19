using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MyWorkID.Server.Features.VerifiedId.Entities;

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
        /// <param name="action">The action the verification is started for, if any. Carried in the signed
        /// token, so the callback learns it without trusting the caller.</param>
        /// <returns>A JWT token as a string.</returns>
        public static string GenerateToken(
            string userId,
            string jwtSigningKey,
            VerifiedIdAction? action = null
        )
        {
            List<Claim> claims = [new Claim("userId", userId)];
            if (action.HasValue)
            {
                claims.Add(new Claim(Strings.VERIFIED_ID_ACTION_CLAIM, action.Value.ToString()));
            }

            // generate token that is valid for 30 minutes
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.UTF8.GetBytes(jwtSigningKey);
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
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
