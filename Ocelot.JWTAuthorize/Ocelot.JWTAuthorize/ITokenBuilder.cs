
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        dynamic BuildJwtToken(Claim[] claims);
    }
}
