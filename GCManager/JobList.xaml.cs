using System.Windows;
using System.Windows.Controls;

namespace GCManager
{
    public partial class JobList : UserControl
    {
        public JobList()
        {
            InitializeComponent();

        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            LV.Items.Clear();
        }
    }
}
