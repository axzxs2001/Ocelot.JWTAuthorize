
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ocelot.JwtAuthorize
{
    /// <summary>
    /// TokenBuilder
    /// </summary>
    public class TokenBuilder
    {
        /// <summary>
        /// get the token of jwt
        /// </summary>
        /// <param name="claims">claim array</param>
        /// <param name="jwtAuthorizationRequirement">JwtAuthorizationRequirement</param>
        /// <returns></returns>
        public static dynamic BuildJwtToken(Claim[] claims, JwtAuthorizationRequirement  jwtAuthorizationRequirement)
        {
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: jwtAuthorizationRequirement.Issuer,
                audience: jwtAuthorizationRequirement.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(jwtAuthorizationRequirement.Expiration),
                signingCredentials: jwtAuthorizationRequirement.SigningCredentials
            );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var responseJson = new 
            {
                Status = true,
                access_token = encodedJwt,
                expires_in = jwtAuthorizationRequirement.Expiration.TotalSeconds,
                token_type = "Bearer"
            };
            return responseJson;
        }
    }
}
