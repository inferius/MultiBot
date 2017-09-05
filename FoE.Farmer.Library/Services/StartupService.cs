using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library.Services
{
    public class StartupService
    {

        /// <summary>
        /// Ocekava responseData
        /// </summary>
        /// <param name="j"></param>
        public static void Parse(JObject j)
        {
            ParseUserData(j["user_data"]);
            ParseSocialBar(j["socialbar_list"]);
            ParseCityMap(j["city_map"]);
            ParseUnitSlots(j["unit_slots"]);
        }

        private static void ParseUserData(JToken j)
        {
            var me = new Player
            {
                ID = j["player_id"].ToObject<int>(),
                IsSelf = true,
                Name = j["user_name"].ToString()
            };

            Manager.Log($"Loaded players data. ID: {me.ID} and Name: {me.Name}");
            ForgeOfEmpires.Manager.Me = me;
            Manager.InitCache();
        }

        private static void ParseSocialBar(JToken j)
        {
            Manager.Log($"Loaded other players data. Count: {(j as JArray)?.Count ?? 0}");

            foreach (var item in j as JArray)
            {
                var p = new Player();
                p.Parse(item);

                ForgeOfEmpires.Manager.Players.Add(p);
            }
            
        }

        private static void ParseCityMap(JToken j)
        {
            Manager.Log($"Loaded buildings and road. Count: {(j["entities"] as JArray)?.Count ?? 0}");
            foreach (var item in j["entities"] as JArray)
            {
                var b = Building.LoadFromJSON(item as JObject);
                if (b == null) continue;
                b.Owner = ForgeOfEmpires.Manager.Me;
                ForgeOfEmpires.Manager.Me.Buildings.Add(b);
            }
        }

        private static void ParseUnitSlots(JToken j)
        {
            //Manager.Log($"Loaded unit slots. Count: {(j["entities"] as JArray)?.Count ?? 0}");
            var militaryBuilding = ForgeOfEmpires.Manager.Me.Buildings.Where(item => item.Type == BuildType.Military);
            foreach (var s in j as JArray)
            {
                var slot = UnitSlot.Parse(s);
                var build = militaryBuilding.FirstOrDefault(item => item.ID == s["entity_id"].ToObject<int>()) as MilitaryBuilding;
                if (build == null) continue;
                slot.Parent = build;
                build.UnitSLots.Add(slot);
                slot.UnlockSlot();
            }
        }
    }
}
