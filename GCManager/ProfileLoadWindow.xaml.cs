using System.Windows;

namespace GCManager
{
    public partial class ProfileLoadWindow : Window
    {
        public bool installOverExisting { get; set; }
        public bool installProfileVersions { get; set; }

        public ProfileLoadWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
