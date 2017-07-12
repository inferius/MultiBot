using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library.Services
{
    public class TreasureHuntService
    {
        public static DateTime NextCheckTime { get; set; } = DateTime.MinValue;


        public static async void CheckTreasureHunt()
        {
            if (NextCheckTime > DateTime.Now) return;
            Manager.Log("Checking treasure hunt...");

            var plainData = await Payloads.TreasureHuntService.GetOverview().Send();
            var timeService = Helper.GetObjectByClass(plainData, "TimeService", "updateTime");
            var curServerTime = timeService["responseData"]["time"].ToObject<long>();

            var data = Helper.GetObjectByClass(plainData, "TreasureHuntService", "getOverview");
            var chests = data["responseData"]["treasure_chests"] as JArray;
            if (chests == null) return;

            for (var i = 0; i < chests.Count; i++)
            {
                var chest = chests[i];
                if (chest["state"]["__class__"].ToString() == "TreasureChestCollectable")
                {
                    // collect
                    if (i + 1 >= chests.Count)
                    {
                        NextCheckTime = DateTime.Now + Helper.GetRandomMinutes();
                        Manager.Log("- Treasure hunt - collected rewards, Complete treasure hunt, next start in: " + NextCheckTime.ToLocalTime());
                    }
                    else
                    {
                        var nextTravelTime = chest["travel_time"].ToObject<int>();
                        NextCheckTime = DateTime.Now + TimeSpan.FromSeconds(nextTravelTime) + Helper.GetRandomMinutes(2, 8);
                        Manager.Log($"- Treasure hunt - collected rewards, Traveling to next (chest no. {i+2}): {NextCheckTime.ToLocalTime()}");
                    }
                    await Payloads.TreasureHuntService.CollectTreasure().Send();
                    return;
                }
                if (chest["state"]["__class__"].ToString() == "TreasureChestTraveling")
                {
                    var arrival_time = chest["state"]["arrival_time"].ToObject<long>();
                    var aTime = Math.Abs(arrival_time - curServerTime);
                    NextCheckTime = DateTime.Now + TimeSpan.FromSeconds(aTime) + Helper.GetRandomMinutes(2, 8);
                    Manager.Log($"- Treasure hunt - traveling to chest {i+1}, next check time " + NextCheckTime.ToLocalTime());
                    return;
                }
                if (chest["state"]["__class__"].ToString() == "TreasureChestClosed")
                {
                    continue;
                }
            }
       

        }
    }
}
