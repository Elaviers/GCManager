using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace GCManager
{
    public class Mod : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private string _name, _author, _description;
        private Uri _imageLink;
        private bool _isInstalled;

        public string name { get { return _name; } set { _name = value; NotifyPropertyChanged("name"); } }
        public string fullName { get; set; }
        public string author { get { return _author; } set { _author = value; NotifyPropertyChanged("author"); } }
        public string description { get { return _description; } set { _description = value; NotifyPropertyChanged("description"); } }
        public string version { get; set; }

        public int downloadCount { get; set; }

        public Uri modLink { get; set; }
        public Uri authorLink { get; set; }

        public Uri imageLink { get { return _imageLink; } set { _imageLink = value; NotifyPropertyChanged("imageLink"); } }

        public string[] dependencies { get; set; }

        //
        public string notes { get; set; }
        public bool isInstalled { get { return _isInstalled; } set { _isInstalled = value; NotifyPropertyChanged("isInstalled"); } }

        public string GetDownloadDirectory()
        {
            return Path.Combine(ManagerInfo.Get().GetFullDownloadDirectory(), this.fullName);
        }

        public string GetInstallDirectory()
        {
            return Path.Combine(ManagerInfo.Get().installDir, "BepInEx", "plugins", this.fullName);
        }

        public void Install()
        {
            if (this.fullName == "bbepis-BepInExPack") //Special case
            {
                Utility.CopyDirectory(Path.Combine(GetDownloadDirectory(), "BepInExPack"), ManagerInfo.Get().installDir);
            }
            else
            {
                List<string> dlls = Utility.FindAllFiles(GetDownloadDirectory(), "*.dll");

                if (dlls.Count > 0)
                {
                    string dir = GetInstallDirectory();

                    Directory.CreateDirectory(dir);

                    foreach (string filepath in dlls)
                    {
                        string dest = Path.Combine(dir, Path.GetFileName(filepath));

                        if (!File.Exists(dest))
                            File.Copy(filepath, dest, true);
                    }
                }
            }

            this.isInstalled = true;
        }

        public void Uninstall()
        {
            try
            {
                if (this.fullName == "bbepis-BepInExPack")
                {
                    MessageBoxResult result = MessageBox.Show("Uninstalling BepInEx will also uninstall all of your mods!\nYou sure about this?", "Warning", MessageBoxButton.YesNo);
                    if (result != MessageBoxResult.Yes)
                        return;

                    string installDir = ManagerInfo.Get().installDir;

                    Directory.Delete(System.IO.Path.Combine(installDir, "BepInEx"), true);
                    File.Delete(System.IO.Path.Combine(installDir, "winhttp.dll"));
                    File.Delete(System.IO.Path.Combine(installDir, "doorstop_config.ini"));

                    //Refresh lists
                    App app = (App)Application.Current;
                    if (app.window != null)
                    {
                        app.window.OnlineMods.modList.UpdateModInstalledStatus();
                        app.window.DownloadedMods.modList.UpdateModInstalledStatus();
                    }
                }
                else
                {
                    Directory.Delete(GetInstallDirectory(), true);
                }
            }
            catch (IOException) { }

            this.isInstalled = false;
        }

        public bool CheckIfInstalled()
        {
            return this.fullName == "bbepis-BepInExPack" ?
                Directory.Exists(Path.Combine(ManagerInfo.Get().installDir, "BepInEx")) :
                Directory.Exists(GetInstallDirectory());
        }

        public Mod()
        {

        }

        public Mod(OnlineManifest mf)
        {
            this.name = mf.name;
            this.fullName = mf.full_name;
            this.author = mf.owner;

            OnlineManifest.VersionManifest latestVersion = mf.versions[0];

            this.description = latestVersion.description;
            this.version = latestVersion.version_number;

            this.downloadCount = 0;
            foreach (var version in mf.versions)
                this.downloadCount += version.downloads;

            this.modLink = new Uri(mf.package_url);
            this.authorLink = new Uri("https://thunderstore.io/package/" + mf.owner);
            this.imageLink = new Uri(latestVersion.icon);

            this.dependencies = latestVersion.dependencies;

            this.notes = mf.is_pinned ? "Pinned" : "";

            this.isInstalled = CheckIfInstalled();
        }

        public Mod(LocalManifest mf, string localDir)
        {
            this.name = mf.name;
            this.fullName = localDir;
            this.author = fullName.Substring(0, fullName.LastIndexOf('-'));

            this.description = mf.description;
            this.version = mf.version_number;

            if (mf.website_url.Length > 0)
                this.modLink = new Uri(mf.website_url);

            this.authorLink = new Uri("https://thunderstore.io/package/" + this.author);
            this.imageLink = new Uri(Path.Combine(localDir, "icon.png"));

            this.dependencies = mf.dependencies.ToArray();

            this.isInstalled = CheckIfInstalled();
        }
    }
}
