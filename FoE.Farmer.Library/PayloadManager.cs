using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library
{
    public class PayloadManager
    {
        public List<Payload> Payloads { get; } = new List<Payload>();
        public override string ToString()
        {
            var j = new JArray(Payloads.ToArray());
            return j.ToString(Formatting.None);
        }
    }
}
