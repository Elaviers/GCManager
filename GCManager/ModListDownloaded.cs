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

                Mod mod = new Mod();
                mod.name = manifest.name;
                mod.fullName = new DirectoryInfo(dir).Name;
                mod.author = mod.fullName.Substring(0, mod.fullName.LastIndexOf('-'));
                mod.authorLink = new Uri("https://thunderstore.io/package/" + mod.author);
                mod.description = manifest.description;
                mod.version = manifest.version_number;

                if (mod.dependencies != null)
                    mod.dependencies = manifest.dependencies.ToArray();

                if (manifest.website_url.Length > 0)
                    mod.modLink = new Uri(manifest.website_url);

                mod.isInstalled = mod.CheckIfInstalled();

                collection.Add(mod);
            }
        }
    }
}
