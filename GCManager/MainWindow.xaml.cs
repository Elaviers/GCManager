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
            
            Mods.GetRelevantMods();
        }

        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://run/632360");
        }

        private void OpenDownloads_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", ManagerInfo.Get().DownloadsFolderName);
        }

        private void UpdateMods_Click(object sender, RoutedEventArgs e)
        {
            ModManager.UpdateMods();
        }
    }
}
