using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCManager
{
    public abstract class ModList
    {
        public ObservableCollection<Mod> collection = new ObservableCollection<Mod>();

        public abstract void GetMods();

        public void UpdateModInstalledStatus()
        {
            foreach (Mod mod in collection)
                mod.isInstalled = mod.CheckIfInstalled();
        }

        public Mod Find(string fullName)
        {
            foreach (Mod mod in collection)            
                if (mod.fullName == fullName)
                    return mod;

            return null;
        }
    }
}
