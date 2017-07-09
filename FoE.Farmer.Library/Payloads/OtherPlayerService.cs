using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library.Payloads
{
    public class OtherPlayerService
    {
        private const string ClassName = "OtherPlayerService";

        /// <summary>
        /// Pomuze priteli
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Payload PolivateRandomBuilding(Player player)
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "polivateRandomBuilding",
                RequestData = new JArray(player.ID)
            };
        }

        /// <summary>
        /// Navstivite pritele
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Payload Visit(Player player)
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "visitPlayer",
                RequestData = new JArray(player.ID)
            };
        }
    }
}
