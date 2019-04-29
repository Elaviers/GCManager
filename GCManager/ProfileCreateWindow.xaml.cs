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
