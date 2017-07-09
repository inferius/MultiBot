using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoE.Farmer.Library.Payloads
{
    public class ResearchService
    {
        private const string ClassName = "ResearchService";

        /// <summary>
        /// Pomuze priteli
        /// </summary>
        /// <param name="buildings"></param>
        /// <returns></returns>
        public static Payload UseResearchPoints(Research research, int pointCount)
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "useStrategyPoints",
                RequestData = new JArray(research.Name, pointCount)
            };
        }
    }
}
