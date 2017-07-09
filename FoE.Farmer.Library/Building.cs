using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library
{
    public class Building
    {
        public int ID { get; private set; }
        public Player Owner { get; set; }
        public int Interval { get; set; }

        public DateTime LastPickup { get; set; }
        public DateTime MinNextPickup { get; set; } = DateTime.MaxValue;

        public BuildType Type { get; private set; }
        public ProductionState ProductionState { get; private set; }
        public (int, int) Position { get; set; }
        public int ProductionTime { get; set; }

        public bool CanPickup
        {
            get
            {
                if (Type == BuildType.Decoration || Type == BuildType.Military) return false;
                if (MinNextPickup > DateTime.Now) return false;
                switch (ProductionState)
                {
                    case ProductionState.Construction:
                    case ProductionState.IsRunning:
                    case ProductionState.Unconnected:
                        return false;
                }

                return true;
            }
        }

        public static Building LoadFromJSON(JObject j)
        {
            if (j["__class__"].ToString() != "CityMapEntity") throw new ArgumentException("Input object has not building");

            var b = new Building();
            switch (j["type"].ToString())
            {
                case "residential": b.Type = BuildType.Residential; break;
                case "decoration": b.Type = BuildType.Decoration; break;
                case "main_building": b.Type = BuildType.MainBuilding; break;
                case "military": b.Type = BuildType.Military; break;
                case "production": b.Type = BuildType.Supplies; break;
                case "goods": b.Type = BuildType.Goods; break;
                default: return null;
            }

            b.ID = j["id"].ToObject<int>();
            b.MinNextPickup = DateTime.MaxValue;
            b.Position = (j["x"].ToObject<int>(), j["y"].ToObject<int>());

            switch (j["state"]["__class__"].ToString())
            {
                case "ProductionFinishedState":
                    b.ProductionState = ProductionState.Finished;
                    b.MinNextPickup = DateTime.Now;
                    break;
                case "ProducingState":
                    b.ProductionState = ProductionState.IsRunning;
                    b.MinNextPickup = Helper.GenerateNextInterval(j["state"]["next_state_transition_in"].ToObject<int>());
                    break;
                case "IdleState": b.ProductionState = ProductionState.Idle; break;
                case "ConstructionState": b.ProductionState = ProductionState.Construction; break;
                case "UnconnectedState": b.ProductionState = ProductionState.Unconnected; break;
                default:
                    Debug.WriteLine("Unknown building state: " + j["state"]["__class__"]);
                    return null;
            }

            if (b.Type == BuildType.Residential) b.ProductionTime = j["state"]["current_product"]["production_time"].ToObject<int>();

            return b;
        }

        public override string ToString()
        {
            return $"{ID} - {Type}: {ProductionState}, {ProductionTime}";
        }
    }

    public enum ProductionState
    {
        Idle,
        Finished,
        IsRunning,
        Construction,
        Unconnected
    }

    public enum BuildType
    {
        Residential,
        Supplies,
        Goods,
        Decoration,
        MainBuilding,
        Military
    }

    public enum TimeIntervalGoods
    {
        FourHours = 1,
        EightHours = 2,
        OneDay = 3,
        TwoDay = 4
    }

    public enum TimeIntervalSupplies
    {
        FiveMinutes = 1,
        FiftenMinuts = 2,
        OneHour = 3,
        FourHour = 4,
        EightHour = 5,
        OneDay = 6
    }
}
