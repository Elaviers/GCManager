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
    public class Profile
    {
        public class ProfileEntry
        {
            public string fullName;
            public string version;

            public ProfileEntry()
            {

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
                    MessageBox.Show("Could not find mod \"" + entry.fullName + "\"!", "Profile Load Error", MessageBoxButton.OK);
            }
        }

        public void Add(Mod localMod)
        {
            entries.Add(new ProfileEntry(localMod));
        }

        public void Save()
        {
            File.WriteAllText(Path.Combine(ManagerInfo.Get().GetFullProfileDirectory(), (name + ".json")), JsonConvert.SerializeObject(this));
        }
    }
}
