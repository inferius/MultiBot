//#define UI_BROWSER
//#define UI_WEBFORM
//#define DISABLE_GPU

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using CefSharp;
using FoE.Farmer.Library.Windows.Events;
#if UI_BROWSER
#if UI_WEBFORM
using CefSharp.WinForms;
#else
using CefSharp.Wpf;
#endif
#else
using CefSharp.OffScreen;
#endif
using FoE.Farmer.Library.Windows.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Data;
using MahApps.Metro.Controls;
using CefSharp.DevTools.Network;

namespace FoE.Farmer.Library.Windows
{
    /// <summary>
    /// Interakční logika pro MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        //public Config Config = null;
        private static ChromiumWebBrowser Browser;
        //private static CefSharp.Wpf.ChromiumWebBrowser otherBrowser;
        private static bool CookieLoaded = false;
        private static Dictionary<string, Payload> payloads = new Dictionary<string, Payload>();
        private static bool ConfigLoaded = false;
        private static bool AutoStart = false;
        private static bool IsScriptLoaded = false;
        private static bool FrameLoaded = false;
        private static LogMessageType ShowLogMessageType = LogMessageType.AllWithoutRequest;
        private static bool IsReloginRunning = false;
        private static readonly string TempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiCHALosoft", "FoEBot");
        private static readonly string ConfigPath = Path.Combine(TempFolder, "FoE.Config.json");
        private bool IsOtherBrowserInit = false;
        private BrowserSettings _globalUIBrowserSetting = new BrowserSettings
        {
            DefaultEncoding = "UTF-8",
            WindowlessFrameRate = 30
        };

