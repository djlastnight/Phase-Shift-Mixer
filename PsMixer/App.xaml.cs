namespace PsMixer
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
        }
    }
}