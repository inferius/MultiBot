using CefSharp;
using System;
using System.Windows;

namespace MultiBot
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        FoE.Farmer.Library.Windows.MainPage d;
        public MainWindow()
        {
            InitializeComponent();
            d = new FoE.Farmer.Library.Windows.MainPage(this);
            MainFrame.Navigate(d);

            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            d.Close();
            Cef.Shutdown();
            Application.Current.Shutdown(0);
            Environment.Exit(0);
        }

    }
}
