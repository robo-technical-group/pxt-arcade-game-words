using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WordLists.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> WordFiles { get; set; }

        public MainWindow()
        {
            WordFiles = [];
            DataContext = this;
            InitializeComponent();
        }

        private void AddWordList()
        {
            OpenFileDialog ofd = new()
            {
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt"
            };
            bool? result = ofd.ShowDialog();

            if (result == true)
            {
                WordFiles.Add(ofd.FileName);
            }
        }

        private void AddWordList_Click(object sender, RoutedEventArgs e)
        {
            AddWordList();
        }

        private void RemoveWordList_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}