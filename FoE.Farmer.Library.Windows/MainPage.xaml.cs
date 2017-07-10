//#define UI_BROWSER

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using CefSharp;
#if UI_BROWSER
using CefSharp.Wpf;
#else
using CefSharp.OffScreen;
#endif
using FoE.Farmer.Library.Windows.Helpers;
using MiCHALosoft;
using Newtonsoft.Json.Linq;

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
        private static bool ConfigLoaded = false;
        private static bool AutoStart = false;
        private static bool IsScriptLoaded = false;
        private static bool FrameLoaded = false;
        private static LogMessageType ShowLogMessageType = LogMessageType.AllWithoutRequest;

        public MainPage(Window w)
        {
            InitBrowser();
            InitializeComponent();
#if UI_BROWSER
            BrowserGrid.Children.Add(Browser);
#else
            BrowserTabItem.Visibility = Visibility.Collapsed;
#endif
            RequestObject.DataRecived += (o, args) =>
            {
                payloads[args.Id].TaskSource.SetResult(JArray.Parse(args.Data));
                ForgeOfEmpires.Manager.ParseStringData(args.Data);
            };

            LoadConfig();

            w.Closed += (sender, args) =>
            {
                Manager.SaveCache();
                SaveConfig();
                Config.Save();
                Cef.Shutdown();
                Environment.Exit(0);
            };

            Requests.PayloadSendRequest += (requests, args) =>
            {
                SendRequest(args.Payload);
            };

            Manager.LogMessageSend += (manager, args) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if ((ShowLogMessageType & args.Type) == args.Type)
                    {
                        LogBox.AppendText(args.Message + "\n");
                        LogBox.ScrollToEnd();
                    }
                });
                //var tr = new TextRange(LogBox.Document.ContentEnd, LogBox.Document.ContentEnd) {Text = args.Message};
                //tr.ApplyPropertyValue(TextElement.­ForegroundProperty, Brushes.Red);
            };

            Manager.Log("Logging...");

            ForgeOfEmpires.Manager.LogoutEvent += (manager, args) =>
            {
                Relogin();
            };

        }

        public void InitBrowser()
        {
#if UI_BROWSER
            Cef.EnableHighDPISupport();
#endif

            var settings = new CefSettings();
            settings.RemoteDebuggingPort = 8088;
            settings.WindowlessRenderingEnabled = true;
            settings.CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiCHALosoft\\FoFBot\\Cache");//@"C:\Temp\Cache";
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

#if UI_BROWSER
            Browser = new ChromiumWebBrowser();
            Browser.WebBrowser = Browser;
            Browser.Address = new Uri("https://cz.forgeofempires.com/").AbsoluteUri;
#else
            Browser = new ChromiumWebBrowser(new Uri("https://cz.forgeofempires.com/").AbsoluteUri);
            Browser.RegisterAsyncJsObject("responseManager", new RequestObject());
#endif

            Browser.BrowserSettings.DefaultEncoding = "UTF-8";
            Browser.BrowserSettings.FileAccessFromFileUrls = CefState.Enabled;
            Browser.BrowserSettings.WindowlessFrameRate = 60;

            Browser.RequestHandler = new RequestHandler();
            //Browser.IsBrowserInitializedChanged += (sender, args) =>
            //{
            //    if (Browser.IsBrowserInitialized)
            //    {
            //    }
            //};
            Browser.FrameLoadEnd += (sender, args) =>
            {
                Dispatcher.Invoke(() =>
                {
                    FrameLoaded = true;

                    if (UserName.Text.Length == 0 || Password.Password.Length == 0) return;
                    LoadScripts();
                });


            };
            //LoadScripts();
        }

        private static void LoadScripts()
        {
            IsScriptLoaded = true;

            var assembly = Assembly.GetExecutingAssembly();
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

        public void Relogin()
        {
            Dispatcher.Invoke(() =>
            {
                CookieLoaded = false;
                //Browser.Address = new Uri("https://cz.forgeofempires.com/").AbsoluteUri;
                Browser.Load(new Uri("https://cz.forgeofempires.com/").AbsoluteUri);

                AutoStart = true;
            });
        }

        public static void ShowCookies()
        {
            if (CookieLoaded) return;
            CookieLoaded = true;
            var visitor = new CookieManager();
            if (Cef.GetGlobalCookieManager().VisitAllCookies(visitor))
                visitor.WaitForAllCookies();
            foreach (var nameValue in visitor.NamesValues)
            {
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

            Manager.Log("Success login");

            if (AutoStart)
            {
                Task.Delay(3000).ContinueWith((t) =>
                {
                    SendRequest(Payloads.StartupService.GetData());
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        private static void LoadRequestString()
        {

            var FncSend = "function sendRequest(data, signature){return new Promise((r, c) => {$.ajax({type: 'POST',url: '/game/json?h=" + Requests.UserKey + "', data: data, contentType: 'application/json', dataType: 'json', beforeSend: (xhr) => {" +
                          "xhr.setRequestHeader('Signature', signature);" +
                          $"xhr.setRequestHeader('Client-Identification', '{Requests.TemplateRequestHeader["Client-Identification"]}');" +
                          $"xhr.setRequestHeader('X-Requested-With', '{Requests.TemplateRequestHeader["X-Requested-With"]}');" +
                          "},success: function(result) {r(result);},error: function(result) { c(result);}});});}";

            Browser.ExecuteScriptAsync(FncSend);
            ForgeOfEmpires.Manager.IsInitialized = true;
        }

        public static async void SendRequest(Payload payload)
        {
            Manager.Log("Request sent: " + payload.ToString(), LogMessageType.Request);
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

            if (WorldName.Text != null)
            {
                Requests.WorldName = WorldName.Text.Trim();
                Config.SetValue("WorldName", Requests.WorldName);
            }

            if (!IsScriptLoaded && Password.Password.Length > 0 && UserName.Text.Trim().Length > 0 && FrameLoaded)
            {
                LoadScripts();
            }
        }

        private void StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ForgeOfEmpires.Manager.IsStarted)
            {
                ForgeOfEmpires.Manager.Stop();
                (sender as Button).Content = "Start";
            }
            else
            {
                if (ForgeOfEmpires.Manager.IsStartupServicesLoad) ForgeOfEmpires.Manager.Start();
                else
                {
                    if (!CookieLoaded)
                    {
                        MessageBox.Show("Wait for login");
                        return;
                    }
                    if (!IsScriptLoaded)
                    {
                        MessageBox.Show("Script not injected. Maybe login data not filled in");
                        return;
                    }
                    SendRequest(Payloads.StartupService.GetData());
                }
                (sender as Button).Content = "Stop";
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (Browser.IsBrowserInitialized) Browser.ShowDevTools();
        }

        private void ButtonBase1_OnClick(object sender, RoutedEventArgs e)
        {
            AutoStart = ForgeOfEmpires.Manager.IsStarted;

            CookieLoaded = false;
            //Browser.Address = new Uri("https://cz.forgeofempires.com/").AbsoluteUri;
            Browser.Load(new Uri("https://cz.forgeofempires.com/").AbsoluteUri);
        }

        private void Password_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Password.Password.Length > 0)
            {
                Requests.Password = Password.Password;
                var pwd = StringCipher.Encrypt(Password.Password, "FoEMultiBot");
                Config.SetValue("Password", pwd);
            }

            if (!IsScriptLoaded && Password.Password.Length > 0 && UserName.Text.Trim().Length > 0 && FrameLoaded)
            {
                LoadScripts();
            }
        }

        private void UpdateUserInterval()
        {
            if (!ConfigLoaded) return;

            var goodsTimer = ConfigGrid.Children.OfType<RadioButton>().Where(item => item.GroupName == "GoodsTimer").FirstOrDefault(r => r.IsChecked.Value)?.Tag.ToString();
            var suppliesTimer = ConfigGrid.Children.OfType<RadioButton>().Where(item => item.GroupName == "SuppliesTimer").FirstOrDefault(r => r.IsChecked.Value)?.Tag.ToString();
            var residentalTimer = ConfigGrid.Children.OfType<RadioButton>().Where(item => item.GroupName == "ResidentalTimer").FirstOrDefault(r => r.IsChecked.Value)?.Tag.ToString();

            if (!Enum.TryParse(goodsTimer, false, out TimeIntervalGoods goodsEnum))
            {
                goodsEnum = TimeIntervalGoods.EightHours;
            }

            if (!Enum.TryParse(suppliesTimer, false, out TimeIntervalSupplies suppliesEnum))
            {
                suppliesEnum = TimeIntervalSupplies.EightHours;
            }

            if (!Enum.TryParse(residentalTimer, false, out TimeIntervalSupplies residentalEnum))
            {
                residentalEnum = TimeIntervalSupplies.EightHours;
            }

            ForgeOfEmpires.Manager.UserIntervalGoods = goodsEnum;
            ForgeOfEmpires.Manager.UserIntervalSupplies = suppliesEnum;
            ForgeOfEmpires.Manager.UserIntervalResidental = residentalEnum;
        }

        public void LoadConfig()
        {
            var pwd = Config.GetValue("Password");
            if (!string.IsNullOrEmpty(pwd))
            {
                pwd = StringCipher.Decrypt(pwd, "FoEMultiBot");
                Requests.Password = pwd;
                Password.Password = pwd;
            }
            pwd = Config.GetValue("UserName");
            if (!string.IsNullOrEmpty(pwd))
            {
                Requests.UserName = pwd;
                UserName.Text = pwd;
            }

            pwd = Config.GetValue("WorldName");
            if (!string.IsNullOrEmpty(pwd))
            {
                Requests.WorldName = pwd;
                WorldName.Text = pwd;
            }

            var val = Config.GetValue("GoodsTimer");
            if (!string.IsNullOrEmpty(val))
            {
                var goodsTimer = ConfigGrid.Children.OfType<RadioButton>().Where(item => item.GroupName == "GoodsTimer").FirstOrDefault(r => r.Tag.ToString() == val);
                if (goodsTimer != null) goodsTimer.IsChecked = true;
            }

            val = Config.GetValue("SuppliesTimer");
            if (!string.IsNullOrEmpty(val))
            {
                var goodsTimer = ConfigGrid.Children.OfType<RadioButton>().Where(item => item.GroupName == "SuppliesTimer")
                    .FirstOrDefault(r => r.Tag.ToString() == val);
                if (goodsTimer != null) goodsTimer.IsChecked = true;
            }

            val = Config.GetValue("ResidentalTimer");
            if (!string.IsNullOrEmpty(val))
            {
                var goodsTimer = ConfigGrid.Children.OfType<RadioButton>().Where(item => item.GroupName == "ResidentalTimer")
                    .FirstOrDefault(r => r.Tag.ToString() == val);
                if (goodsTimer != null) goodsTimer.IsChecked = true;
            }
            AutoLoginCheck.IsChecked = Config.GetValueAsBool("AutoLoginAfterStart");
            ConfigLoaded = true;
            UpdateUserInterval();
        }

        public void SaveConfig()
        {
            if (UserName.Text.Trim().Length > 0)
            {
                Requests.UserName = UserName.Text.Trim();
                Config.SetValue("UserName", Requests.UserName);
            }
            if (WorldName.Text != null)
            {
                Requests.WorldName = WorldName.Text.Trim();
                Config.SetValue("WorldName", Requests.WorldName);
            }
            if (Password.Password.Length > 0)
            {
                var pwd = StringCipher.Encrypt(Password.Password, "FoEMultiBot");
                Requests.Password = Password.Password;
                Config.SetValue("Password", pwd);
            }
            var goodsTimer = ConfigGrid.Children.OfType<RadioButton>().Where(item => item.GroupName == "GoodsTimer").FirstOrDefault(r => r.IsChecked.Value)?.Tag.ToString();
            var suppliesTimer = ConfigGrid.Children.OfType<RadioButton>().Where(item => item.GroupName == "SuppliesTimer").FirstOrDefault(r => r.IsChecked.Value)?.Tag.ToString();
            var residentalTimer = ConfigGrid.Children.OfType<RadioButton>().Where(item => item.GroupName == "ResidentalTimer").FirstOrDefault(r => r.IsChecked.Value)?.Tag.ToString();

            if (goodsTimer != null)
            {
                Config.SetValue("GoodsTimer", goodsTimer);
            }
            if (suppliesTimer != null)
            {
                Config.SetValue("SuppliesTimer", suppliesTimer);
            }
            if (residentalTimer != null)
            {
                Config.SetValue("ResidentalTimer", residentalTimer);
            }

            Config.SetValue("AutoLoginAfterStart", AutoLoginCheck.IsChecked.Value.ToString());
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            UpdateUserInterval();
        }

        private void AutoLoginCheck_OnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