        public MainPage(Window w)
        {
            InitializeComponent();
            Directory.CreateDirectory(TempFolder);

            RequestObject.DataRecived += (o, args) =>
            {
                if (args.IsError == RecivedDataType.Error)
                {
                    Manager.Log(args.Data, LogMessageType.Error);
                }
                else
                {
                    payloads[args.Id].TaskSource.SetResult(JArray.Parse(args.Data));
                    ForgeOfEmpires.Manager.ParseStringData(args.Data);
                }
            };

            otherBrowser.BrowserSettings = _globalUIBrowserSetting;

            LoadConfig();

            LoadOtherResourcesHtml();

            InitBrowser();

            var random = new Random();
            Requests.PayloadSendRequest += async (requests, args) =>
            {
                var delay = random.Next(50, 400);
                Manager.Log($"Request delay: {delay}ms", LogMessageType.Request);
                await Task.Delay(delay);
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

            if (UserName.Text.Length == 0 || Password.Password.Length == 0)
            {
                Manager.Log("Login failed. User name or password is empty. Fill in login data and click on Relogin");
            }

            ForgeOfEmpires.Manager.LogoutEvent += (manager, args) =>
            {
                if (IsReloginRunning) return;
                IsReloginRunning = true;
                Relogin();
            };

            ForgeOfEmpires.Manager.ResourcesUpdate += (manager, args) =>
            {
                try
                {
                    foreach (var value in args.Values)
                    {
                        switch (value.Item1)
                        {
                            //case "tavern_silver": TavernSilver.Dispatcher.Invoke(() => TavernSilver.Text = value.Item2.ToString(CultureInfo.CurrentUICulture)); break;
                            //case "premium": DiamondBox.Dispatcher.Invoke(() => DiamondBox.Text = value.Item2.ToString(CultureInfo.CurrentUICulture)); break;
                            //case "money": MoneyBox.Dispatcher.Invoke(() => MoneyBox.Text = value.Item2.ToString(CultureInfo.CurrentUICulture)); break;
                            //case "supplies": SupplyBox.Dispatcher.Invoke(() => SupplyBox.Text = value.Item2.ToString(CultureInfo.CurrentUICulture)); break;
                            //case "strategy_points": ForgePointsBox.Dispatcher.Invoke(() => ForgePointsBox.Text = value.Item2.ToString(CultureInfo.CurrentUICulture)); break;
                            //case "medals": Medals.Dispatcher.Invoke(() => Medals.Text = value.Item2.ToString(CultureInfo.CurrentUICulture)); break;
                            default:
                                otherBrowser.ExecuteScriptAsync("updateItem", value.Item1, value.Item2);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Manager.Log("Resource update Exception: " + e.ToString(), LogMessageType.Exception);
                }
            };

        }

        private void InitOtherBrowser()
        {
            if (IsOtherBrowserInit) return;
            IsOtherBrowserInit = true;
            //otherBrowser = new CefSharp.Wpf.ChromiumWebBrowser();
            otherBrowser.WebBrowser = otherBrowser;
            //otherBrowser.Address = new Uri("http://api.michalosoft.cz/ResourceService.html").AbsoluteUri;
            otherBrowser.Address = new Uri(Path.Combine(TempFolder, "OtherResources.html")).AbsoluteUri;

            //otherBrowser.Margin = new Thickness(10, 113, 10.4, 10.4);
            otherBrowser.Width = double.NaN;
            otherBrowser.Height = double.NaN;

            ForegroundGrid.Children.Remove(otherBrowser);
            ResourceInfoGrid.Children.Add(otherBrowser);
        }

        private void InitBrowser()
        {
#if UI_BROWSER
#if UI_WEBFORM
            Browser = new ChromiumWebBrowser(new Uri($"https://{Requests.Domain}/").AbsoluteUri);
#else
            Browser = new ChromiumWebBrowser();
            Browser.WebBrowser = Browser;
            Browser.Address = new Uri($"https://{Requests.Domain}/").AbsoluteUri;
#endif
#else
            var browserSetting = new BrowserSettings
            {
                DefaultEncoding = "UTF-8",
                //WindowlessFrameRate = 30
            };

            Browser = new ChromiumWebBrowser(new Uri($"https://{Requests.Domain}/").AbsoluteUri, browserSetting, automaticallyCreateBrowser: false);
#endif

            Browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            Browser.JavascriptObjectRepository.Register(AP._f("responseManager"), new RequestObject(), isAsync: true, options: BindingOptions.DefaultBinder);
            Browser.RequestHandler = new FoFRequestHandler();

            Browser.ConsoleMessage += (sender, args) =>
            {
                Manager.Log(args.Message);
            };


            Browser.FrameLoadEnd += (sender, args) =>
            {
                if (args.Frame.IsMain) {
                    Dispatcher.Invoke(() => {
                        FrameLoaded = true;

                        /*args.Frame.EvaluateScriptAsync(@"Array.from(document.querySelectorAll(""script[src]"")).map(item => item.getAttribute(""src"")).find(item => item?.includes(""innogamescdn.com/cache/ForgeHX""))").ContinueWith(x => {
                            var response = x.Result;
                            if (response.Success && response.Result != null) {
                                using (var f = new StreamReader(response.Result.ToString())) {
                                    var t = f.ReadToEnd();
                                    Application.Current.Dispatcher.Invoke(() => {
                                        Manager.Log("SID FOUND");
                                    });

                                }
                            }
                        });*/

                        if (UserName.Text.Length == 0 || Password.Password.Length == 0) return;
                        LoadScripts();
                    });
                }

            };
#if !UI_BROWSER
            Browser.CreateBrowser();
#endif

            //LoadScripts();

#if UI_BROWSER
#if UI_WEBFORM
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
            host.Child= Browser;
            BrowserGrid.Children.Add(host);
#else
            BrowserGrid.Children.Add(Browser);
#endif
#else
            BrowserTabItem.Visibility = Visibility.Collapsed;
#endif
        }

        private static void LoadScripts()
        {

            var result = new StringBuilder();

            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "FoE.Farmer.Library.Windows.External.Inject.js";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var wn = Requests.WorldName == null ? "null" : $"'{Requests.WorldName}'";
                result.Append(reader.ReadToEnd());
                result.Insert(0, $"var FoELoginUserName = '{Requests.UserName}'; var FoELoginPassword = '{Requests.Password}'; var FoEWordName = {wn};");
                result.Replace("FoELoginUserName", AP._f("FoELoginUserName"));
                result.Replace("FoELoginPassword", AP._f("FoELoginPassword"));
                result.Replace("FoEWordName", AP._f("FoEWordName"));
                result.Replace("FoEInit", AP._f("FoEInit"));
                result.Replace("FoELogin", AP._f("FoELogin"));
                result.Replace("FoFTimer", AP._f("FoFTimer"));
                result.Replace("FoEPlay", AP._f("FoEPlay"));

            }

            Browser.ExecuteScriptAsync(result.ToString());
            IsScriptLoaded = true;

        }

        private void LoadOtherResourcesHtml()
        {
            //otherBrowser.Load("http://api.michalosoft.cz/ResourceService.html");
            //return;
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "FoE.Farmer.Library.Windows.External.OtherResources.html";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd();
                File.WriteAllText(Path.Combine(TempFolder, "OtherResources.html"), result);
                //otherBrowser.LoadHtml(result, "http://www.example.com/");
            }
        }

        public void Relogin()
        {
            Dispatcher.Invoke(() =>
            {
                CookieLoaded = false;
                //Browser.Address = new Uri("https://cz.forgeofempires.com/").AbsoluteUri;
                Browser.Load(new Uri($"https://{Requests.Domain}/").AbsoluteUri);

                //AutoStart = true;
            });
        }

        public static async void ShowCookies()
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

            // after relogin must start timer
            if (IsReloginRunning)
            {
                Manager.Log("Timer start");
                ForgeOfEmpires.Manager.Start();
            }

            if (AutoStart && !IsReloginRunning)
            {
                await Task.Delay(2000);
                await SendRequest(Payloads.StartupService.GetData());
                Manager.Log("Success login. (StartupService)");
            }
            else Manager.Log("Success login");

            IsReloginRunning = false;
        }

