using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GCManager
{
    class ModListOnline : ModList
    {
        public override void GetMods()
        {
            collection.Clear();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            WebRequest modsPlease = WebRequest.Create("https://thunderstore.io/api/v1/package/");

            WebResponse response = modsPlease.GetResponse();

            if (response.ContentLength > 0)
            {
                string data = new StreamReader(response.GetResponseStream()).ReadToEnd();

                OnlineManifest[] entries = JsonConvert.DeserializeObject<OnlineManifest[]>(data);
                int FirstUnpinnedIndex = 0;

                foreach (OnlineManifest mf in entries)
                {
                    Mod mod = new Mod();
                    mod.name = mf.name;
                    mod.fullName = mf.full_name;
                    mod.author = mf.owner;
                    mod.authorLink = new Uri("https://thunderstore.io/package/" + mod.author);
                    mod.modLink = new Uri(mf.package_url);

                    mod.version = mf.versions[0].version_number;
                    mod.description = mf.versions[0].description;
                    mod.dependencies = mf.versions[0].dependencies.ToArray();
                    mod.imageLink = new Uri(mf.versions[0].icon);

                    mod.isInstalled = mod.CheckIfInstalled();

                    mod.downloadCount = 0;
                    for (int i = 0; i < mf.versions.Length; i++)
                        mod.downloadCount += mf.versions[i].downloads;

                    if (mf.is_pinned)
                    {
                        mod.notes = "Pinned";
                        collection.Insert(FirstUnpinnedIndex++, mod);
                    }
                    else
                        collection.Add(mod);
                }
            }

            response.Close();
        }
    }
}
