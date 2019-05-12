using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace GCManager
{
    class ModListDownloaded : ModList
    {
        public override void RefreshCollection()
        {
            collection.Clear();

            foreach (string dir in Directory.GetDirectories(ManagerInfo.Get().GetFullDownloadDirectory()))
            {
                string mfPath = Path.Combine(dir, "manifest.json");

                if (File.Exists(mfPath))
                {
                    try
                    {
                        string json = File.ReadAllText(mfPath);

                        LocalManifest manifest = JsonConvert.DeserializeObject<LocalManifest>(json);

                        collection.Add(new Mod(manifest, dir));
                    }
                    catch (IOException ex)
                    {
                        var result = MessageBox.Show($"Exception reading manifest for \"{new DirectoryInfo(dir).Name}\":\n{ex.Message}\nDo you want to re-download this mod?");

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                Directory.Delete(dir, true);

                                Mod mod = ModManager.onlineModList.Find(new DirectoryInfo(dir).Name);

                                mod.isInstalled = false;
                                ModManager.EnQueueModDownload(mod, mod.version);
                            }
                            catch (IOException ex2)
                            {
                                MessageBox.Show("Oh no! An exception was thrown when trying to delete the mod, too!\n" + ex2.Message);
                            }
                        }
                    }
                }
            }
        }
    }
}
