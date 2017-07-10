using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
        public List<Player> Players { get; } = new List<Player>();
        public Player Me { get; set; }
        public Requests Requests { get; set; } = new Requests();
        public Player GetPlayerById(int id) => Players.Find(item => item.ID == id);

        public DateTime NextMinGoodsTime { get; set; } = DateTime.MinValue;
        public DateTime NextMinResidentalTime { get; set; } = DateTime.MinValue;
        public DateTime NextMinSuppliesTime { get; set; } = DateTime.MinValue;

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

        public Manager()
        {
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
                NextMinGoodsTime = Helper.GenerateNextInterval((int)_userIntervalGoods, BuildType.Goods);
                Log($"Next time for pickup, Goods: {NextMinGoodsTime.ToLocalTime()}");
            }
            if (NextMinResidentalTime < DateTime.Now)
            {
                PickupByType(BuildType.Residential);
                NextMinResidentalTime = Helper.GenerateNextInterval((int)_userIntervalGoods, BuildType.Residential);
                Log($"Next time for pickup, Residental: {NextMinResidentalTime.ToLocalTime()}");
            }
            if (NextMinSuppliesTime < DateTime.Now)
            {
                PickupByType(BuildType.Supplies);
                NextMinSuppliesTime = Helper.GenerateNextInterval((int)_userIntervalGoods, BuildType.Supplies);
                Log($"Next time for pickup, Supplies: {NextMinSuppliesTime.ToLocalTime()}");
            }

            PickupByType(BuildType.MainBuilding);

        }

        private void PickupByType(BuildType type)
        {
            foreach (var building in Me.Buildings.Where(item => item.CanPickup && item.Type == type))
            {
                var req = building.Pickup();
                if (req == null) continue;
                foreach (var payload in req)
                {
                    Requests.AddPayload(payload);
                }
            }
        }

        public void RunCheckTimer()
        {
            PickupBuildings();
        }

        private void UpdateBuildingInterval()
        {
            if (!IsInitialized) return;

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
            PickupBuildings();
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
                        // Tavern service
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
                        break;
                }
                
            }
        }

        public static void Log(string text)
        {
            LogMessageSend?.Invoke(ForgeOfEmpires.Manager, new LoggingDataEventArgs { Message = text });
        }

        public void ParseStartupData()
        {
            
        }
    }
}
