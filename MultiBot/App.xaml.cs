#define DISABLE_GPU
using CefSharp;
using CefSharp.DevTools.Network;
using CefSharp.Wpf;
using System;
using System.IO;
using System.Windows;


namespace MultiBot
{
    /// <summary>
    /// Interakční logika pro App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string TempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiCHALosoft", "FoEBot");
        public App()
        {
#if ANYCPU
            //Only required for PlatformTarget of AnyCPU
            CefRuntime.SubscribeAnyCpuAssemblyResolver();
#endif
#if UI_BROWSER
            Cef.EnableHighDPISupport();
#endif
            var settings = new CefSettings
            {
                //RemoteDebuggingPort = 8088,
#if !UI_BROWSER
                //WindowlessRenderingEnabled = true,
#endif
                CachePath = Path.Combine(TempFolder, "Cache"),//@"C:\Temp\Cache";
                //UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.4.130.0 Safari/537.36",
                //LogFile = Path.Combine(TempFolder, "Debug.log"),
            };

            //Enables WebRTC
            // - CEF Doesn't currently support permissions on a per browser basis see https://bitbucket.org/chromiumembedded/cef/issues/2582/allow-run-time-handling-of-media-access
            // - CEF Doesn't currently support displaying a UI for media access permissions
            //
            //NOTE: WebRTC Device Id's aren't persisted as they are in Chrome see https://bitbucket.org/chromiumembedded/cef/issues/2064/persist-webrtc-deviceids-across-restart
            settings.CefCommandLineArgs.Add("enable-media-stream");
            //https://peter.sh/experiments/chromium-command-line-switches/#use-fake-ui-for-media-stream
            settings.CefCommandLineArgs.Add("use-fake-ui-for-media-stream");
            //For screen sharing add (see https://bitbucket.org/chromiumembedded/cef/issues/2582/allow-run-time-handling-of-media-access#comment-58677180)
            settings.CefCommandLineArgs.Add("enable-usermedia-screen-capturing");
#if DISABLE_GPU
            settings.CefCommandLineArgs.Add("disable-gpu"); // Disable GPU acceleration
            //settings.CefCommandLineArgs.Add("disable-gpu-vsync"); //Disable GPU vsync
            //settings.CefCommandLineArgs.Add("disable-3d-apis"); //Disable GPU vsync
            settings.CefCommandLineArgs.Add("headless");
            settings.CefCommandLineArgs.Add("disable-software-rasterizer");
            
#endif

            //settings.CefCommandLineArgs.Add("allow-universal-access-from-files", String.Empty);
            //settings.CefCommandLineArgs.Add("allow-file-access-from-files", String.Empty);
            settings.IgnoreCertificateErrors = true;
            /*settings.RegisterScheme(new CefCustomScheme
            {
                SchemeName = FoFSchemeHandlerFactory.SchemeName,
                SchemeHandlerFactory = new FoFSchemeHandlerFactory()
            });*/

            if (!Cef.IsInitialized)
                if (!Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null))
                    throw new Exception();
        }
    }
}
