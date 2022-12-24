using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library.Payloads
{
    public static class TavernService
    {
        private const string ClassName = "FriendsTavernService";

        /// <summary>
        /// Informace o taverne pritele
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Payload GetOtherTavernState(Player player)
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "getOtherTavernState",
                RequestData = new JArray(player.ID)
            };
        }

        /// <summary>
        /// Informace o okolních tavernach
        /// </summary>
        /// <returns></returns>
        public static Payload GetOtherTavernStates()
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "getOtherTavernStates",
            };
        }

        public static Payload GetConfig()
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "getConfig"
            };
        }

        /// <summary>
        /// Informace o vlastní Taverne
        /// </summary>
        /// <returns></returns>
        public static Payload GetOwnTavern()
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "getOwnTavern"
            };
        }

        /// <summary>
        /// Informace o hracske Taverne
        /// </summary>
        /// <returns></returns>
        public static Payload GetOtherTavern(Player player)
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "getOtherTavern",
                RequestData = new JArray(player.ID)
            };
        }

        

        /// <summary>
        /// Vybere stribrnaky z Taverny
        /// </summary>
        /// <returns></returns>
        public static Payload CollectReward()
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "collectReward"
            };
        }

        /// <summary>
        /// Vrati pocet obsazenych mist v Taverne
        /// </summary>
        /// <returns></returns>
        public static Payload GetSittingPlayersCount()
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "getSittingPlayersCount"
            };
        }
    }
}
