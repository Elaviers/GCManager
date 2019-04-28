using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCManager
{
    class ModListDownloaded : ModList
    {
        public override void GetMods()
        {
            collection.Clear();

            foreach (string dir in Directory.GetDirectories(ManagerInfo.Get().GetFullDownloadDirectory()))
            {
                string json = File.ReadAllText(Path.Combine(dir, "manifest.json"));

                LocalManifest manifest = JsonConvert.DeserializeObject<LocalManifest>(json);

                collection.Add(new Mod(manifest, dir));
            }
        }
    }
}
