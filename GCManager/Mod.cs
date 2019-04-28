using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace GCManager
{
    public class Mod : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private string _name, _author, _description;
        private Uri _imageLink;
        private bool _isInstalled;

        public string name { get { return _name; } set { _name = value; NotifyPropertyChanged("name"); } }
        public string fullName { get; set; }
        public string author { get { return _author; } set { _author = value; NotifyPropertyChanged("author"); } }
        public string description { get { return _description; } set { _description = value; NotifyPropertyChanged("description"); } }
        public string version { get; set; }

        public int downloadCount { get; set; }

        public Uri modLink { get; set; }
        public Uri authorLink { get; set; }
        public Uri downloadLink { get; set; }

        public Uri imageLink { get { return _imageLink; } set { _imageLink = value; NotifyPropertyChanged("imageLink"); } }

        public string[] dependencies { get; set; }

        //
        public string notes { get; set; }
        public bool isInstalled { get { return _isInstalled; } set { _isInstalled = value; NotifyPropertyChanged("isInstalled"); } }

        public bool CheckIfInstalled()
        {
            return this.fullName == "bbepis-BepInExPack" ?
                Directory.Exists(Path.Combine(ManagerInfo.Get().installDir, "BepInEx")) :
                Directory.Exists(Path.Combine(ManagerInfo.Get().installDir, "BepInEx", "plugins", this.fullName));
        }

        public static void CheckIfModsInstalled(ObservableCollection<Mod> list)
        {
            foreach (Mod mod in list)
            {
                mod.isInstalled = mod.CheckIfInstalled();
            }
        }
    }
}
