using System;

namespace Ocelot.JwtAuthorize
{
    /// <summary>
    /// Ocelot.JwtAuthorize customer exception
    /// </summary>
    public class OcelotJwtAuthoizeException : ApplicationException
    {
        public OcelotJwtAuthoizeException(string message) :
            base(message)
        { }

        public OcelotJwtAuthoizeException() : base() { }
    }
}
