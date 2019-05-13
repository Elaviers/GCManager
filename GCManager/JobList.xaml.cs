using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace GCManager
{
    public partial class JobList : UserControl
    {
        public ObservableCollection<JobEntry> lvItems { get; protected set; } = new ObservableCollection<JobEntry>();

        public JobList()
        {
            InitializeComponent();

            this.DataContext = this;

            ModManager.ModDownloadProgressUpdate += OnModDownloading;
            ModManager.ModExtractStarted += OnModExtracting;
            ModManager.ModExtractFinished += OnModExtracted;
            ModManager.ModInstallStarted += OnModInstalling;
            ModManager.ModInstallFinished += OnModInstalled;
            ModManager.ModUninstallFinished += OnModUninstalled;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            lvItems.Clear();
        }

        private JobEntry GetEntry(Mod mod)
        {
            JobEntry entry = null;

            if (lvItems.Count > 0 && lvItems[0].fullName == mod.fullName)
            {
                entry = lvItems[0];
            }
            else
            {
                foreach (JobEntry item in lvItems)
                {
                    if (item.fullName == mod.fullName)
                    {
                        lvItems.Remove(item);
                        entry = item;
                        break;
                    }
                }

                if (entry == null)
                {
                    entry = new JobEntry();
                    entry.fullName = mod.fullName;
                    entry.version = mod.version;
                    entry.progress = 100;
                    entry.image = mod.image;
                }

                lvItems.Insert(0, entry);
            }

            return entry;
        }

        private void OnModDownloading(Mod mod, float progress)
        {
            var entry = GetEntry(mod);
            entry.status = EntryStatus.DOWNLOADING;
            entry.progress = progress;
        }

        private void OnModExtracting(Mod mod)
        {
            GetEntry(mod).status = EntryStatus.EXTRACTING;
            LV.Items.Refresh();
        }

        private void OnModExtracted(Mod mod)
        {
            GetEntry(mod).status = EntryStatus.EXTRACTED;
        }

        private void OnModInstalling(Mod mod)
        {
            GetEntry(mod).status = EntryStatus.INSTALLING;
        }

        private void OnModInstalled(Mod mod)
        {
            GetEntry(mod).status = EntryStatus.INSTALLED;
        }

        private void OnModUninstalled(Mod mod)
        {
            GetEntry(mod).status = EntryStatus.UNINSTALLED;
        }
    }
}
