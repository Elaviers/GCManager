using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

//Warning: HUGE MESS BELOW!
//Scroll down at your own risk. I am not liable for any brain damage caused due to the reading of this file's code.

namespace GCManager
{
    class ModManager
    {
        private static readonly int MAX_CONCURRENT_DOWNLOADS = 1;

        class DownloadInfo
        {
            public Mod mod;
            public string version;
        }

        class Manifest
        {
            public string name, description, website_url, version_number;

            public List<String> dependencies;
        }

        public static ObservableCollection<Mod> downloadedMods = new ObservableCollection<Mod>();
        public static ObservableCollection<Mod> onlineMods = new ObservableCollection<Mod>();

        public static Mod selectedModInfo = new Mod();

        public static List<EntryInfo> jobs = new List<EntryInfo>();

        private static Queue<DownloadInfo> _downloadQueue = new Queue<DownloadInfo>();


        private static int _downloadsInProgress = 0;

        private static List<string> _priorityInstalls = new List<string>();
        private static Queue<Mod> _pendingInstalls = new Queue<Mod>();

        public static Mod FindMod(ObservableCollection<Mod> list, string fullName)
        {
            foreach (Mod mod in list)
            {
                if (mod.fullName == fullName)
                    return mod;
            }

            return null;
        }

        private static EntryInfo GetEntryInfo(Mod mod)
        {
            foreach (EntryInfo info in jobs)
            {
                if (info.fullName == mod.fullName)
                {
                    System.Windows.Controls.ItemCollection items = ((App)Application.Current).JobListItems;

                    if (((JobEntry)items.GetItemAt(0)).entryInfo.fullName != mod.fullName)
                    {
                        foreach (JobEntry item in items)
                        {
                            if (item.entryInfo.fullName == mod.fullName)
                            {
                                items.Remove(item);
                                break;
                            }
                        }

                        items.Insert(0, new JobEntry(info));
                    }

                    return info;
                }
            }

            EntryInfo newInfo = new EntryInfo();
            newInfo.fullName = mod.fullName;
            newInfo.imageLink = mod.imageLink;
            newInfo.progress = 100;

            jobs.Insert(0, newInfo);
            ((App)Application.Current).JobListItems.Insert(0, new JobEntry(newInfo));

            return newInfo;
        }

        public static void QueryDownloadedMods()
        {
            downloadedMods.Clear();

            foreach (string dir in Directory.GetDirectories(ManagerInfo.Get().DownloadDirectory))
            {
                string json = File.ReadAllText(Path.Combine(dir, "manifest.json"));

                Manifest manifest = JsonConvert.DeserializeObject<Manifest>(json);

                Mod mod = new Mod();
                mod.name = manifest.name;
                mod.fullName = new DirectoryInfo(dir).Name;
                mod.author = mod.fullName.Substring(0, mod.fullName.LastIndexOf('-'));
                mod.authorLink = new Uri("https://thunderstore.io/package/" + mod.author);
                mod.description = manifest.description;
                mod.version = manifest.version_number;
                mod.dependencies = manifest.dependencies.ToArray();

                if (manifest.website_url.Length > 0)
                    mod.modLink = new Uri(manifest.website_url);

                mod.isInstalled = mod.CheckIfInstalled();

                downloadedMods.Add(mod);
            }
        }

        public static void QueryOnlineMods()
        {
            onlineMods.Clear();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            WebRequest modsPlease = WebRequest.Create("https://thunderstore.io/api/v1/package/");

            WebResponse response = modsPlease.GetResponse();

            if (response.ContentLength > 0)
            {
                string data = new StreamReader(response.GetResponseStream()).ReadToEnd();

                var def = new[]
                {
                    new {
                        name = "",
                        full_name = "",
                        owner = "",
                        package_url = "",
                        is_pinned = false,
                        versions = new[]
                        {
                            new {
                                name = "",
                                full_name = "",
                                description = "",
                                icon = "",
                                version_number = "",
                                dependencies = new List<String>(),
                                download_url = "",
                                downloads = 0,
                                date_created = "",
                                website_url = ""
                            }
                        }
                    }
                };

                var entries = JsonConvert.DeserializeAnonymousType(data, def);
                int FirstUnpinnedIndex = 0;

                foreach (var entry in entries)
                {
                    Mod mod = new Mod();
                    mod.name = entry.name;
                    mod.fullName = entry.full_name;
                    mod.author = entry.owner;
                    mod.authorLink = new Uri("https://thunderstore.io/package/" + mod.author);
                    mod.modLink = new Uri(entry.package_url);

                    mod.version = entry.versions[0].version_number;
                    mod.description = entry.versions[0].description;
                    mod.dependencies = entry.versions[0].dependencies.ToArray();
                    mod.imageLink = new Uri(entry.versions[0].icon);

                    mod.isInstalled = mod.CheckIfInstalled();

                    mod.downloadCount = 0;
                    for (int i = 0; i < entry.versions.Length; i++)
                        mod.downloadCount += entry.versions[i].downloads;

                    if (entry.is_pinned)
                    {
                        mod.notes = "Pinned";
                        onlineMods.Insert(FirstUnpinnedIndex++, mod);
                    }
                    else
                        onlineMods.Add(mod);
                }
            }

            response.Close();
        }

