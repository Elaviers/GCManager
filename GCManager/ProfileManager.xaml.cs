using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GCManager
{

    public partial class ProfileManager : Window
    {
        public List<Profile> profileList = new List<Profile>();

        public ObservableCollection<string> profileNameList { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> modNameList { get; set; } = new ObservableCollection<string>();

        private Profile _currentProfile = null;

        public bool isClosed { get; private set; }

        public ProfileManager()
        {
            InitializeComponent();

            this.DataContext = this;

            RecreateLists();
        }

        private void NoProfileError()
        {
            MessageBox.Show("No profile selected!", "Nice try", MessageBoxButton.OK);
        }

        private void RecreateLists()
        {
            string[] profiles = Directory.GetFiles(ManagerInfo.Get().GetFullProfileDirectory(), "*.json");

            profileList.Clear();
            profileNameList.Clear();
            modNameList.Clear();

            foreach (string file in profiles)
            {
                profileNameList.Add(Path.GetFileNameWithoutExtension(file));

                profileList.Add(JsonConvert.DeserializeObject<Profile>(File.ReadAllText(file)));
            }
        }

        private void AddCurrentInstalledModsToProfile(Profile profile)
        {
            foreach (Mod mod in ((App)Application.Current).window.OnlineMods.modList.collection)
            {
                if (mod.isInstalled)
                {
                    profile.entries.Add(new Profile.ProfileEntry(mod));
                }
            }
        }

        private void CreateProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileCreateWindow dialog = new ProfileCreateWindow();

            if (dialog.ShowDialog() == true)
            {
                Profile newProfile = new Profile();
                newProfile.name = dialog.profileName;

                AddCurrentInstalledModsToProfile(newProfile);

                newProfile.Save();
                RecreateLists();
                SetCurrentProfile(newProfile);
            }
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProfile != null)
            {
                string path = Path.Combine(ManagerInfo.Get().GetFullProfileDirectory(), (_currentProfile.name + ".json"));
                File.Delete(path);
                _currentProfile = null;
                RecreateLists();
            }
        }

        private void CreateFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            string clipboardText = Clipboard.GetText();

            if (clipboardText.Length > 0)
            {
                Profile profile = null;

                try
                {
                    profile = JsonConvert.DeserializeObject<Profile>(Clipboard.GetText());
                }
                catch (JsonReaderException) {}

                if (profile == null || profile.name == null)
                    MessageBox.Show("Invalid clipboard data!", "That won't work", MessageBoxButton.OK);
                else
                {
                    profile.Save();
                    RecreateLists();
                    SetCurrentProfile(profile);
                }
            }
            else
                MessageBox.Show("You should probably have a json string in your clipboard before pressing that button again...", "Don't be silly!", MessageBoxButton.OK);
        }

        private void LoadProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProfile == null)
            {
                NoProfileError();
                return;
            }

            ProfileLoadWindow dialog = new ProfileLoadWindow();
            if (dialog.ShowDialog() == true)
            {
                if (!dialog.installOverExisting)
                    Mod.Uninstall("bbepis-BepInExPack", true);

                ModManager.UpdateInstalledStatuses();

                ModManager.silent = true;
                _currentProfile.Install(((App)Application.Current).window.OnlineMods.modList, dialog.installProfileVersions);
                ModManager.silent = false;
            }
        }

        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProfile == null)
                NoProfileError();
            else
            {
                _currentProfile.entries.Clear();
                AddCurrentInstalledModsToProfile(_currentProfile);
                _currentProfile.Save();
                RecreateLists();
            }
        }

        private void SetCurrentProfile(Profile profile)
        {
            for (int i = 0; i < profileList.Count; i++)
            {
                if (profileList[i].name == profile.name)
                {
                    ProfileCB.SelectedIndex = i;
                    break;
                }
            }

            _currentProfile = profile;
        }

        private void ProfileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfileCB.SelectedIndex >= 0)
            {
                Profile profile = profileList[ProfileCB.SelectedIndex];

                DataLB.DataContext = this;

                if (profile != null)
                {
                    modNameList.Clear();

                    foreach (var entry in profile.entries)
                    {
                        modNameList.Add(entry.fullName + " Version " + entry.version);
                    }

                    _currentProfile = profile;
                }
                else
                    MessageBox.Show("Error: Could not find profile " + ProfileCB.Name + " in profile list", "WTF?", MessageBoxButton.OK);
            }
        }

        private void SaveToClipboard_click(object sender, RoutedEventArgs e)
        {
            if (_currentProfile == null)
                NoProfileError();
            else
                Clipboard.SetText(JsonConvert.SerializeObject(_currentProfile));
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            isClosed = true;
        }
    }
}
