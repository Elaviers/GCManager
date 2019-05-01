using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GCManager
{
    public partial class App : Application
    {
        public static readonly string VERSION = "1.3.3";

        public MainWindow window = null;

        public System.Windows.Controls.ItemCollection JobListItems = null;
        App()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\ror2mm"))
            {
                string applicationLocation = typeof(App).Assembly.Location;

                key.SetValue("", "URL:GCManager Protocol");
                key.SetValue("URL Protocol", "");

                using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                {
                    defaultIcon.SetValue("", System.IO.Path.GetFileName(applicationLocation));
                }

                using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                {
                    commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                }
            }
        }

        private void StartupEvent(object sender, StartupEventArgs e)
        {
            var args = e.Args;
            if (args != null && args.Count() > 0)
            {
                ModInstallWindow mi = new ModInstallWindow(args[0]);
                JobListItems = mi.Jobs.LV.Items;
                mi.Show();

                return;
            }

            window = new MainWindow();
            JobListItems = window.Jobs.LV.Items;
            window.Show();
        }
    }
}
