using System.Windows;

namespace GCManager
{
    public partial class ModInstallWindow : Window
    {
        ModListOnline onlineModList = new ModListOnline();

        private string arg;
        public ModInstallWindow(string arg)
        {
            InitializeComponent();

            this.arg = arg;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] tokens = arg.Split('/');

            if (tokens.Length < 4)
            {
                MessageBox.Show("Unsupported arguments...\nThe problematic arguments are:\n" + arg);
            }
            else
            {
                string author = tokens[tokens.Length - 4];
                string name = tokens[tokens.Length - 3];
                string version = tokens[tokens.Length - 2];

                ModManager.onlineModList = onlineModList;

                onlineModList.RefreshCollection();

                foreach (Mod mod in onlineModList.collection)
                {
                    if (mod.name == name && mod.author == author)
                    {
                        ModManager.ActivateMod(mod, version);
                        break;
                    }
                }
            }
        }
    }
}
