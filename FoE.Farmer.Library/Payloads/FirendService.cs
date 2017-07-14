using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoE.Farmer.Library.Payloads
{
    public class FirendService : Payload
    {
        private const string ClassName = "FriendService";

        /// <summary>
        /// Vyhleda doporucene pratele
        /// </summary>
        /// <returns></returns>
        public static Payload GetFriendSuggestions()
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "getFriendSuggestions"
            };
        }

        /// <summary>
        /// Pozve hrace na základe jména
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Payload InvitePlayerByName(string name)
        {
            return new Payload
            {
                RequestClass = ClassName,
                RequestMethod = "invitePlayerByName"
            };
        }
    }
}
