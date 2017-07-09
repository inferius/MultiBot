using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoE.Farmer.Library.Payloads
{
    public class CityProductionService
    {
        private const string ClassName = "CityProductionService";

        public static Payload PickupProduction(Building[] buildings)
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "pickupProduction",
                RequestData = new JArray(new JArray(buildings.Select(item => item.ID)))
            };
        }

        public static Payload StartProduction(Building building)
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "startProduction",
                RequestData = new JArray(building.ID, building.Interval)
            };
        }

    }
}
