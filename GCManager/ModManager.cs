using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

//Warning: HUGE MESS BELOW!
//Scroll down at your own risk. I am not liable for any brain damage caused due to the reading of this file's code.

namespace GCManager
{
    class ModManager
    {
        private static readonly int MAX_CONCURRENT_DOWNLOADS = 1;

        public static ModList onlineModList = null;

        public static bool silent = false;

        class DownloadInfo
        {
            public Mod mod;
            public string version;
        }

        public static Mod selectedModInfo = new Mod();

        public static List<EntryInfo> jobs = new List<EntryInfo>();

        private static Queue<DownloadInfo> _downloadQueue = new Queue<DownloadInfo>();


        private static int _downloadsInProgress = 0;

        private static List<string> _priorityInstalls = new List<string>();
        private static Queue<Mod> _pendingInstalls = new Queue<Mod>();

        public static void UpdateInstalledStatuses()
        {
            App app = (App)Application.Current;
            if (app.window != null)
            {
                app.window.OnlineMods.modList.UpdateModInstalledStatus();
                app.window.DownloadedMods.modList.UpdateModInstalledStatus();
            }
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

        public static void ActivateMod(Mod mod, string version = null)
        {
            string downloadDir = mod.GetDownloadDirectory();

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
                    LocalManifest manifest = JsonConvert.DeserializeObject<LocalManifest>(File.ReadAllText(manifestPath));

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

                string path = mod.GetDownloadDirectory();

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

                if (onlineModList != null)
                {
                    Mod dependencyMod = onlineModList.Find(dependencyFullName);

                    _priorityInstalls.Add(dependencyFullName);

                    if (dependencyMod == null)
                    {
                        MessageBox.Show("Error: Somehow, the dependency \"" + dependency + "\" could not be found in the online mod list.\nHmm... maybe you could try refreshing the online mod list?", "Uh oh", MessageBoxButton.OK);
                    }
                    else if (!dependencyMod.CheckIfInstalled())
                    {
                        if (!silent)
                        {
                            MessageBoxResult result = MessageBox.Show(
                                "This mod requires the dependency \"" + dependency + "\".\nDo you want to install this? (You probably should!)",
                                "You must install additional mods", MessageBoxButton.YesNo);

                            if (result == MessageBoxResult.Yes || silent)
                                ActivateMod(dependencyMod);
                        }
                    }

                    _priorityInstalls.Remove(dependencyFullName);
                }
                else
                    MessageBox.Show("The dependency mod list is not set", "How did this even hapen?!", MessageBoxButton.OK);
            }

            mod.Install();
            GetEntryInfo(mod).status = EntryStatus.INSTALLED;

            if (_pendingInstalls.Count > 0)
            {
                InstallMod(_pendingInstalls.Dequeue());
            }

            UpdateInstalledStatuses();
        }

        public static void UninstallMod(Mod mod)
        {
            mod.Uninstall();
            GetEntryInfo(mod).status = EntryStatus.UNINSTALLED;

            UpdateInstalledStatuses();
        }

        public static void UpdateMod(Mod localMod)
        {
            Mod onlineMod = onlineModList.Find(localMod.fullName);

            if (onlineMod != null && new Version(localMod.version) < new Version(onlineMod.version))
            {
                if (localMod.fullName != "bbepis-BepInExPack")
                    UninstallMod(localMod);

                Directory.Delete(localMod.GetDownloadDirectory(), true);

                ActivateMod(onlineMod);
            }
        }
    }
}
