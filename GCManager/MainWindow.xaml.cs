using System.Windows;
using System.Diagnostics;
using System.IO;

namespace GCManager
{
    public partial class MainWindow : Window
    {
        ModListOnline onlineModList = new ModListOnline();
        ModListDownloaded downloadedModList = new ModListDownloaded();

        ProfileManager profileManager;
        public MainWindow()
        {
            InitializeComponent();

            this.Title = "GCManager V" + App.VERSION;

            ModManager.selectedModInfo.name = "GCManager";
            ModManager.selectedModInfo.author = "Risk of Rain 2 Mod Manager";
            ModManager.selectedModInfo.imageLink = new System.Uri("pack://application:,,,/commando.png");
            this.DataContext = ModManager.selectedModInfo;

            InstallDirText.Text = ManagerInfo.Get().installDir;

            ModManager.onlineModList = onlineModList;

            OnlineMods.SetModList(onlineModList);
            OnlineMods.RefreshList();
            DownloadedMods.SetModList(downloadedModList);
            DownloadedMods.RefreshList();
        }

        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://run/632360");
        }

        private void OpenDownloads_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", ManagerInfo.Get().GetFullDownloadDirectory());
        }

        private void UpdateMods_Click(object sender, RoutedEventArgs e)
        {
            foreach (Mod mod in downloadedModList.collection)
                ModManager.UpdateMod(mod);
        }

        private void DeleteMods_Click(object sender, RoutedEventArgs e)
        {
            foreach (Mod mod in downloadedModList.collection)
            {
                ModManager.UninstallMod(mod);

                try
                {
                    Directory.Delete(mod.GetDownloadDirectory(), true);
                }
                catch (DirectoryNotFoundException) { }
                catch (IOException ex) { MessageBox.Show("An IO Exception was thrown\n" + ex.Message); }
            }
        }

        private void ChangeGameDir_Click(object sender, RoutedEventArgs e)
        {
            ManagerInfo inst = ManagerInfo.Get();
            string result = GameInstallFinder.FindInstallDir_Dialog();
            if (result != null)
            {
                InstallDirText.Text = result;
                inst.installDir = result;
                inst.Save();
            }
        }

        private void OpenProfileManager_Click(object sender, RoutedEventArgs e)
        {
            if (profileManager == null || profileManager.isClosed)
                profileManager = new ProfileManager();

            profileManager.Show();
        }
    }
}
