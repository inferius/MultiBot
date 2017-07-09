using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library
{
    public class Manager
    {
        public List<Player> Players { get; } = new List<Player>();
        public Player Me { get; set; }
        public Requests Requests { get; set; } = new Requests();
        public Player GetPlayerById(int id) => Players.Find(item => item.ID == id);


        public void PickupBuildings()
        {
            foreach (var building in Me.Buildings)
            {
                
            }
            
        }

        public async void StartupService()
        {
            var startUpPayload = Payloads.StartupService.GetData();
            
            Requests.AddPayload(startUpPayload/*, (j) =>
            {
                File.WriteAllText("debug_response.txt", j.ToString(Formatting.Indented));
            }*/);

            //var data = await startUpPayload.Task;
            //File.WriteAllText("debug_response.txt", data.ToString(Formatting.Indented));
        }

        public void ParseStringData(string data)
        {
            var ja = JArray.Parse(data);

            foreach (var item in ja)
            {
                var j = item as JObject;

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
                        break;
                }
                
            }
        }

        public void ParseStartupData()
        {
            
        }
    }
}
