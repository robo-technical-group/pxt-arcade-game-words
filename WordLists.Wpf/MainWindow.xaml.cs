using BloomFilters;
using ftss;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
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
        public ObservableCollection<string> ExclusionFiles { get; set; }
        public ObservableCollection<string> WordFiles { get; set; }

        public MainWindow()
        {
            ExclusionFiles = [];
            WordFiles = [];
            DataContext = this;
            InitializeComponent();
        }

        private void AddExclusionList()
        {
            OpenFileDialog ofd = new()
            {
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt|All files|*.*",
            };
            bool? result = ofd.ShowDialog();

            if (result == true)
            {
                ExclusionFiles.Add(ofd.FileName);
            }
        }

        private void AddWordList()
        {
            OpenFileDialog ofd = new()
            {
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt|All files|*.*",
            };
            bool? result = ofd.ShowDialog();

            if (result == true)
            {
                WordFiles.Add(ofd.FileName);
            }
        }

        private async Task Build()
        {
            SaveFileDialog sfd = new()
            {
                DefaultExt = ".ts",
                Filter = "TypeScript files (.ts)|*.ts",
            };
            bool? result = sfd.ShowDialog();
            if (result == true)
            {
                List<string> exclusions = [];
                string? line;
                foreach (string exclusionFile in ExclusionFiles)
                {
                    using StreamReader reader = new(exclusionFile);
                    while ((line = await reader.ReadLineAsync()) is not null)
                    {
                        exclusions.Add(line.ToUpper());
                    }
                }

                BloomFilter filter = new();
                FastTernaryStringSet wordset = [];

                foreach (string wordFile in  WordFiles)
                {
                    using StreamReader reader = new(wordFile);
                    while ((line = await reader.ReadLineAsync()) is not null)
                    {
                        string word = line.ToUpper();
                        if (!exclusions.Contains(word))
                        {
                            // filter.Add(word);
                            wordset.Add(word);
                        }
                    }
                }

                using StreamWriter writer = new(sfd.FileName);
                foreach (string word in wordset.ToList())
                {
                    await writer.WriteLineAsync(word);
                }

                MessageBox.Show($"Done! Output file: {sfd.FileName}.");
            }
        }

        private void AddWordList_Click(object sender, RoutedEventArgs e)
        {
            AddWordList();
        }

        private void RemoveWordList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddExclusion_Click(object sender, RoutedEventArgs e)
        {
            AddExclusionList();
        }

        private void RemoveExclusion_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Build_Click(object sender, RoutedEventArgs e)
        {
            await Build();
        }
    }
}