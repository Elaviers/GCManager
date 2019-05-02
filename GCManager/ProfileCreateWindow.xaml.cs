using System.Windows;

namespace GCManager
{

    public partial class ProfileCreateWindow : Window
    {
        public string profileName { get; set; }

        public ProfileCreateWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
