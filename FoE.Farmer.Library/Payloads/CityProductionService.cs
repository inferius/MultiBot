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
            var j = new JArray();
            j.Add(new JArray(buildings.Select(item => item.ID)));
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "pickupProduction",
                RequestData = j
            };
        }

        public static Payload StartProduction(Building building)
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "startProduction",
                RequestData = new JArray(building.ID, building.Type == BuildType.Military ? (building as MilitaryBuilding).ReserveSlot() : building.Interval)
            };
        }

        public static Payload UnlockSlot(UnitSlot slot)
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "unlockSlot",
                RequestData = new JArray(slot.Parent.ID, slot.Order, 0)
            };
        }

    }
}
