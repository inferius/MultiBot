using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library.Services
{
    public class TavernService
    {
        public Player Owner { get; set; }
        public int UnlockedChairs { get; set; } = 0;
        public int OccupiedChairs { get; set; } = 0;
        public int BaseSilver { get; set; } = 56;
        public int SilverAdd { get; set; } = 2;
        public static decimal MinTaverOccupation { get; set; } = 0.7M;
        public DateTime MinTavernCheckTime { get; set; } = DateTime.MinValue;

        public bool CanCollect
        {
            get
            {
                if (!Owner.IsSelf) return false;
                var minOccupied = (int)Math.Round(UnlockedChairs * MinTaverOccupation);
                if (OccupiedChairs >= minOccupied) return true;
                return false;
            }
        }

        public bool CanSit
        {
            get
            {
                if (Owner.IsSelf || Owner.IsFriend != FriendStatus.Friend) return false;
                if (MinTavernCheckTime > DateTime.Now) return false;

                var minOccupied = (int)Math.Round(UnlockedChairs * MinTaverOccupation);
                if (OccupiedChairs >= minOccupied) return true;
                return false;
            }
        }

        public void Parse(JObject j)
        {
            if (j == null) return;

            if (j["requestMethod"].ToString() == "getOwnTavern") ParseOwnTavern(j["responseData"]);
        }

        public void ParseOwnTavern(JToken j)
        {
            MinTavernCheckTime = DateTime.Now + TimeSpan.FromHours(1);

            UnlockedChairs = j["view"]["unlockedChairs"].ToObject<int>();
            OccupiedChairs = ((JArray)j["view"]["visitors"])?.Count ?? 0;
            BaseSilver = j["view"]["tavernSilverBase"].ToObject<int>();
            SilverAdd = j["view"]["tavernSilverAdd"].ToObject<int>();

            Collect();
        }

        public void CheckTavernOccupation()
        {
            if (!Owner.IsSelf) return;
            if (MinTavernCheckTime < DateTime.Now) Payloads.TavernService.GetOwnTavern().Send();
        }

        public async void Collect()
        {
            if (!CanCollect) return;

            await Payloads.TavernService.GetConfig().Send();
            await Payloads.TavernService.CollectReward().Send();

            Manager.Log("Collecting own tavern");
        }

        public async void Sit()
        {
            if (Owner.IsSelf || Owner.IsFriend != FriendStatus.Friend) return;
            if (MinTavernCheckTime > DateTime.Now) return;

            //MinTavernCheckTime = DateTime.Now + TimeSpan.FromHours(3);
            //Manager.Log("Checking friend tavern. " + Owner.ID);
            var data = await Payloads.TavernService.GetOtherTavern(Owner).Send();
            var j = Helper.GetObjectByClass(data, "FriendsTavernService", "getOtherTavern");
            var rd = j["responseData"];
            if (rd["state"]?.ToString() == "alreadyVisited")
            {
                MinTavernCheckTime = DateTime.Now + TimeSpan.FromSeconds(rd["nextVisitTime"].ToObject<int>());
            }
            else if (rd["state"]?.ToString() == "isSitting")
            {
                MinTavernCheckTime = DateTime.Now + TimeSpan.FromHours(24);
            }
            else if (rd["state"]?.ToString() == "notFriend")
            {
                MinTavernCheckTime = DateTime.Now + TimeSpan.FromDays(3);
            }
            else //if (rd["state"]?.ToString() == "noChair")
            {
                MinTavernCheckTime = DateTime.Now + TimeSpan.FromHours(3);
            }
            Owner.SetCache("TavernNextCheck", MinTavernCheckTime);
        }
    }
}
