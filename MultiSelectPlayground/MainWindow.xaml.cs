using System.ComponentModel;
using System.Windows;

namespace MultiSelectPlayground
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public class Status
        {
            public bool IsSelected { get; set; }
            public string StatusCode { get; set; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged is not null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public List<Status> Statuses1 { get; set; }
        public List<Status> Statuses2 { get; set; }
        public List<String> Statuses3 { get; set; }

        protected string _checkedList1 = string.Empty;
        public string CheckedList1
        {
            get { return _checkedList1; }
            set
            {
                _checkedList1 = value;
                NotifyPropertyChanged("CheckedList1");
            }
        }

        protected string _checkedList2 = string.Empty;
        public string CheckedList2
        {
            get { return _checkedList2; }
            set
            {
                _checkedList2 = value;
                NotifyPropertyChanged("CheckedList2");
            }
        }

        protected string _checkedList3 = string.Empty;
        public string CheckedList3
        {
            get { return _checkedList3; }
            set
            {
                _checkedList3 = value;
                NotifyPropertyChanged("CheckedList3");
            }
        }

        public MainWindow()
        {
            Statuses1 =
            [
                new Status { IsSelected = false, StatusCode = "OPEN" },
                new Status { IsSelected = false, StatusCode = "APPROVED" },
                new Status { IsSelected = false, StatusCode = "CLOSED" },
            ];
            Statuses2 =
            [
                new Status { IsSelected = false, StatusCode = "OPEN" },
                new Status { IsSelected = false, StatusCode = "APPROVED" },
                new Status { IsSelected = false, StatusCode = "CLOSED" },
            ];
            Statuses3 = [ "OPEN", "APPROVED", "CLOSED", ];
            InitializeComponent();
        }

        protected void Check1_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            CheckedList1 = string.Join(",",
                Statuses1.Where((s) => s.IsSelected).Select((x) => x.StatusCode).ToArray());
        }

        protected void Check2_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            CheckedList2 = string.Join(",",
                Statuses2.Where((s) => s.IsSelected).Select((x) => x.StatusCode).ToArray());
        }

        protected void Check3_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            CheckedList3 = string.Join(",", Statuses3ListBox.SelectedItems as IList<string>);
        }
    }
}