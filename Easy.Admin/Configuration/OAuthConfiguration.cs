using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Easy.Admin.Configuration
{
    public class OAuthConfiguration
    {
        public string Scheme { get; set; }
        public string Authority { get; set; }
        public string CallbackPath { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ResponseType { get; set; }
        public string[] Scopes { get; set; }
    }
}
