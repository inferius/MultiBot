using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using MiCHALosoft;

namespace FoE.Farmer.Library.Windows
{
    /// <summary>
    /// Interakční logika pro MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public static Config Config = new Config("FoE.Config.xml");
        private static ChromiumWebBrowser Browser;
        private static bool CookieLoaded = false;
        private static Dictionary<string, Payload> payloads = new Dictionary<string, Payload>();
        public MainPage(Window w)
        {
            InitBrowser();
            InitializeComponent();
            BrowserGrid.Children.Add(Browser);

            RequestObject.DataRecived += (o, args) =>
            {
                //using (var sw = new StreamWriter("_rq_data.txt", true))
                //{
                //    sw.WriteLine(args.Data);
                //    sw.WriteLine();
                //    sw.WriteLine();

                //}
                payloads[args.Id].TaskSource.SetResult(args.Data);
                ForgeOfEmpires.Manager.ParseStringData(args.Data);
            };

            LoadConfig();

            w.Closed += (sender, args) =>
            {
                Config.Save();
                Cef.Shutdown();
                Environment.Exit(0);
            };
        }

        public void InitBrowser()
        {
            Cef.EnableHighDPISupport();



            var settings = new CefSettings();
            settings.RemoteDebuggingPort = 8088;
            settings.WindowlessRenderingEnabled = true;
            settings.CachePath = @"C:\Temp\Cache";
            settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.3071.115 Safari/537.36";

            if (settings.CefCommandLineArgs.ContainsKey("enable-system-flash"))
            {
                settings.CefCommandLineArgs["enable-system-flash"] = "1";
            }
            else
            {
                settings.CefCommandLineArgs.Add("enable-system-flash", "1");
            }

            settings.CefCommandLineArgs.Add("enable-npapi", "1");
            //settings.CefCommandLineArgs.Add("enable-system-flash", "1");
            //settings.CefCommandLineArgs.Add("ppapi-flash-version", @"26.0.0.131");
            //settings.CefCommandLineArgs.Add("ppapi-flash-path", @"C:\Temp\pepflashplayer64_26_0_0_131.dll");
            settings.IgnoreCertificateErrors = true;
            settings.RegisterScheme(new CefCustomScheme
            {
                SchemeName = FoFSchemeHandlerFactory.SchemeName,
                SchemeHandlerFactory = new FoFSchemeHandlerFactory()
            });

            if (!Cef.IsInitialized)
                if (!Cef.Initialize(settings))
                    throw new Exception();

            Browser = new ChromiumWebBrowser();
            Browser.RegisterAsyncJsObject("responseManager", new RequestObject());

            Browser.WebBrowser = Browser;

            //Browser.Address = new Uri("http://isflashinstalled.com").AbsoluteUri;
            Browser.Address = new Uri("https://cz.forgeofempires.com/").AbsoluteUri;


            //BrowserGrid.Children.Add(Browser);

            Browser.BrowserSettings.DefaultEncoding = "UTF-8";
            Browser.BrowserSettings.FileAccessFromFileUrls = CefState.Enabled;
            Browser.BrowserSettings.WindowlessFrameRate = 60;

            //Browser.IsBrowserInitializedChanged += (sender, args) => Browser.ShowDevTools();

            //Browser.WebBrowser.LoadHtml(ReadFileContents(@"Local\test.html"), "test dsd");
            //Browser.ConsoleMessage += (sender, args) => MessageBox.Show(args.Message);
            Browser.RequestHandler = new RequestHandler();
            Browser.IsBrowserInitializedChanged += (sender, args) =>
            {
                if (Browser.IsBrowserInitialized)
                {
                }
            };
            Browser.FrameLoadEnd += (sender, args) =>
            {
                LoadScripts();
            };
            //LoadScripts();
        }

        private static void LoadScripts()
        {


            var assembly = Assembly.GetExecutingAssembly();
            //var names = assembly.GetManifestResourceNames();
            const string resourceName = "FoE.Farmer.Library.Windows.External.Inject.js";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd();
                var wn = Requests.WorldName == null ? "null" : $"'{Requests.WorldName}'";
                result =
                    $"var FoELoginUserName = '{Requests.UserName}'; var FoELoginPassword = '{Requests.Password}'; var FoEWordName = {wn};" +
                    result;
                Browser.ExecuteScriptAsync(result);
            }

