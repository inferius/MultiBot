using FoE.Farmer.Library.Windows.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FoE.Farmer.Library.Windows
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Config : INotifyPropertyChanged
    {
        public const string PasswordSalt = "FoEMultiBot";
        private string password = "";
        private string userName = "";
        private string worldName = "";
        private string domain = "";
        private TimerEnum goodsTimer = TimerEnum.FourHours;
        private TimerEnum suppliesTimer = TimerEnum.FiveMinutes;
        private TimerEnum residentalTimer = TimerEnum.FiveMinutes;
        private int tavernMinOccupation = 100;
        private bool autoLoginAfterStart = true;

        /// <summary>
        /// Password
        /// </summary>
        [JsonConverter(typeof(PasswordJsonConverter))]
        public string Password { get => password; set { password = value; OnPropertyChanged(); } }
        /// <summary>
        /// Username
        /// </summary>
        public string UserName { get => userName; set { userName = value; OnPropertyChanged(); } }
        /// <summary>
        /// Domain when used for your world
        /// </summary>
        public string Domain { get => domain; set { domain = value; OnPropertyChanged(); } }
        /// <summary>
        /// Timer for harvest goods
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TimerEnum GoodsTimer { get => goodsTimer; set { goodsTimer = value; OnPropertyChanged(); } }
        /// <summary>
        /// Timer for harvest supplies building
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TimerEnum SuppliesTimer { get => suppliesTimer; set { suppliesTimer = value; OnPropertyChanged(); } }
        /// <summary>
        /// Timer for harvest village building
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TimerEnum ResidentalTimer { get => residentalTimer; set { residentalTimer = value; OnPropertyChanged(); } }
        /// <summary>
        /// Minimal occupation for harvesting tavern
        /// </summary>
        public int TavernMinOccupation { get => tavernMinOccupation; set { tavernMinOccupation = value; OnPropertyChanged(); } }
        public bool AutoLoginAfterStart { get => autoLoginAfterStart; set { autoLoginAfterStart = value; OnPropertyChanged(); } }
        /// <summary>
        /// World Name to login
        /// </summary>
        public string WorldName { get => worldName; set { worldName = value; OnPropertyChanged(); } }

        [field: NonSerializedAttribute]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public object GetPropValue(string propName)
        {
            return GetType().GetProperty(propName).GetValue(this, null);
        }

        public T GetPropValue<T>(string name)
        {
            Object retval = GetPropValue(name);
            if (retval == null) { return default(T); }

            // throws InvalidCastException if types are incompatible
            return (T)retval;
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this));
        }
        
        public void Load(string path)
        {
            JsonConvert.PopulateObject(File.ReadAllText(path), this);
        }

    }

    public class PasswordJsonConverter : JsonConverter<string>
    {
        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value.ToString();
            if (hasExistingValue && value.Length > 0) return StringCipher.Decrypt(value, Config.PasswordSalt);
            return string.Empty;
        }

        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
        {
            writer.WriteValue(StringCipher.Encrypt(value, Config.PasswordSalt));
        }
    }
}
