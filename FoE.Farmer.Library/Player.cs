using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoE.Farmer.Library.Payloads;

namespace FoE.Farmer.Library
{
    public class Player
    {
        public int ID { get; set; }
        public bool IsSelf { get; set; } = false;
        public string Name { get; set; }
        public bool IsNeighbour { get; set; } = false;
        public bool IsGuildMember { get; set; } = false;
        public FriendStatus IsFriend { get; set; } = FriendStatus.NoFriend;
        public DateTime LastTavernVisit { get; set; }
        public DateTime LastHelp { get; set; }
        public List<Building> Buildings { get; } = new List<Building>();

        public bool CanAidable
        {
            get
            {
                if (LastHelp > DateTime.Now) return false;
                if (IsSelf) return false;

                if (IsNeighbour) return true;
                return IsFriend == FriendStatus.Friend || IsGuildMember;
            }
        }

        public Payload Aid()
        {
            if (CanAidable) return OtherPlayerService.PolivateRandomBuilding(this);
            return null;
        }

        public Payload Visit()
        {
            return OtherPlayerService.Visit(this);
        }
    }

    public enum FriendStatus
    {
        Friend,
        NoFriend,
        FriendRequest,
        BlackList
    }
}
