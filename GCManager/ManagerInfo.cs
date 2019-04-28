using System;
using System.IO;

using Newtonsoft.Json;

namespace GCManager
{
    public class ManagerInfo
    {
        private static ManagerInfo _instance = null;

        public string installDir = null;
        public string DownloadDirectory = null;

        public static string GetConfigFileName()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        }

        public string GetFullDownloadDirectory()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.DownloadDirectory);
        }

        public static ManagerInfo Get()
        {
            if (_instance == null)
            {
                string configFileName = GetConfigFileName();

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

                _instance.Save();
            }

            if (!Directory.Exists(_instance.GetFullDownloadDirectory()))
                Directory.CreateDirectory(_instance.GetFullDownloadDirectory());

            return _instance;
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(GetConfigFileName(), JsonConvert.SerializeObject(_instance));
            }
            catch (IOException e)
            {
                System.Windows.MessageBox.Show("config write error " + e.ToString());
            }

        }

    }
}
