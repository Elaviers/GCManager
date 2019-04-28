using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GCManager
{
    public partial class ModListControl : UserControl
    {
        private ModList _modList;
        public ModList modList { get { return _modList; } }

        private string _filterText;
        public string filterText { get { return _filterText; } set { _filterText = value; _refreshCollectionView(); } }

        private CollectionViewSource _collectionViewSource = new CollectionViewSource();
        public System.ComponentModel.ICollectionView collectionView;

        public ModListControl()
        {
            InitializeComponent();
            this.DataContext = this;

            _collectionViewSource.Filter += new FilterEventHandler(DataGrid_Filter);
        }

        public void SetModList(ModList ml)
        {
            _modList = ml;
            _collectionViewSource.Source = _modList.collection;
        }

        private void UpdateSelected_Click(object sender, RoutedEventArgs e)
        {
            foreach (Mod mod in DG.SelectedItems)
            {
                ModManager.UpdateMod(mod);
            }
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

        public void RefreshList()
        {
            _modList.GetMods();

           collectionView = _collectionViewSource.View;
            DG.DataContext = _collectionViewSource.View;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            DG.DataContext = null;
            RefreshList();
        }

        private void ModFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (DG == null) return;

            _refreshCollectionView();
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
            foreach (Mod mod in DG.SelectedItems)
            {
                if (mod.isInstalled)
                    ModManager.UninstallMod(mod);
                else
                    ModManager.ActivateMod(mod);
            }
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
