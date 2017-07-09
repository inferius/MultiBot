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
        }

        private static void ParseUserData(JToken j)
        {
            var me = new Player
            {
                ID = j["player_id"].ToObject<int>(),
                IsSelf = true,
                Name = j["user_name"].ToString()
            };

            ForgeOfEmpires.Manager.Me = me;
        }

        private static void ParseSocialBar(JToken j)
        {
            foreach (var item in j as JArray)
            {
                var isFriend = item["is_friend"]?.ToObject<bool>() ?? false;
                var isInvited = item["is_invited"]?.ToObject<bool>() ?? false;
                

                var p = new Player
                {
                    IsSelf = item["is_self"].ToObject<bool>(),
                    IsNeighbour = item["is_neighbor"]?.ToObject<bool>() ?? false,
                    IsGuildMember = item["is_guild_member"]?.ToObject<bool>() ?? false,
                    ID = item["player_id"].ToObject<int>()
                };

                if (!isFriend && !isInvited) p.IsFriend = FriendStatus.NoFriend;
                else if (isFriend) p.IsFriend = FriendStatus.Friend;
                else if (isInvited) p.IsFriend = FriendStatus.FriendRequest;
                ForgeOfEmpires.Manager.Players.Add(p);
            }
            
        }

        private static void ParseCityMap(JToken j)
        {
            foreach (var item in j["entities"] as JArray)
            {
                var b = Building.LoadFromJSON(item as JObject);
                if (b == null) continue;
                b.Owner = ForgeOfEmpires.Manager.Me;
                ForgeOfEmpires.Manager.Me.Buildings.Add(b);
            }
        }
    }
}
