using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoE.Farmer.Library.Payloads;
using Newtonsoft.Json.Linq;

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
        public DateTime NextHelp { get; set; } = DateTime.Now;
        public List<Building> Buildings { get; } = new List<Building>();
        public Services.TavernService Tavern { get; set; }

        public Player()
        {
            Tavern = new Services.TavernService {Owner = this};
        }

        public bool CanAidable
        {
            get
            {
                if (NextHelp > DateTime.Now) return false;
                if (IsSelf) return false;

                if (IsNeighbour) return true;
                return IsFriend == FriendStatus.Friend || IsGuildMember;
            }
        }

        public void Aid()
        {
            if (CanAidable)
            {
                NextHelp = DateTime.Now + TimeSpan.FromHours(24) + Helper.GetRandomMinutes(20, 60);
                OtherPlayerService.PolivateRandomBuilding(this).Send();
                SetCache("NextHelp", NextHelp);
            }
        }

        public Payload Visit()
        {
            return OtherPlayerService.Visit(this);
        }

        public void SetCache(string attr, DateTime value)
        {
            if (Manager.Cache[ForgeOfEmpires.Manager.Me.ID.ToString()]["Players"][ID.ToString()] == null) Manager.Cache[ForgeOfEmpires.Manager.Me.ID.ToString()]["Players"][ID.ToString()] = new JObject();
            Manager.Cache[ForgeOfEmpires.Manager.Me.ID.ToString()]["Players"][ID.ToString()][attr] = value;
        }

        public void Parse(JToken j)
        {
            ID = j["player_id"].ToObject<int>();

            var nextInter = j["next_interaction_in"]?.ToObject<int>() ?? 0;

            var isFriend = j["is_friend"]?.ToObject<bool>() ?? false;
            var isInvited = j["is_invited"]?.ToObject<bool>() ?? false;


            Name = j["name"].ToObject<string>();
            IsSelf = j["is_self"].ToObject<bool>();
            IsNeighbour = j["is_neighbor"]?.ToObject<bool>() ?? false;
            IsGuildMember = j["is_guild_member"]?.ToObject<bool>() ?? false;
            ID = j["player_id"].ToObject<int>();
            NextHelp = DateTime.Now + TimeSpan.FromSeconds(j["next_interaction_in"]?.ToObject<int>() ?? 0);

            var p_cache = ForgeOfEmpires.Manager.CurrentCache["Players"][ID.ToString()];
            if (p_cache != null)
            {
                if (p_cache["NextHelp"] != null) NextHelp = p_cache["NextHelp"].ToObject<DateTime>();
                if (p_cache["TavernNextCheck"] != null) Tavern.MinTavernCheckTime = p_cache["TavernNextCheck"].ToObject<DateTime>();
                p_cache["Name"] = Name;
            }

            if (!isFriend && !isInvited) IsFriend = FriendStatus.NoFriend;
            else if (isFriend) IsFriend = FriendStatus.Friend;
            else if (isInvited) IsFriend = FriendStatus.FriendRequest;
        }
    }

    public enum FriendStatus
    {
        Friend,
        NoFriend,
        FriendRequest,
        IncomingFriendRequest,
        BlackList
    }
}
