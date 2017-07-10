using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Flurl;
using Flurl.Http;
using FoE.Farmer.Library.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library
{
    public class Requests
    {
        public static event PayloadSendRequestHandler PayloadSendRequest;
        public delegate void PayloadSendRequestHandler(Requests m, Events.PayloadRequestEventArgs e);

        public static string UserName = "nms10266@uzrip.com";
        public static string Password = "nms10266@uzrip.com";
        public static string WorldName = null;

        public const string BaseAddress = "{0}.forgeofempires.com";
        public const string AddressTemplate = "https://{0}/game/json?h={1}";
        public static readonly HttpClient Client = new HttpClient();
        public const string Secret = "oDtc9YJ7nRbNf3ui2AS/7yrFS4k4MGgjWCr6/Komfxmq9ue5EJBY1apDyRMV2fdvaLkQ3AnJrsMhWfdbSIieCw==";
        public static string Timestamp = "1498641829";

        private const string GameVersion = "1.104";

        public static Dictionary<string, string> TemplateRequestHeader { get; set; } = new Dictionary<string, string>();
        //nms10266@uzrip.com

        private static string Server { get; set; } = "cz5";
        public static string UserKey { get; set; } = "QImVFSEspfcKkw5KT4D5CVjq";
        // Cookies
        public static string SID { get; set; } = "9P5Tup3ajM04nLdxKgkVT0GO6j4VTHq5p95Yq5Ub";
        public static string _GA { get; set; } = "";
        public static string _GID { get; set; } = "";
        public static string StartupMicrotime { get; set; } = "";
        public static string MetricsUvId { get; set; } = "";
        public static string IgLastSite { get; set; } = "";

        public string RequestsAddress => string.Format(AddressTemplate, Domain, UserKey);
        internal static string Domain => string.Format(BaseAddress, Server);

        private readonly Queue<(Payload, Action<JObject>)> payloads = new Queue<(Payload, Action<JObject>)>();
        private Timer requestSendTimer = new Timer(500);

        public Requests()
        {
            requestSendTimer.Elapsed += (e,s) => SendPayload();
            //requestSendTimer.Start();
        }

        public void AddPayload(Payload payload, Action<JObject> callback = null) 
        {
            payloads.Enqueue((payload, callback));
            if (payloads.Count == 1) requestSendTimer.Start();
        }

        private void SendPayload()
        {
            if (payloads.Count == 0) return;
            if (string.IsNullOrWhiteSpace(UserKey) || string.IsNullOrWhiteSpace(SID)) return;
            if (payloads.Count == 1) requestSendTimer.Stop();

            var item = payloads.Dequeue();
            if (item.Item1.TaskSource.Task.IsCanceled) return;

            PayloadSendRequest?.Invoke(this, new PayloadRequestEventArgs { Payload = item.Item1 });
        }

        public static string BuildSignature(string data)
        {
            return MD5(UserKey + Secret + data).Substring(0, 10);
        }

        private static string MD5(string data)
        {
            // byte array representation of that string
            var encodedData = new UTF8Encoding().GetBytes(data);

            // need MD5 to calculate the hash
            var hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedData);

            // string representation (similar to UNIX format)
            return BitConverter.ToString(hash)
                // without dashes
                .Replace("-", string.Empty)
                // make lowercase
                .ToLower();
        }

        private string SendRequest2(Payload data)
        {
            var jsonString = "[" + data + "]";
            string response = null;

            var wr = (HttpWebRequest)WebRequest.Create(RequestsAddress);

            wr.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            wr.Method = "POST";
            wr.ContentType = "application/json";
            wr.KeepAlive = true;
            wr.Accept = "*/*";
            wr.Host = Domain;
            wr.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            wr.Referer = $"https://foecz.innogamescdn.com/swf/Preloader.swf?{Timestamp}/[[DYNAMIC]]/1";
            wr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
            wr.CookieContainer = new CookieContainer();
            wr.CookieContainer.Add(GetCookie("metricsUvId", "98cf6fd3-5872-4080-bf40-934b90b70a71"));
            wr.CookieContainer.Add(GetCookie("sid", SID));
            wr.CookieContainer.Add(GetCookie("req_page_info", "game_v1"));
            wr.CookieContainer.Add(GetCookie("start_page_type", "game"));
            wr.CookieContainer.Add(GetCookie("start_page_version", "v1"));
            wr.CookieContainer.Add(GetCookie("_ga", "GA1.2.1298412460.1491422581"));
            wr.CookieContainer.Add(GetCookie("ig_conv_last_site", $"https://{Domain}/game/index"));


            //wr.Headers["Connection"] = "keep-alive";
            wr.Headers["Client-Identification"] = $"version={GameVersion}; requiredVersion={GameVersion}; platform=bro; platformVersion=web";
            wr.Headers["Origin"] = "https://foecz.innogamescdn.com";
            wr.Headers["X-Requested-With"] = "ShockwaveFlash/26.0.0.131";
            wr.Headers["Signature"] = BuildSignature(jsonString);
            //wr.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
            //wr.Headers["Referer"] = $"https://foecz.innogamescdn.com/swf/Preloader.swf?{Timestamp}/[[DYNAMIC]]/1";
            wr.Headers["Accept-Encoding"] = "gzip, deflate, br";
            wr.Headers["Accept-Language"] = "cs-CZ,cs;q=0.8";
            //wr.Headers["Cookie"] = $"metricsUvId=98cf6fd3-5872-4080-bf40-934b90b70a71; sid={SID}; req_page_info=game_v1; start_page_type=game; start_page_version=v1; _ga=GA1.2.1298412460.1491422581; ig_conv_last_site=https://{Domain}/game/index";

            using (var s = wr.GetResponse().GetResponseStream())
            {
                using (var sr = new System.IO.StreamReader(s))
                {
                    response = sr.ReadToEnd();
                }
            }

            return response;

        }

        private string SendRequest3(Payload data)
        {
            var jsonString = "[{\"requestData\":[],\"requestId\":0,\"requestMethod\":\"getData\",\"__class__\":\"ServerRequest\",\"requestClass\":\"StartupService\",\"voClassName\":\"ServerRequest\"}]";//"[" + data + "]";
            string response = null;

            //var wr = (HttpWebRequest)WebRequest.Create("http://requestb.in/rbpr4zrc");
            var wr = (HttpWebRequest) WebRequest.Create(TemplateRequestHeader["Uri"]);
            var uri = new Uri(TemplateRequestHeader["Uri"]);
            

            wr.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            wr.Method = "POST";
            wr.ContentType = TemplateRequestHeader["Content-Type"];
            wr.KeepAlive = true;
            wr.Accept = TemplateRequestHeader["Accept"];
            wr.Host = uri.Host;
            wr.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            wr.Referer = TemplateRequestHeader["Referrer"];
            //wr.ContentLength = jsonString.Length;
            wr.UserAgent = TemplateRequestHeader["User-Agent"];//"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
            //wr.CookieContainer = new CookieContainer();
            //wr.CookieContainer.Add(GetCookie("metricsUvId", MetricsUvId));
            //wr.CookieContainer.Add(GetCookie("sid", SID));
            //wr.CookieContainer.Add(GetCookie("req_page_info", "game_v1"));
            //wr.CookieContainer.Add(GetCookie("start_page_type", "game"));
            //wr.CookieContainer.Add(GetCookie("start_page_version", "v1"));
            //wr.CookieContainer.Add(GetCookie("_ga", _GA));
            //wr.CookieContainer.Add(GetCookie("_gid", _GID));
            //wr.CookieContainer.Add(GetCookie("startup_microtime", StartupMicrotime));
            //wr.CookieContainer.Add(GetCookie("ig_conv_last_site", IgLastSite));


            //wr.Headers["Connection"] = "keep-alive";
            wr.Headers["Client-Identification"] = TemplateRequestHeader["Client-Identification"];//$"version={GameVersion}; requiredVersion={GameVersion}; platform=bro; platformVersion=web";
            wr.Headers["Origin"] = TemplateRequestHeader["Origin"];
            wr.Headers["X-Requested-With"] = TemplateRequestHeader["X-Requested-With"];
            wr.Headers["Signature"] = BuildSignature(jsonString);
            //wr.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
            //wr.Headers["Referer"] = $"https://foecz.innogamescdn.com/swf/Preloader.swf?{Timestamp}/[[DYNAMIC]]/1";
            wr.Headers["DNT"] = "1";
            wr.Headers["Accept-Encoding"] = "gzip, deflate, br";
            wr.Headers["Accept-Language"] = "cs,en-US;q=0.7,en;q=0.3";
            wr.Headers["Cookie"] = $"metricsUvId={MetricsUvId}; sid={SID}; req_page_info=game_v1; start_page_type=game; start_page_version=v1; _ga={_GA}; ig_conv_last_site={IgLastSite}; startup_microtime={StartupMicrotime}; _gid={_GID}";

            using (var s = wr.GetResponse().GetResponseStream())
            {
                using (var sr = new System.IO.StreamReader(s))
                {
                    response = sr.ReadToEnd();
                }
            }

            using (var sw = new StreamWriter("_rq_data.txt", true))
            {
                sw.WriteLine($"{wr.Method} {wr.Address}");
                foreach (var key in wr.Headers.AllKeys)
                {
                    sw.WriteLine($"{key}: {wr.Headers[key]}");
                }
                sw.WriteLine();
                sw.WriteLine($"RequestData: {jsonString}");
                sw.WriteLine();
                sw.WriteLine($"ResponseData: {response}");
                sw.WriteLine();
                sw.WriteLine();

            }

            return response;

        }

        private static Cookie GetCookie(string name, string value)
        {
            return new Cookie(name, value, "/", Domain);
        }

    }
}
