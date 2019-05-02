using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GCManager
{
    class ModListDownloaded : ModList
    {
        public override void GetMods()
        {
            collection.Clear();

            foreach (string dir in Directory.GetDirectories(ManagerInfo.Get().GetFullDownloadDirectory()))
            {
                try
                {
                    string json = File.ReadAllText(Path.Combine(dir, "manifest.json"));

                    LocalManifest manifest = JsonConvert.DeserializeObject<LocalManifest>(json);

                    collection.Add(new Mod(manifest, dir));
                }
                catch (IOException ex)
                {
                    var result = MessageBox.Show(string.Format("Exception reading manifest for \"{0}\":\n{1}\nDo you want to re-download this mod?", 
                        new DirectoryInfo(dir).Name, 
                        ex.Message), "What's all this", MessageBoxButton.YesNo);

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