        private static void LoadRequestString()
        {

            var FncSend = "function "+ AP._f("sendRequest") + "(data, signature){return new Promise((r, c) => {$.ajax({type: 'POST',url: '/game/json?h=" + Requests.UserKey + "', data: data, contentType: 'application/json', dataType: 'json', beforeSend: (xhr) => {" +
                          "xhr.setRequestHeader('Signature', signature);" +
                          $"xhr.setRequestHeader('Client-Identification', '{Requests.TemplateRequestHeader["Client-Identification"]}');" +
                          //$"xhr.setRequestHeader('X-Requested-With', '{Requests.TemplateRequestHeader["X-Requested-With"]}');" +
                          "},success: function(result) {r(result);},error: function(result) { c(result);}});});}";

            Browser.ExecuteScriptAsync(FncSend);
            ForgeOfEmpires.Manager.IsInitialized = true;
        }

        public static async Task<JavascriptResponse> SendRequest(Payload payload)
        {
            Manager.Log("Request sent: " + payload.ToString(), LogMessageType.Request);
            var data = "[" + payload + "]";
            var signature = Requests.BuildSignature(data);
            var script = $"return (async () => {AP._f("responseManager")}.setData(JSON.stringify(await {AP._f("sendRequest")}('{data}', '{signature}')), '{signature}') )();";

            payloads.Add(signature, payload);
            var resp = await Browser.EvaluateScriptAsPromiseAsync(script);
            try {
                if (resp.Success) payload.TaskSource.SetResult(JArray.Parse(resp.Result.ToString()));
            }
            catch { 
                // ignore
            }

            return resp;
        }

