using System;

namespace Ocelot.JwtAuthorize
{
    /// <summary>
    /// Ocelot.JwtAuthorize customer exception
    /// </summary>
    public class OcelotJwtAuthorizeException : ApplicationException
    {
        public OcelotJwtAuthorizeException(string message) :
            base(message)
        { }

        public OcelotJwtAuthorizeException() : base() { }
    }
}
