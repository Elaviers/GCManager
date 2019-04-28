using System;
using System.IO;

using Newtonsoft.Json;

namespace GCManager
{
    public class ManagerInfo
    {
        public string installDir = null;

        public string DownloadDirectory = null;

        private static ManagerInfo _instance = null;

        public static ManagerInfo Get()
        {
            if (_instance == null)
            {
                string configFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

                if (File.Exists(configFileName))
                {
                    string json = File.ReadAllText(configFileName);

                    _instance = JsonConvert.DeserializeObject<ManagerInfo>(json);
                }
                else
                    _instance = new ManagerInfo();

                if (_instance.installDir == null || !Directory.Exists(_instance.installDir))
                {

                    _instance.installDir = GameInstallFinder.FindGameInstallDirectory();

                    if (_instance.installDir == null)
                    {
                        System.Windows.Application.Current.Shutdown();
                        return null;
                    }
                }

                if (_instance.DownloadDirectory == null)
                    _instance.DownloadDirectory = "Downloads";

                try
                {
                    File.WriteAllText(configFileName, JsonConvert.SerializeObject(_instance));
                }
                catch (IOException e)
                {
                    System.Windows.MessageBox.Show("config write error " + e.ToString());
                }

                _instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _instance.DownloadDirectory);
            }

            return _instance;
        }

    }
}