        private void StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            InitOtherBrowser();

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
            Browser.Load(new Uri($"https://{Requests.Domain}/").AbsoluteUri);
        }

        private void Password_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Password.Password.Length > 0)
            {
                //Requests.Password = Password.Password;
                ConfigProvider.Password = Password.Password;
            }

            if (!IsScriptLoaded && Password.Password.Length > 0 && UserName.Text.Trim().Length > 0 && FrameLoaded)
            {
                LoadScripts();
            }
        }

        private void UpdateUserInterval()
        {
            if (!ConfigLoaded) return;

            ForgeOfEmpires.Manager.UserIntervalGoods = (TimeIntervalGoods)ConfigProvider.GoodsTimer;
            ForgeOfEmpires.Manager.UserIntervalSupplies = (TimeIntervalSupplies)ConfigProvider.SuppliesTimer;
            ForgeOfEmpires.Manager.UserIntervalResidental = (TimeIntervalSupplies)ConfigProvider.ResidentalTimer;
        }

        private Binding SetBindingHelper(string propertyName)
        {
            Binding myBinding = new Binding(propertyName);
            myBinding.Source = ConfigProvider;
            myBinding.Mode = BindingMode.TwoWay;

            return myBinding;
        }

        public void LoadConfig()
        {
            if (!ConfigLoaded)
            {
                ConfigProvider.Load(ConfigPath);
                ConfigProvider.PropertyChanged += (sender, arg) => {
                    switch(arg.PropertyName) {
                        case "Password": Requests.Password = ConfigProvider.Password; break;
                        case "WorldName": Requests.WorldName = ConfigProvider.WorldName; break;
                        case "UserName": Requests.UserName = ConfigProvider.UserName; break;
                        case "Domain": Requests.Domain = ConfigProvider.Domain; break;
                        case "GoodsTimer": ForgeOfEmpires.Manager.UserIntervalGoods = (TimeIntervalGoods)ConfigProvider.GoodsTimer; break;
                        case "SuppliesTimer": ForgeOfEmpires.Manager.UserIntervalSupplies = (TimeIntervalSupplies)ConfigProvider.SuppliesTimer; break;
                        case "ResidentalTimer": ForgeOfEmpires.Manager.UserIntervalResidental = (TimeIntervalSupplies)ConfigProvider.ResidentalTimer; break;
                        case "TavernMinOccupation": {
                                TavernMinOccupation.Text = ConfigProvider.TavernMinOccupation.ToString(CultureInfo.InvariantCulture);
                                Services.TavernService.MinTaverOccupation = ConfigProvider.TavernMinOccupation / 100;

                                break; 
                            }
                    }
                };
            }

            Password.Password = ConfigProvider.Password;
            Requests.Password = ConfigProvider.Password;
            Requests.WorldName = ConfigProvider.WorldName;
            Requests.UserName = ConfigProvider.UserName;
            Requests.Domain = ConfigProvider.Domain;

            UserName.SetBinding(TextBox.TextProperty, SetBindingHelper("UserName"));
            WorldName.SetBinding(TextBox.TextProperty, SetBindingHelper("WorldName"));
            Domain.SetBinding(TextBox.TextProperty, SetBindingHelper("Domain"));
            AutoLoginCheck.SetBinding(CheckBox.IsCheckedProperty, SetBindingHelper("AutoLoginCheck"));

            // Bindovani Radiogroup
            foreach (var item in ConfigGrid.FindChildren<RadioButton>()) {
                var b = new Binding(item.GroupName)
                {
                    Converter = new BooleanToTagConverter(),
                    ConverterParameter = Enum.Parse(typeof(TimerEnum), item.Tag.ToString()),
                    Mode = BindingMode.TwoWay
                };
                item.SetBinding(RadioButton.IsCheckedProperty, b);
            }

            ConfigLoaded = true;
        }

        public void SaveConfig()
        {
            ConfigProvider.Save(ConfigPath);
        }


        private void AutoLoginCheck_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private string TavernMinOcLastValidText = "75";
        private void TavernMinOccupation_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = TavernMinOccupation.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;
            if (Regex.IsMatch(text, "^([0-9]|[1-9][0-9]|100)$"))
            {
                
                TavernMinOcLastValidText = text;
                var num = text == "100" ? 1M : decimal.Parse("0." + text, CultureInfo.InvariantCulture);
                Services.TavernService.MinTaverOccupation = num;
            }
            else
            {
                TavernMinOccupation.Text = TavernMinOcLastValidText;
                TavernMinOccupation.SelectAll();
            }
        }

        private void DevToolButtonInner_OnClick(object sender, RoutedEventArgs e)
        {
            if (otherBrowser.IsBrowserInitialized)
            {
                otherBrowser.ShowDevTools();
            }
        }

        public void Close()
        {
            Browser?.CloseDevTools();
            Manager.SaveCache();
            SaveConfig();
            if (File.Exists(Path.Combine(TempFolder, "OtherResources.html"))) File.Delete(Path.Combine(TempFolder, "OtherResources.html"));

            otherBrowser?.Dispose();
            Browser?.Dispose();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
