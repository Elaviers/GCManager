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

        public static string GetPluginPath(string fullName)
        {
            return Path.Combine(ManagerInfo.Get().installDir, "BepInEx", "plugins", fullName);
        }

        public static string GetMonoModPath(string fullName)
        {
            return Path.Combine(ManagerInfo.Get().installDir, "BepInEx", "monomod", fullName);
        }

        public void Install()
        {
            if (this.fullName == "bbepis-BepInExPack") //Special case
            {
                Utility.CopyDirectory(Path.Combine(GetDownloadDirectory(), "BepInExPack"), ManagerInfo.Get().installDir);
            }
            else
            {
                List<string> dirs = Utility.FindAllDirectories(GetDownloadDirectory());
                dirs.Add(GetDownloadDirectory());

                foreach (string dir in dirs)
                {
                    string destDir;

                    if (new DirectoryInfo(dir).Name.ToLower() == "monomod")
                        destDir = GetMonoModPath("");
                    else
                        destDir = GetPluginPath("");

                    string[] dlls = Directory.GetFiles(dir, "*.dll");

                    if (dlls.Length > 0)
                    {
                        destDir = Path.Combine(destDir, this.fullName);
                        Directory.CreateDirectory(destDir);

                        foreach (string filepath in dlls)
                        {
                            string dest = Path.Combine(destDir, Path.GetFileName(filepath));

                            if (!File.Exists(dest))
                                File.Copy(filepath, dest, true);
                        }
                    }
                }
            }

            this.isInstalled = true;
        }

        public void Uninstall()
        {
            Uninstall(this.fullName);

            this.isInstalled = false;
        }

        public static void Uninstall(string name, bool silent = false)
        {
            try
            {
                if (name == "bbepis-BepInExPack")
                {
                    if (!silent)
                    {
                        MessageBoxResult result = MessageBox.Show("Uninstalling BepInEx will also uninstall all of your mods!\nYou sure about this?", "Warning", MessageBoxButton.YesNo);
                        if (result != MessageBoxResult.Yes)
                            return;
                    }

                    string installDir = ManagerInfo.Get().installDir;

                    foreach (string dir in Directory.GetDirectories(Path.Combine(installDir, "BepInEx")))
                    {
                        if (new DirectoryInfo(dir).Name != "config")
                            Directory.Delete(dir, true);
                    }

                    File.Delete(Path.Combine(installDir, "winhttp.dll"));
                    File.Delete(Path.Combine(installDir, "doorstop_config.ini"));
                }
                else
                {
                    if (Directory.Exists(GetMonoModPath(name)))
                        Directory.Delete(GetMonoModPath(name), true);

                    if (Directory.Exists(GetPluginPath(name)))
                        Directory.Delete(GetPluginPath(name), true);
                }
            }
            catch (IOException) { }
        }

        public bool CheckIfInstalled()
        {
            return this.fullName == "bbepis-BepInExPack" ?
                Directory.Exists(Path.Combine(ManagerInfo.Get().installDir, "BepInEx", "core")) :
                Directory.Exists(GetMonoModPath(this.fullName)) || Directory.Exists(GetPluginPath(this.fullName)); 
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

        public Mod(LocalManifest mf, string downloadDir)
        {
            this.name = mf.name;
            this.fullName = new DirectoryInfo(downloadDir).Name;
            this.author = fullName.Substring(0, fullName.LastIndexOf('-'));

            this.description = mf.description;
            this.version = mf.version_number;

            if (mf.website_url.Length > 0)
                this.modLink = new Uri(mf.website_url);

            this.authorLink = new Uri("https://thunderstore.io/package/" + this.author);
            this.imageLink = new Uri(Path.Combine(downloadDir, "icon.png"));

            if (mf.dependencies != null)
                this.dependencies = mf.dependencies.ToArray();
            else
                this.dependencies = null;

            this.isInstalled = CheckIfInstalled();
        }
    }
}