        public static void ActivateMod(Mod mod, string version = null)
        {
            string downloadDir = Path.Combine(ManagerInfo.Get().DownloadDirectory, mod.fullName);

            if (!Directory.Exists(downloadDir))
            {
                QueueModDownload(mod, version ?? mod.version);
                return;
            }


            if (version != null)
            {
                string manifestPath = Path.Combine(downloadDir, "manifest.json");

                if (File.Exists(manifestPath))
                {
                    Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifestPath));

                    if (version != manifest.version_number)
                    {
                        Directory.Delete(downloadDir, true);
                        QueueModDownload(mod, version);
                    }
                    else
                    {
                        InstallMod(mod, version);
                    }

                    return;
                }
                else
                    MessageBox.Show(String.Format("\"{0}\" has a directory, but doesn't have a manifest!\nWhat's the deal with that?", mod.fullName), "Oh No!", MessageBoxButton.OK);
            }

            InstallMod(mod, mod.version);
        }

        public static void QueueModDownload(Mod mod, string version)
        {
            if (_downloadsInProgress < MAX_CONCURRENT_DOWNLOADS)
                _DownloadModAndInstall(mod, version);
            else
            {
                DownloadInfo info = new DownloadInfo();
                info.mod = mod;
                info.version = version;

                _downloadQueue.Enqueue(info);
            }
        }

        public static void _DownloadModAndInstall(Mod mod, string version)
        {
            EntryInfo info = GetEntryInfo(mod);
            info.version = version;
            info.progress = 0;
            info.status = EntryStatus.DOWNLOADING;

            WebClient client = new WebClient();

            client.DownloadProgressChanged += DownloadProgressUpdate;
            client.DownloadDataCompleted += DownloadComplete;

            string modVersion = version ?? mod.version;

            _downloadsInProgress++;

            client.DownloadDataAsync(new Uri(String.Format("https://thunderstore.io/package/download/{0}/{1}/{2}/", mod.author, mod.name, version)), mod);
        }

        private static void DownloadComplete(object sender, DownloadDataCompletedEventArgs args)
        {
            _downloadsInProgress--;
            
            if (_downloadQueue.Count > 0)
            {
                DownloadInfo info = _downloadQueue.Dequeue();
                _DownloadModAndInstall(info.mod, info.version);
            }

            UnzipModAndInstall((Mod)args.UserState, args.Result);
        }

        public static void UnzipModAndInstall(Mod mod, byte[] zipData)
        {
            GetEntryInfo(mod).status = EntryStatus.EXTRACTING;

            if (mod != null && zipData.Length > 0)
            {
                ZipArchive zip = new ZipArchive(new MemoryStream(zipData));

                string path = Path.Combine(ManagerInfo.Get().DownloadDirectory, mod.fullName);

                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    string entryPath = Path.Combine(path, entry.FullName);

                    Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                    if (Path.GetExtension(entryPath).Length != 0)
                    {
                        Stream input = entry.Open();
                        FileStream output = File.Create(entryPath);

                        input.CopyTo(output);

                        input.Close();
                        output.Close();
                    }
                }

                zip.Dispose();

                InstallMod(mod);
            }
        }

        private static void DownloadProgressUpdate(object sender, DownloadProgressChangedEventArgs args)
        {
            Mod mod = (Mod)args.UserState;

            var info = GetEntryInfo(mod);

            info.progress = args.ProgressPercentage;
        }

        public static void InstallMod(Mod mod, string version = null)
        {
            var info = GetEntryInfo(mod);
            info.status = EntryStatus.INSTALLING;
            if (version != null)
                info.version = version;


            if (_priorityInstalls.Count > 0 && !_priorityInstalls.Contains(mod.fullName))
            {
                _pendingInstalls.Enqueue(mod);
                return;
            }

            foreach (string dependency in mod.dependencies)
            {
                string[] tokens = dependency.Split('-');
                string dependencyFullName = tokens[0] + '-' + tokens[1];

                Mod dependencyMod = FindMod(onlineMods, dependencyFullName);

                _priorityInstalls.Add(dependencyFullName);

                if (dependencyMod == null)
                {
                    MessageBox.Show("Error: Somehow, the dependency \"" + dependency + "\" could not be found in the online mod list.\nHmm... maybe you could try refreshing the online mod list?", "Uh oh", MessageBoxButton.OK);
                }
                else if (!dependencyMod.CheckIfInstalled())
                {
                    MessageBoxResult result = MessageBox.Show(
                        "This mod requires the dependency \"" + dependency + "\".\nDo you want to install this? (You probably should!)", 
                        "You must install additional mods", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                        ActivateMod(dependencyMod);
                }

                _priorityInstalls.Remove(dependencyFullName);
            }

            if (mod.fullName == "bbepis-BepInExPack") //Special case
            {
                Utility.CopyDirectory(Path.Combine(ManagerInfo.Get().DownloadDirectory, mod.fullName, "BepInExPack"), ManagerInfo.Get().installDir);
            }
            else
            {
                List<string> dlls = Utility.FindAllFiles(Path.Combine(ManagerInfo.Get().DownloadDirectory, mod.fullName), "*.dll");

                if (dlls.Count > 0)
                {
                    string dir = Path.Combine(ManagerInfo.Get().installDir, "BepInEx", "plugins", mod.fullName);

                    Directory.CreateDirectory(dir);

                    foreach (string filepath in dlls)
                    {
                        string dest = Path.Combine(dir, Path.GetFileName(filepath));

                        if (!File.Exists(dest))
                            File.Copy(filepath, dest, true);
                    }
                }
            }

            mod.isInstalled = true;
            GetEntryInfo(mod).status = EntryStatus.INSTALLED;

            if (_pendingInstalls.Count > 0)
            {
                InstallMod(_pendingInstalls.Dequeue());
            }
        }

        public static void UninstallMod(Mod mod)
        {
            if (mod.fullName == "bbepis-BepInExPack") //Special case
            {
                MessageBoxResult result = MessageBox.Show("Uninstalling BepInExPack will also uninstall all of your mods!\nContinue?", "Wait a minute...", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    mod.isInstalled = true;
                    return;
                }

                string installDir = ManagerInfo.Get().installDir;

                try
                {
                    Directory.Delete(System.IO.Path.Combine(installDir, "BepInEx"), true);
                    File.Delete(System.IO.Path.Combine(installDir, "winhttp.dll"));
                    File.Delete(System.IO.Path.Combine(installDir, "doorstop_config.ini"));
                }
                catch (IOException) { }

                Mod.CheckIfModsInstalled(onlineMods);
                Mod.CheckIfModsInstalled(downloadedMods);
            }
            else
            {
                try
                {
                    Directory.Delete(System.IO.Path.Combine(ManagerInfo.Get().installDir, "BepInEx", "plugins", mod.fullName), true);
                }
                catch (DirectoryNotFoundException) { return; }
            }

            mod.isInstalled = false;
            GetEntryInfo(mod).status = EntryStatus.UNINSTALLED;
        }

        class Version
        {
            float major;
            float minor;
            float patch;

            public Version(String s)
            {
                string[] tokens = s.Split('.');
                if (tokens.Length >= 3)
                {
                    major = float.Parse(tokens[0]);
                    minor = float.Parse(tokens[1]);
                    patch = float.Parse(tokens[2]);
                }
            }

            public static bool operator <(Version a, Version b)
            {
                if (a.major < b.major)
                    return true;

                if (a.minor < b.minor)
                    return true;

                if (a.patch < b.patch)
                    return true;

                return false;
            }
            public static bool operator>(Version a, Version b)
            {
                if (a.major > b.major)
                    return true;

                if (a.major == b.major)
                {
                    if (a.minor > b.minor)
                        return true;

                    if (a.minor == b.minor && a.patch > b.patch)
                        return true;
                }

                return false;
            }
        }

        public static void UpdateMods()
        {
            foreach (Mod localMod in downloadedMods)
            {
                Mod onlineMod = FindMod(onlineMods, localMod.fullName);

                if (onlineMod != null && new Version(localMod.version) < new Version(onlineMod.version))
                {
                    if (localMod.fullName != "bbepis-BepInExPack")
                        UninstallMod(localMod);

                    Directory.Delete(Path.Combine(ManagerInfo.Get().DownloadDirectory, localMod.fullName), true);

                    ActivateMod(onlineMod);
                }
            }

        }
    }
}
