using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library
{
    public class MilitaryBuilding : Building
    {
        public List<UnitSlot> UnitSLots = new List<UnitSlot>();

        public override bool CanPickup
        {
            get
            {
                if (Type == BuildType.Decoration) return false;
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

        public bool HasFreeSlot => UnitSLots.Any(item => item.UnitId == -1 && item.IsUnlocked);
        public bool HasUnlockableSlot => UnitSLots.Any(item => !item.IsUnlocked && item.IsUnlockable);
        public UnitSlot GetFreeSlot => UnitSLots.FirstOrDefault(item => item.UnitId == -1 && item.IsUnlocked);
        public UnitSlot GetUnlockableSlot => UnitSLots.FirstOrDefault(item => !item.IsUnlocked && item.IsUnlockable);

        public override async Task<bool> Pickup()
        {
            if (!CanPickup) return false;

            if (Type == BuildType.Military)
            {
                var oldProdState = ProductionState;
                ProductionState = ProductionState.IsRunning;
                if (oldProdState != ProductionState.Idle)
                {
                    await Payloads.CityProductionService.PickupProduction(new[] { this }).Send();
                }

                if (HasUnlockableSlot)
                {
                    await GetUnlockableSlot.UnlockSlot();
                }

                if (HasFreeSlot)
                {
                    var resp = await Payloads.CityProductionService.StartProduction(this).Send();
                    ParseResponse(resp);
                }
            }

            return true;
        }

        private void ParseResponse(JToken r)
        {
            if (r["responseData"]["updatedEntities"] != null)
            {
                var ja = r["responseData"]["updatedEntities"] as JArray;
                var nextProductTime = ja[0]["state"]["next_state_transition_in"].ToObject<int>();

                MinNextPickup = Helper.GenerateNextInterval(nextProductTime);
            }
        }

        public int ReserveSlot()
        {
            var slot = GetFreeSlot;

            slot.UnitId = 0;

            return slot.Order;
        }
    }

    public class UnitSlot
    {
        public Building Parent { get; set; }
        public bool IsUnlocked { get; set; } = false;
        public bool IsUnlockable { get; set; } = false;
        public bool IsTraining { get; set; } = false;
        public int UnitId { get; set; } = -1;
        public int Order { get; set; } = 0;

        public static UnitSlot Parse(JToken j)
        {
            var us = new UnitSlot();
            us.IsUnlocked = j["unlocked"].ToObject<bool>();
            us.IsUnlockable = j["is_unlockable"].ToObject<bool>();
            if (j["nr"] != null) us.Order = j["nr"].ToObject<int>();
            us.UnitId = j["unit_id"].ToObject<int>();

            if (!us.IsUnlocked && us.IsUnlockable)
            {
                us.IsUnlockable = j["unlock_costs"]["premium"] == null;
            }

            return us;
        }

        public async Task UnlockSlot()
        {
            if (IsUnlocked || !IsUnlockable) return;
            await Payloads.CityProductionService.UnlockSlot(this).Send();
            IsUnlocked = true;
        }


    }
}
