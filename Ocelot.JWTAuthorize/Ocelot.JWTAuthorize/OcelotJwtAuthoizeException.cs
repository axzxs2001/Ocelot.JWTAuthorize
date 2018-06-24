using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.JWTAuthorize
{
    public class OcelotJwtAuthoizeException : ApplicationException
    {
        public OcelotJwtAuthoizeException(string message) :
            base(message)
        { }

        public OcelotJwtAuthoizeException() : base() { }
    }
}
