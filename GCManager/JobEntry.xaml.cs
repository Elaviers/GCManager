using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace GCManager
{
    public enum EntryStatus
    {
        DOWNLOADING,
        EXTRACTING,
        EXTRACTED,
        INSTALLING,
        INSTALLED,
        UNINSTALLED
    }

    public class EntryStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EntryStatus)
            {
                EntryStatus v = (EntryStatus)value;

                switch (v)
                {
                    case EntryStatus.INSTALLED:
                        return "Installed";

                    case EntryStatus.UNINSTALLED:
                        return "Uninstalled";

                    case EntryStatus.DOWNLOADING:
                        return "Downloading...";

                    case EntryStatus.EXTRACTING:
                        return "Extracting...";

                    case EntryStatus.EXTRACTED:
                        return "Extracted";

                    case EntryStatus.INSTALLING:
                        return "Installing...";
                }
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EntryInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private float _progress;

        private EntryStatus _status;

        public float progress { get { return _progress; } set { _progress = value; NotifyPropertyChanged("progress"); } }

        public Uri imageLink { get; set; }

        public string fullName { get; set; }

        public string version { get; set; }

        public EntryStatus status { get { return _status; } set { _status = value; NotifyPropertyChanged("status"); } }

    }

    public partial class JobEntry : UserControl
    {
        public EntryInfo entryInfo;

        public JobEntry(EntryInfo data)
        {
            InitializeComponent();

            this.entryInfo = data;
            this.DataContext = data;
        }
    }
}
