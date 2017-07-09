using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoE.Farmer.Library.Payloads
{
    public class StartupService
    {
        private const string ClassName = "StartupService";

        public static Payload GetData()
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "getData"
            };
        }
    }
}
