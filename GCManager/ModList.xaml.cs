using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GCManager
{
    public partial class ModList : UserControl
    {
        private string _filterText;
        public string filterText { get { return _filterText; } set { _filterText = value; _refreshCollectionView(); } }

        private CollectionViewSource _collectionViewSource = new CollectionViewSource();

        public System.ComponentModel.ICollectionView collectionView;

        private object _cvLock = new object();

        public ModList()
        {
            InitializeComponent();
            this.DataContext = this;

            _collectionViewSource.Filter += new FilterEventHandler(DataGrid_Filter);
        }

        private void InstallSelected_Click(object sender, RoutedEventArgs e)
        {
            foreach (Mod mod in DG.SelectedItems)
            {
                ModManager.ActivateMod(mod);
            }
        }

        private void UninstallSelected_Click(object sender, RoutedEventArgs e)
        {
           
            foreach (Mod mod in DG.SelectedItems)
            {
                ModManager.UninstallMod(mod);
            }
        }

        public void GetRelevantMods()
        {
            if (FilterCB.SelectedIndex == 0)
            {
                ModManager.QueryOnlineMods();
                _collectionViewSource.Source = ModManager.onlineMods;
            }
            else
            {
                ModManager.QueryDownloadedMods();
                _collectionViewSource.Source = ModManager.downloadedMods;
            }

           collectionView = _collectionViewSource.View;
            DG.DataContext = _collectionViewSource.View;

            BindingOperations.EnableCollectionSynchronization(collectionView, _cvLock);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            DG.DataContext = null;
            GetRelevantMods();
        }

        private void ModFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (DG == null) return;

            GetRelevantMods();
        }

        private void DG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Mod mod = (Mod)DG.SelectedItem;

            if (mod != null)
            {
                ModManager.selectedModInfo.name = mod.name;
                ModManager.selectedModInfo.author = mod.author;
                ModManager.selectedModInfo.description = mod.description;
                ModManager.selectedModInfo.imageLink = mod.imageLink;
                ModManager.selectedModInfo.isInstalled = mod.isInstalled;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Mod mod = (Mod)DG.SelectedItem;

            if (mod.isInstalled)
                ModManager.UninstallMod(mod);
            else
                ModManager.ActivateMod(mod);
        }

        private void DG_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            //Pass to parent
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent?.RaiseEvent(eventArg);
            }
        }

        private void DataGridHyperlinkColumn_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private void DataGrid_Filter(object sender, FilterEventArgs args)
        {
            if (filterText == null || filterText.Length <= 0)
            {
                args.Accepted = true;
                return;
            }

            Mod mod = (Mod)args.Item;

            string lowerFilterText = filterText.ToLower();

            if (mod != null)
            {
                if (mod.fullName.ToLower().Contains(lowerFilterText) || mod.description.ToLower().Contains(lowerFilterText))
                {
                    args.Accepted = true;
                }
                else args.Accepted = false;
            }
        }

        private void _refreshCollectionView()
        {
            if (collectionView != null)
                collectionView.Refresh();
        }
    }
}
