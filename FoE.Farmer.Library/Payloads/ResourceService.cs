using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library.Payloads
{
    public class ResourceService
    {
        private const string ClassName = "ResourceService";

        public static Payload GetData()
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "getPlayerResources"
            };
        }
    }
}
