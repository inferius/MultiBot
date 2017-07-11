using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library.Payloads
{
    public class TreasureHuntService : Payload
    {
        private const string ClassName = "TreasureHuntService";

        /// <summary>
        /// Nacte informace o honbe za pokladem
        /// </summary>
        /// <returns></returns>
        public static Payload GetOverview()
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "getOverview"
            };
        }

        /// <summary>
        /// Posbira odmeny
        /// </summary>
        /// <returns></returns>
        public static Payload CollectTreasure()
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "collectTreasure"
            };
        }
        
    }
}
