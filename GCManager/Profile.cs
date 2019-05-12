using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace GCManager
{
    public class Profile
    {
        public class ProfileEntry
        {
            public string fullName;
            public string version;

            public ProfileEntry()
            { }

            public ProfileEntry(string fullName, string version)
            {
                this.fullName = fullName;
                this.version = version;
            }

            public ProfileEntry(Mod mod)
            {
                this.fullName = mod.fullName;
                this.version = mod.version;
            }
        }

        public string name;

        public List<ProfileEntry> entries = new List<ProfileEntry>();

        public void Install(ModList onlineMods, bool installProfileVersions)
        {
            foreach (ProfileEntry entry in entries)
            {
                Mod mod = onlineMods.Find(entry.fullName);

                if (mod != null)
                {
                    ModManager.ActivateMod(mod, installProfileVersions ? entry.version : null);
                }
                else
                    MessageBox.Show($"Could not find mod \"{entry.fullName}\"!", "Profile Load Error", MessageBoxButton.OK);
            }
        }

        public void Add(Mod localMod)
        {
            entries.Add(new ProfileEntry(localMod));
        }

        public static Profile Load(string json, string name = null)
        {
            var content = JToken.Parse(json);

            if (content != null)
            {
                if (content is JArray)
                {
                    var profile = new Profile();

                    if (name == null)
                    {

                        ProfileCreateWindow dialog = new ProfileCreateWindow();
                        if (dialog.ShowDialog() == true)
                            profile.name = dialog.profileName;
                        else
                            return null;
                    }
                    else
                        profile.name = name;

                    JArray array = content as JArray;

                    foreach (string package in array)
                    {
                        int versionHyphenIndex = package.LastIndexOf('-');

                        string fullName = package.Substring(0, versionHyphenIndex);
                        string version = package.Substring(versionHyphenIndex + 1);

                        profile.entries.Add(new ProfileEntry(fullName, version));
                    }

                    return profile;
                }
                else if (content is JObject)
                {
                    return content.ToObject<Profile>();
                }
            }

            return null;
        }

        public string GetJSON()
        {
            List<string> packages = new List<string>();

            foreach (ProfileEntry entry in entries)
            {
                packages.Add($"{entry.fullName}-{entry.version}");
            }

            return JsonConvert.SerializeObject(packages);
        }

        public void Save()
        {
            File.WriteAllText(Path.Combine(ManagerInfo.Get().GetFullProfileDirectory(), (name + ".json")), GetJSON());
        }
    }
}