            //Browser.ExecuteScriptAsync()

        }

        public static void ShowCookies()
        {
            if (CookieLoaded) return;
            CookieLoaded = true;
            var visitor = new CookieManager();
            if (Cef.GetGlobalCookieManager().VisitAllCookies(visitor))
                visitor.WaitForAllCookies();
            //var sb = new StringBuilder();
            foreach (var nameValue in visitor.NamesValues)
            {
                //sb.AppendLine(nameValue.Item1 + " = " + nameValue.Item2);
                switch (nameValue.Item1)
                {
                    case "metricsUvId": Requests.MetricsUvId = nameValue.Item2; break;
                    case "sid": Requests.SID = nameValue.Item2; break;
                    case "_ga": Requests._GA = nameValue.Item2; break;
                    case "_gid": Requests._GID = nameValue.Item2; break;
                    case "startup_microtime": Requests.StartupMicrotime = nameValue.Item2; break;
                    case "ig_conv_last_site": Requests.IgLastSite = nameValue.Item2; break;
                }
                if (nameValue.Item1 == "sid")
                {
                    Requests.SID = nameValue.Item2;
                }
            }
            LoadRequestString();

            //MessageBox.Show(sb.ToString());
            //using (var sw = new StreamWriter("cookies.txt"))
            //{
            //    sw.Write(sb);
            //}

            //Cef.GetGlobalCookieManager().VisitAllCookies(visitor);
        }

        private static void LoadRequestString()
        {

            var FncSend = "function sendRequest(data, signature){return new Promise((r, c) => {$.ajax({type: 'POST',url: '/game/json?h=" + Requests.UserKey + "', data: data, dataType: 'json', beforeSend: (xhr) => {" +
                          "xhr.setRequestHeader('Signature', signature);" +
                          $"xhr.setRequestHeader('Client-Identification', '{Requests.TemplateRequestHeader["Client-Identification"]}');" +
                          $"xhr.setRequestHeader('X-Requested-With', '{Requests.TemplateRequestHeader["X-Requested-With"]}');" +
                          "},success: function(result) {r(result);},error: function(result) { c(result);}});});}";

            Browser.ExecuteScriptAsync(FncSend);
        }

        public static async void SendRequest(Payload payload)
        {
            var data = "[" + payload + "]";
            var signature = Requests.BuildSignature(data);
            var script = $"(async () => responseManager.setData(JSON.stringify(await sendRequest('{data}', '{signature}')), '{signature}') )();";

            payloads.Add(signature, payload);
            await Browser.EvaluateScriptAsync(script);

        }

        private void UserInfoChange_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (UserName.Text.Trim().Length > 0)
            {
                Requests.UserName = UserName.Text.Trim();
                Config.SetValue("UserName", Requests.UserName);
            }

            if (WorldName.Text.Trim().Length > 0)
            {
                Requests.UserName = WorldName.Text.Trim();
                Config.SetValue("WorldName", Requests.UserName);
            }
            

        }

        private async void StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            SendRequest(Payloads.StartupService.GetData());
            //ForgeOfEmpires.Manager.StartupService();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //LoadScripts();
            if (Browser.IsBrowserInitialized) Browser.ShowDevTools();
        }

        private void ButtonBase1_OnClick(object sender, RoutedEventArgs e)
        {
            Browser.Address = new Uri("https://cz.forgeofempires.com/").AbsoluteUri;
        }

        private void Password_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Password.Password.Length > 0)
            {
                Requests.Password = Password.Password.Trim();
                Config.SetValue("Password", Requests.Password);
            }
        }

        public void LoadConfig()
        {
            var pwd = Config.GetValue("Password");
            if (pwd != null)
            {
                Requests.Password = pwd;
                Password.Password = pwd;
            }
            pwd = Config.GetValue("UserName");
            if (pwd != null)
            {
                Requests.UserName = pwd;
                UserName.Text = pwd;
            }

            pwd = Config.GetValue("WorldName");
            if (pwd != null)
            {
                Requests.WorldName = pwd;
                WorldName.Text = pwd;
            }
        }
    }
}
