using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

namespace GCManager
{
    class ModManager
    {
        private static readonly int MAX_CONCURRENT_DOWNLOADS = 1;

        public static ModList onlineModList = null;
        public static ModList downloadedModList = null;

        public static bool silent = false;

        public static Mod selectedModInfo = new Mod();

        private static Queue<Mod> _downloadQueue = new Queue<Mod>();


        private static int _downloadsInProgress = 0;

        private static List<string> _priorityInstalls = new List<string>();
        private static Queue<Mod> _installQueue = new Queue<Mod>();

        public static event Action<Mod, float> ModDownloadProgressUpdate;
        public static event Action<Mod> ModExtractStarted;
        public static event Action<Mod> ModExtractFinished;
        public static event Action<Mod> ModInstallStarted;
        public static event Action<Mod> ModInstallFinished;
        public static event Action<Mod> ModUninstallFinished;
        public static event Action<Mod> LocalModDeletionImminent;

        public static void UpdateInstalledStatuses()
        {
            onlineModList?.UpdateModInstalledStatus();
            downloadedModList?.UpdateModInstalledStatus();
        }

        public static void ActivateMod(Mod mod, string version = null)
        {
            string downloadDir = mod.GetDownloadDirectory();

            if (!mod.IsDownloaded())
            {
                EnQueueModDownload(mod, version ?? mod.version);
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
                        mod.installOnDownloaded = true;
                        EnQueueModDownload(mod, version);
                    }
                    else
                    {
                        InstallMod(mod, version);
                    }

                    return;
                }
                else
                    MessageBox.Show($"\"{mod.fullName}\" is downloaded, but doesn't have a manifest!\nThis should never happen, good job.", "Oh No!", MessageBoxButton.OK);
            }

            InstallMod(mod, mod.version);
        }

        public static void EnQueueModDownload(Mod mod, string version)
        {
            Mod downloadMod = new Mod(mod);
            mod.version = version;

            if (_downloadsInProgress < MAX_CONCURRENT_DOWNLOADS)
                _DownloadMod(downloadMod);
            else
                _downloadQueue.Enqueue(downloadMod);
        }

        private static void _DownloadMod(Mod mod)
        {
            WebClient client = new WebClient();

            client.DownloadProgressChanged += DownloadProgressUpdate;
            client.DownloadDataCompleted += _DownloadComplete;

            _downloadsInProgress++;

            client.DownloadDataAsync(new Uri($"https://thunderstore.io/package/download/{mod.author}/{mod.name}/{mod.version}/"), mod);
        }

        private static void _DownloadComplete(object sender, DownloadDataCompletedEventArgs args)
        {
            _downloadsInProgress--;

            if (_downloadQueue.Count > 0)
            {
                _DownloadMod(_downloadQueue.Dequeue());
            }

            UnzipMod((Mod)args.UserState, args.Result);
        }

        private static void DownloadProgressUpdate(object sender, DownloadProgressChangedEventArgs args)
        {
            Mod mod = (Mod)args.UserState;

            ModDownloadProgressUpdate(mod, args.ProgressPercentage);
        }

        public static void UnzipMod(Mod mod, byte[] zipData)
        {
            ModExtractStarted(mod);

            if (mod != null && zipData.Length > 0)
            {
                ZipArchive zip = new ZipArchive(new MemoryStream(zipData));

                string path = mod.GetDownloadDirectory();

                if (Directory.Exists(path))
                    Directory.Delete(path, true);

                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    string entryPath = Path.Combine(path, entry.FullName);

                    Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                    if (Path.GetExtension(entryPath).Length != 0)
                    {
                        Stream input = entry.Open();
                        FileStream output = File.OpenWrite(entryPath);

                        input.CopyTo(output);

                        input.Close();
                        output.Close();
                    }
                }

                zip.Dispose();

                downloadedModList?.RefreshCollection();

                ModExtractFinished(mod);

                if (mod.installOnDownloaded)
                    InstallMod(mod);
                else
                    mod.isInstalled = mod.CheckIfInstalled();
            }
        }

        public static void InstallMod(Mod mod, string version = null)
        {
            ModInstallStarted(mod);

            if (_priorityInstalls.Count > 0 && !_priorityInstalls.Contains(mod.fullName))
            {
                _installQueue.Enqueue(mod);
                return;
            }

            foreach (string dependency in mod.dependencies)
            {
                string[] tokens = dependency.Split('-');
                string dependencyFullName = tokens[0] + '-' + tokens[1];

                if (onlineModList != null)
                {
                    Mod dependencyMod = onlineModList.Find(dependencyFullName);

                    if (dependencyMod == null)
                    {
                        MessageBox.Show($"Error: Somehow, the dependency \"{dependency}\" could not be found in the online mod list.\nHmm... maybe you could try refreshing the online mod list?", "Uh oh", MessageBoxButton.OK);
                    }
                    else if (!dependencyMod.CheckIfInstalled())
                    {
                        MessageBoxResult result = MessageBoxResult.Yes;

                        _priorityInstalls.Add(dependencyFullName);

                        if (!silent)
                        {
                            result = MessageBox.Show(
                                $"This mod depends on \"{dependency}\".\nDo you want to install this first?",
                                "You must install additional mods", MessageBoxButton.YesNo);
                        }

                        if (result == MessageBoxResult.Yes)
                        {
                            ActivateMod(dependencyMod);
                            _installQueue.Enqueue(mod);
                            return;
                        }
                        else
                        {
                            _priorityInstalls.Remove(dependencyFullName);
                        }
                    }
                }
            }

            mod.Install();

            ModInstallFinished(mod);

            _priorityInstalls.Remove(mod.fullName);

            if (_installQueue.Count > 0)
            {
                InstallMod(_installQueue.Dequeue());
            }

            UpdateInstalledStatuses();
        }

        public static void UninstallMod(Mod mod)
        {
            mod.Uninstall(silent);
            ModUninstallFinished(mod);

            UpdateInstalledStatuses();
        }

        public static void UpdateMod(Mod localMod)
        {
            Mod onlineMod = onlineModList.Find(localMod.fullName);

            if (onlineMod != null && new Version(localMod.version) < new Version(onlineMod.version))
            {
                if (localMod.fullName != "bbepis-BepInExPack")
                    UninstallMod(localMod);

                LocalModDeletionImminent(localMod);

                try
                {
                    Directory.Delete(localMod.GetDownloadDirectory(), true);
                }
                catch (IOException) { }

                ActivateMod(onlineMod);
            }
        }
    }
}
