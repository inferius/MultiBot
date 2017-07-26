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

                if (ProductionState == ProductionState.IsRunning && MinNextPickup < DateTime.Now)
                {
                    ProductionState = ProductionState.Finished;
                }

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

        public async Task<bool> Pickup()
        {
            if (!CanPickup) return false;

            if (Type == BuildType.Residential || Type == BuildType.MainBuilding)
            {
                ProductionState = ProductionState.IsRunning;

                MinNextPickup = Helper.GenerateNextInterval(ProductionTime);
                await Payloads.CityProductionService.PickupProduction(new[] {this}).Send();
            }
            else if (Type == BuildType.Goods || Type == BuildType.Supplies)
            {
                var oldProdState = ProductionState;
                ProductionState = ProductionState.IsRunning;

                var interval = Type == BuildType.Goods ? (int)ForgeOfEmpires.Manager.UserIntervalGoods : (int)ForgeOfEmpires.Manager.UserIntervalSupplies;
                MinNextPickup = Helper.GenerateNextInterval(interval, Type);
                if (oldProdState == ProductionState.Idle)
                {
                    await Payloads.CityProductionService.StartProduction(this).Send();
                }
                else
                {
                    await Payloads.CityProductionService.PickupProduction(new[] {this}).Send();
                    await Payloads.CityProductionService.StartProduction(this).Send();
                }
            }

            return true;
        }

        public static Building LoadFromJSON(JObject j)
        {
            if (j["__class__"].ToString() != "CityMapEntity") throw new ArgumentException("Input object has not building");

            var b = new Building();
            switch (j["type"].ToString())
            {
                case "residential": b.Type = BuildType.Residential; b.Interval = (int)ForgeOfEmpires.Manager.UserIntervalResidental; break;
                case "decoration": b.Type = BuildType.Decoration; break;
                case "main_building": b.Type = BuildType.MainBuilding; b.Interval = (int)TimeIntervalSupplies.OneDay; break;
                case "military": b.Type = BuildType.Military; break;
                case "production": b.Type = BuildType.Supplies; b.Interval = (int)ForgeOfEmpires.Manager.UserIntervalSupplies; break;
                case "goods": b.Type = BuildType.Goods; b.Interval = (int)ForgeOfEmpires.Manager.UserIntervalGoods; break;
                default: return null;
            }

            b.ID = j["id"].ToObject<int>();
            b.MinNextPickup = DateTime.MaxValue;
            b.Position = (j["x"].ToObject<int>(), j["y"].ToObject<int>());

            switch (j["state"]["__class__"].ToString())
            {
                case "ProductionFinishedState":
                    b.ProductionState = ProductionState.Finished;
                    b.MinNextPickup = DateTime.MinValue;
                    break;
                case "ProducingState":
                    b.ProductionState = ProductionState.IsRunning;
                    b.MinNextPickup = Helper.GenerateNextInterval(j["state"]["next_state_transition_in"].ToObject<int>());
                    break;
                case "IdleState":
                    b.ProductionState = ProductionState.Idle;
                    b.MinNextPickup = DateTime.MinValue;
                    break;
                case "ConstructionState": b.ProductionState = ProductionState.Construction; break;
                case "UnconnectedState": b.ProductionState = ProductionState.Unconnected; break;
                default:
                    Debug.WriteLine("Unknown building state: " + j["state"]["__class__"]);
                    return null;
            }

            if (b.Type == BuildType.Residential || b.Type == BuildType.MainBuilding) b.ProductionTime = j["state"]["current_product"]?["production_time"]?.ToObject<int>() ?? 300;

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
        TwoDays = 4
    }

    public enum TimeIntervalSupplies
    {
        FiveMinutes = 1,
        FiftenMinutes = 2,
        OneHour = 3,
        FourHours = 4,
        EightHours = 5,
        OneDay = 6
    }
}
