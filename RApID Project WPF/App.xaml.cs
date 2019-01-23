using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;
using MonkeyCache.FileStore;
using Dragablz;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Barrel.ApplicationId = AppDomain.CurrentDomain.FriendlyName;

            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }
}
