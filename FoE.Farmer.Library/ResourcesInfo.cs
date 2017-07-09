using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library
{
    public class ResourcesInfo
    {
        private Dictionary<ResourceType, long> resources = new Dictionary<ResourceType, long>();
        public ResourcesInfo(JArray data)
        {
            
        }

        public void ParseData(JArray data)
        {
            foreach (JObject item in data)
            {
                if (item["requestClass"].ToString() == "ResourceService" &&
                    item["requestMethod"].ToString() == "getPlayerResources")
                {
                    ParseResourcesObject(item["responseData"] as JObject);
                }
            }
        }

        private void ParseResourcesObject(JObject res)
        {
            
        }
    }

    public enum ResourceType
    {
        Money,
        Population,
        Medals,
        Supplies,
        [Description("tavern_silver")]
        TavernSilver,
        Premium,
        [Description("strategy_points")]
        ReforgePoints

    }
}
