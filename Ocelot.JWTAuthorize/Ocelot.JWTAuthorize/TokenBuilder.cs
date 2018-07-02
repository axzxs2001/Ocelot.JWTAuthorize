
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
    public class TokenBuilder : ITokenBuilder
    {
        /// <summary>
        /// JwtAuthorizationRequirement
        /// </summary>
        readonly JwtAuthorizationRequirement _jwtAuthorizationRequirement;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="jwtAuthorizationRequirement"></param>
        public TokenBuilder(JwtAuthorizationRequirement jwtAuthorizationRequirement)
        {
            _jwtAuthorizationRequirement = jwtAuthorizationRequirement;
        }
        /// <summary>
        /// get the token of jwt
        /// </summary>
        /// <param name="claims">claim array</param>
        /// <returns></returns>
        public Token BuildJwtToken(Claim[] claims)
        {
            var claimList = new List<Claim>(claims);
            claimList.Add(new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(_jwtAuthorizationRequirement.Expiration.TotalSeconds).ToString()));
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: _jwtAuthorizationRequirement.Issuer,
                audience: _jwtAuthorizationRequirement.Audience,
                claims: claimList.ToArray(),
                notBefore: now,
                expires: now.Add(_jwtAuthorizationRequirement.Expiration),
                signingCredentials: _jwtAuthorizationRequirement.SigningCredentials
            );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var responseJson = new Token
            {
                TokenValue = encodedJwt,
                Expires = _jwtAuthorizationRequirement.Expiration.TotalSeconds,
                TokenType = "Bearer"
            };
            return responseJson;
        }
        /// <summary>
        /// back token
        /// </summary>
        public class Token
        {
            /// <summary>
            /// Token Value
            /// </summary>
            public string TokenValue
            { get; set; }
            /// <summary>
            /// Expires (unit second)
            /// </summary>
            public double Expires
            { get; set; }
            /// <summary>
            /// token type
            /// </summary>
            public string TokenType
            { get; set; }
        }
    }
}
