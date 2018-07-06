
using System;
using System.Security.Claims;
using static Ocelot.JwtAuthorize.TokenBuilder;

namespace Ocelot.JwtAuthorize
{
    /// <summary>
    /// TokenBuilder interface
    /// </summary>
    public interface ITokenBuilder
    {
        /// <summary>
        /// get the token of jwt
        /// </summary>
        /// <param name="claims">claim array</param>
        /// <returns></returns>
        Token BuildJwtToken(Claim[] claims, DateTime? expires = null);
    }
}
