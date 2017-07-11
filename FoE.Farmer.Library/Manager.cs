using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Flurl.Util;
using FoE.Farmer.Library.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library
{
    public class Manager
    {
        public static event LoggingDataHandler LogMessageSend;
        public delegate void LoggingDataHandler(Manager m, Events.LoggingDataEventArgs e);

        public event LogoutEventHandler LogoutEvent;
        public delegate void LogoutEventHandler(Manager m, Events.LogoutEvent e);

        public event ResourceUpdateEventArgsHandler ResourcesUpdate;
        public delegate void ResourceUpdateEventArgsHandler(Manager m, Events.ResourceUpdateEventArgs e);

        private JObject Config = new JObject();
        public static JObject Cache = new JObject();

        private static string CachePath { get; set; } = "FoFCache.json";

        public List<Player> Players { get; } = new List<Player>();
        public Player Me { get; set; }
        public Requests Requests { get; set; } = new Requests();
        public Player GetPlayerById(int id) => Players.Find(item => item.ID == id);

        private DateTime _nextMinGoodsTime = DateTime.MinValue;
        public DateTime NextMinGoodsTime
        {
            get => _nextMinGoodsTime;
            set
            {
                CurrentCache["NextMinGoodsTime"] = value;
                _nextMinGoodsTime = value;
            }
        }

        private DateTime _nextMinResidentalTime = DateTime.MinValue;
        public DateTime NextMinResidentalTime
        {
            get => _nextMinResidentalTime;
            set
            {
                CurrentCache["NextMinResidentalTime"] = value;
                _nextMinResidentalTime = value;
            }
        }
        private DateTime _nextMinSuppliesTime = DateTime.MinValue;
        public DateTime NextMinSuppliesTime
        {
            get => _nextMinSuppliesTime;
            set
            {
                CurrentCache["NextMinSuppliesTime"] = value;
                _nextMinSuppliesTime = value;
            }
        }

        public bool IsInitialized { get; set; } = false;
        public bool IsStartupServicesLoad { get; set; } = false;
        public bool IsStarted { get; set; } = false;

        private TimeIntervalGoods _userIntervalGoods = TimeIntervalGoods.EightHours;
        private TimeIntervalSupplies _userIntervalSupplies = TimeIntervalSupplies.FiftenMinutes;
        private TimeIntervalSupplies _userIntervalResidental = TimeIntervalSupplies.FiftenMinutes;

        private Timer _timer = new Timer(60000);

        public TimeIntervalGoods UserIntervalGoods
        {
            get => _userIntervalGoods;
            set { _userIntervalGoods = value; UpdateBuildingInterval(); }
        }
        public TimeIntervalSupplies UserIntervalSupplies
        {
            get => _userIntervalSupplies;
            set { _userIntervalSupplies = value; UpdateBuildingInterval(); }
        }
        public TimeIntervalSupplies UserIntervalResidental
        {
            get => _userIntervalResidental;
            set { _userIntervalResidental = value; UpdateBuildingInterval(); }
        }

        public JObject CurrentCache => Cache[Me.ID.ToString()] as JObject;

        static Manager()
        {
            if (File.Exists(CachePath)) Cache = JObject.Parse(File.ReadAllText(CachePath));
        }

        internal static void InitCache()
        {
            if (Cache[ForgeOfEmpires.Manager.Me.ID.ToString()] == null) Cache[ForgeOfEmpires.Manager.Me.ID.ToString()] = new JObject();
            if (Cache[ForgeOfEmpires.Manager.Me.ID.ToString()]["Players"] == null) Cache[ForgeOfEmpires.Manager.Me.ID.ToString()]["Players"] = new JObject();

        }
        public static void SaveCache()
        {
            File.WriteAllText(CachePath, Cache.ToString(Formatting.None));
        }

        public Manager()
        {
            if (File.Exists("FoFFriendData.json")) Config = JObject.Parse(File.ReadAllText("FoFFriendData.json"));

            _timer.Elapsed += (sender, args) =>
            {
                RunCheckTimer();
            };
        }

        public void PickupBuildings()
        {

            if (NextMinGoodsTime < DateTime.Now)
            {
                PickupByType(BuildType.Goods);
                NextMinGoodsTime = Helper.GenerateNextInterval((int)UserIntervalGoods, BuildType.Goods);
                Log($"Next time for pickup, Goods: {NextMinGoodsTime.ToLocalTime()}");
            }
            if (NextMinResidentalTime < DateTime.Now)
            {
                PickupByType(BuildType.Residential);
                NextMinResidentalTime = Helper.GenerateNextInterval((int)UserIntervalResidental, BuildType.Residential);
                Log($"Next time check residental pickup {NextMinResidentalTime.ToLocalTime()}");
            }
            if (NextMinSuppliesTime < DateTime.Now)
            {
                PickupByType(BuildType.Supplies);
                NextMinSuppliesTime = Helper.GenerateNextInterval((int)UserIntervalSupplies, BuildType.Supplies);
                Log($"Next time for pickup, Supplies: {NextMinSuppliesTime.ToLocalTime()}");
            }

            PickupByType(BuildType.MainBuilding);

        }

        public void TavernAndAidService()
        {
            Me.Tavern.CheckTavernOccupation();
            var countAid = 0;
            var countTavern = 0;
            foreach (var player in Players)
            {
                if (player.CanAidable) countAid++;
                if (player.Tavern.CanSit) countTavern++;
                player.Tavern.Sit();
                player.Aid();
            }
            if (countTavern > 0 || countAid > 0) Log($"Tavern sitting: {countTavern} and players help: {countAid}");
        }

        private void PickupByType(BuildType type)
        {
            var idleStart = new List<int>();
            var pickupStart = new List<int>();
            var onlypickup = new List<int>();

            foreach (var building in Me.Buildings.Where(item => item.CanPickup && item.Type == type))
            {
                if (building.CanPickup)
                    if (building.Type == BuildType.Residential || building.Type == BuildType.MainBuilding) onlypickup.Add(building.ID);
                    else if (building.Type == BuildType.Goods || building.Type == BuildType.Supplies)
                        if (building.ProductionState == ProductionState.Idle) idleStart.Add(building.ID);
                        else pickupStart.Add(building.ID);

                building.Pickup();
            }
            if (onlypickup.Count > 0) Log($"Pickup buildings ID: {string.Join(", ", onlypickup)}");
            if (idleStart.Count > 0) Log($"Idle bulding, only start production ID: {string.Join(", ", idleStart)}");
            if (pickupStart.Count > 0) Log($"Pickup building and start production ID: {string.Join(", ", pickupStart)}");
        }

        public void RunCheckTimer()
        {
            PickupBuildings();
            TavernAndAidService();

            Services.TreasureHuntService.CheckTreasureHunt();
        }

        private void UpdateBuildingInterval()
        {
            if (!IsInitialized) return;
            if (Me == null) return;

            foreach (var building in Me.Buildings)
            {
                if (building.Type == BuildType.Goods) building.Interval = (int)_userIntervalGoods;
                if (building.Type == BuildType.Supplies) building.Interval = (int)_userIntervalSupplies;
                if (building.Type == BuildType.Residential) building.Interval = (int)_userIntervalResidental;
            }
        }

        private void StartupService()
        {
            IsStartupServicesLoad = true;
            // Load cache
            if (CurrentCache["NextMinGoodsTime"] != null)
            {
                _nextMinGoodsTime = CurrentCache["NextMinGoodsTime"].ToObject<DateTime>();
                Log($"Next goods pickup loaded from cache: {NextMinGoodsTime.ToLocalTime()}");
            }
            if (CurrentCache["NextMinSuppliesTime"] != null)
            {
                _nextMinSuppliesTime = CurrentCache["NextMinSuppliesTime"].ToObject<DateTime>();
                Log($"Next supplies pickup loaded from cache: {NextMinSuppliesTime.ToLocalTime()}");
            }
            if (CurrentCache["NextMinResidentalTime"] != null)
            {
                _nextMinResidentalTime = CurrentCache["NextMinResidentalTime"].ToObject<DateTime>();
                Log($"Next residental pickup loaded from cache: {NextMinResidentalTime.ToLocalTime()}");
            }

            RunCheckTimer();
            _timer.Start();
            IsStarted = true;
        }

        //public void Start()
        //{
        //    if (!IsStartupServicesLoad)
        //}

        public void Stop()
        {
            _timer.Stop();
            IsStarted = false;
        }

        public void Start()
        {
            _timer.Start();
            IsStarted = true;
        }

        public void ParseStringData(string data)
        {
            var ja = JArray.Parse(data);
            var taverUnlocked = 0;
            var taverOccup = 0;

            foreach (var item in ja)
            {
                var j = item as JObject;

                if (j["__class__"].ToString() == "Redirect")
                {
                    _timer.Stop();
                    Log("Session Timeout! Relogin requested.");
                    LogoutEvent?.Invoke(this, new LogoutEvent());
                    return;
                }
                if (j["__class__"].ToString() == "Error")
                {
                    Log("Error:" + j.ToString());
                    return;
                }

                switch (j["requestClass"].ToString())
                {
                    case "FriendsTavernService":
                        if (j["requestMethod"].ToString() == "getSittingPlayersCount")
                        {
                            var ja2 = j["responseData"] as JArray;
                            taverUnlocked = (ja2[1]).ToObject<int>();
                            taverOccup = (ja2[2]).ToObject<int>();
                        }
                        else if (j["requestMethod"].ToString() == "getOwnTavern")
                        {
                            Me.Tavern.Parse(j);
                        }
                        break;
                    case "ResourceService":
                        if (j["requestMethod"].ToString() == "getPlayerResources")
                        {
                            var res = new List<(string, int)>();
                            var resArray = j["responseData"]["resources"] as JObject;
                            if (resArray != null)
                            {
                                foreach (var oneRes in resArray)
                                {
                                    if (oneRes.Key.StartsWith("raw_")) continue;

                                    switch (oneRes.Key)
                                    {
                                        case "carnival_roses":
                                        case "negotiation_game_turn":
                                        case "population":
                                        case "expansions":
                                        case "guild_expedition_attempt":
                                        case "summer_tickets":
                                        case "spring_lanterns":
                                        case "stars":
                                            break;
                                        default:
                                            res.Add((oneRes.Key, oneRes.Value.ToObject<int>()));
                                            break;
                                    }
                                }

                                ResourcesUpdate?.Invoke(this, new ResourceUpdateEventArgs {Values = res.ToArray()});
                            }
                        }
                        break;
                    case "ResearchService":
                        // Research saervice
                        break;
                    case "OtherPlayerService":
                        // player and friend service
                        break;
                    case "StartupService":
                        Services.StartupService.Parse(j["responseData"] as JObject);
                        StartupService();
                        Me.Tavern.UnlockedChairs = taverUnlocked;
                        Me.Tavern.OccupiedChairs = taverOccup;

                        break;
                }

            }
        }

        public static void Log(string text, LogMessageType type = LogMessageType.Info)
        {
            LogMessageSend?.Invoke(ForgeOfEmpires.Manager, new LoggingDataEventArgs { Message = text, Type = type });
        }

        public void ParseStartupData()
        {

        }
    }

    [Flags]
    public enum LogMessageType
    {
        Warning = 0b0000_0001,
        Info = 0b0000_0010,
        Error = 0b0000_0100,
        Debug = 0b0000_1000,
        Request = 0b0001_0000,
        Verbose = 0b1111_1111,
        AllWithoutRequest = 0b1110_1111
    }
}
