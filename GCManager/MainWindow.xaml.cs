using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

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
            ModManager.selectedModInfo.image = new BitmapImage(new System.Uri("pack://application:,,,/commando.png"));
            this.DataContext = ModManager.selectedModInfo;

            InstallDirText.Text = ManagerInfo.Get().installDir;

            ModManager.onlineModList = onlineModList;
            ModManager.downloadedModList = downloadedModList;

            ModManager.LocalModDeletionImminent += PreModDeletion;

            OnlineMods.SetModList(onlineModList);
            OnlineMods.RefreshList();
            DownloadedMods.SetModList(downloadedModList);
            DownloadedMods.RefreshList();
        }

        private void PreModDeletion(Mod localMod)
        {
            if (ModManager.selectedModInfo.image == localMod.image)
            {
                ModImg.Source = null;
                ModManager.selectedModInfo.image = null;
            }
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
            MessageBoxResult result = MessageBox.Show("This will uninstall all of your mods and delete everything that's been downloaded.\nYou sure about this?", "Oi", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
                return;

            ModManager.silent = true;

            foreach (Mod mod in downloadedModList.collection)
            {
                ModManager.UninstallMod(mod);

                try
                {
                    Directory.Delete(mod.GetDownloadDirectory(), true);
                }
                catch (IOException) { }
            }

            ModManager.silent = false;
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
