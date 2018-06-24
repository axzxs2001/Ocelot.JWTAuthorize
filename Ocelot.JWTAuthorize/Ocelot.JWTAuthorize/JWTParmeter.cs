using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.JWTAuthorize
{
    public class JWTParmeter
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Secret { get; set; }
        public string DefaultScheme { get; set; }
        public string PolicyName { get; set; }
        public string DeniedUrl { get; set; }
        public bool IsHttps { get; set; }
    }
}
