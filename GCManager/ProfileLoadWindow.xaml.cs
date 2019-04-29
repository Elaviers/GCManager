using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GCManager
{
    /// <summary>
    /// Interaction logic for ProfileLoadWindow.xaml
    /// </summary>
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
