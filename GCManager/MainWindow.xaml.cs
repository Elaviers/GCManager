using System.Windows;
using System.Diagnostics;

namespace GCManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Title = "GCManager V" + App.VERSION;

            ModManager.selectedModInfo.name = "GCManager";
            ModManager.selectedModInfo.author = "Risk of Rain 2 Mod Manager";
            ModManager.selectedModInfo.imageLink = new System.Uri("pack://application:,,,/commando.png");
            this.DataContext = ModManager.selectedModInfo;

            InstallDirText.Text = ManagerInfo.Get().installDir;

            Mods.GetRelevantMods();
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
            ModManager.UpdateMods();
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
    }
}
