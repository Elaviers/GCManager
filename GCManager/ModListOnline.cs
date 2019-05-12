using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace GCManager
{
    class ModListOnline : ModList
    {
        public override void RefreshCollection()
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

                foreach (OnlineManifest manifest in entries)
                {
                    Mod mod = new Mod(manifest);

                    if (manifest.is_pinned)
                        collection.Insert(FirstUnpinnedIndex++, mod);
                    else
                        collection.Add(mod);
                }
            }

            response.Close();
        }
    }
}
