using BloomFilters;
using ftss;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows;

namespace WordLists.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> ExclusionFiles { get; set; }
        public ObservableCollection<string> WordFiles { get; set; }

        protected const string INDENT = "    ";

        [GeneratedRegex("[^a-zA-Z0-9]")]
        private static partial Regex AlphaNumericRegex();

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

                WordLookup filter = new();
                FastTernaryStringSet wordset = [];
                wordset.ForceUppercase = true;
                bool areWordStems = WordStemCheckBox.IsChecked ?? false;
                bool useBloom = BloomCheckBox.IsChecked ?? false;
                bool useFtss = FtssCheckBox.IsChecked ?? false;
                foreach (string wordFile in  WordFiles)
                {
                    if (useBloom)
                    {
                        await filter.ScanFile(wordFile);
                    }
                    using StreamReader reader = new(wordFile);
                    while ((line = await reader.ReadLineAsync()) is not null)
                    {
                        string word = line.ToUpper();
                        if (areWordStems)
                        {
                            bool goodWord = true;
                            foreach (string stem in exclusions)
                            {
                                if (word.Contains(stem))
                                {
                                    goodWord = false;
                                    break;
                                }
                            }
                            if (goodWord)
                            {
                                if (useBloom)
                                {
                                    filter.AddWord(line);
                                }
                                if (useFtss)
                                {
                                    wordset.Add(line);
                                }
                            }
                        }
                        else
                        {
                            if (!exclusions.Contains(word))
                            {
                                if (useBloom)
                                {
                                    filter.AddWord(line);
                                }
                                if (useFtss)
                                {
                                    wordset.Add(line);
                                }
                            }
                        }
                    }
                }
                if (useFtss)
                {
                    wordset.Balance();
                    wordset.Compact();
                }
                using StreamWriter writer = new(sfd.FileName);
                await WriteHeader(writer, useBloom, useFtss);
                if (useBloom)
                {
                    await WriteBloom(writer, filter,
                        String.IsNullOrWhiteSpace(BloomNameTextBox.Text) ?
                        "Bloom Filter" :
                        BloomNameTextBox.Text.Trim());
                }
                if (useFtss)
                {
                    await WriteFtss(writer, wordset,
                        String.IsNullOrWhiteSpace(FtssNameTextBox.Text) ?
                        "Word Set" :
                        FtssNameTextBox.Text.Trim());
                }
                await WriteFooter(writer);

                MessageBox.Show($"Done! Output file: {sfd.FileName}.");
            }
        }

        private static async Task WriteBloom(StreamWriter writer, WordLookup bloom, string name)
        {
            string bloomName = AlphaNumericRegex().Replace(name, "");
            string filterArrayName = bloomName.ToUpper();

            await writer.WriteAsync(INDENT);
            await writer.WriteLineAsync($"const {filterArrayName}: BloomFilter[] = [");
            for (int i = 0; i <= bloom.Filters.Keys.Max(); i++)
            {
                if (bloom.Filters.TryGetValue(i, out var filter))
                {
                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(INDENT);
                    await writer.WriteLineAsync('{');

                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(INDENT);
                    await writer.WriteLineAsync($"// {i}-letter words.");

                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(INDENT);
                    await writer.WriteLineAsync($"n: {filter.N}, m: {filter.M}, k: {filter.K}, p: '{filter.P}', prob: {filter.Probability},");

                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(@"aValues: [");
                    foreach (BigInteger a in filter.AValues)
                    {
                        await writer.WriteAsync($"'{a}', ");
                    }
                    await writer.WriteLineAsync(@"],");

                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(@"bValues: [");
                    foreach (BigInteger b in filter.BValues)
                    {
                        await writer.WriteAsync($"'{b}', ");
                    }
                    await writer.WriteLineAsync(@"],");

                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(@"filter: '");
                    await writer.WriteAsync(filter.Filter);
                    await writer.WriteLineAsync(@"',");

                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(INDENT);
                    await writer.WriteLineAsync(@"},");
                }
                else
                {
                    await writer.WriteAsync(INDENT);
                    await writer.WriteAsync(INDENT);
                    await writer.WriteLineAsync($"null, // No {i}-letter words.");
                }
            }

            await writer.WriteAsync(INDENT);
            await writer.WriteLineAsync(']');
            await writer.WriteLineAsync();

            await writer.WriteAsync(INDENT);
            await writer.WriteLineAsync(@"//% fixedInstance");

            await writer.WriteAsync(INDENT);
            await writer.WriteLineAsync($"//% block=\"{name}\" weight=100");

            await writer.WriteAsync(INDENT);
            await writer.WriteAsync($"export const {bloomName}: Bloom = new Bloom({filterArrayName}, ");
            await writer.WriteAsync(bloom.IsUpperCase.ToString().ToLower());
            await writer.WriteLineAsync(')');
        }

        private static async Task WriteFtss(StreamWriter writer, FastTernaryStringSet ftss, string name)
        {
            string ftssName = AlphaNumericRegex().Replace(name, "");
            string stringSetName = ftssName.ToUpper();

            await writer.WriteAsync(INDENT);
            await writer.WriteLineAsync($"const {stringSetName}: string[] = [");
            foreach (string s in ftss.ToBuffer().ToBase64StringSet())
            {
                await writer.WriteAsync(INDENT);
                await writer.WriteAsync(INDENT);
                await writer.WriteAsync('"');
                await writer.WriteAsync(s);
                await writer.WriteAsync('"');
                await writer.WriteLineAsync(',');
            }
            await writer.WriteAsync(INDENT);
            await writer.WriteLineAsync(']');
            await writer.WriteLineAsync();

            await writer.WriteAsync(INDENT);
            await writer.WriteLineAsync(@"//% fixedInstance");

            await writer.WriteAsync(INDENT);
            await writer.WriteLineAsync($"//% block=\"{name}\" weight=100");

            await writer.WriteAsync(INDENT);
            await writer.WriteLineAsync($"export let {ftssName}: TernaryStringSet =");

            await writer.WriteAsync(INDENT);
            await writer.WriteAsync(INDENT);
            await writer.WriteLineAsync($"TernaryStringSet.fromB64StringSet({stringSetName})");

            await writer.WriteAsync(INDENT);
            await writer.WriteLineAsync($"{ftssName}.forceUpper = {ftss.ForceUppercase.ToString().ToLower()}");
        }

        private static async Task WriteFooter(StreamWriter writer)
        {
            await writer.WriteLineAsync('}');
        }

        private static async Task WriteHeader(StreamWriter writer, bool includeBloomUrl, bool includeFtssUrl)
        {
            await writer.WriteLineAsync("/**");
            await writer.WriteLineAsync(" * Add the following extension(s) to your project:");
            if (includeBloomUrl)
            {
                await writer.WriteLineAsync(" *   https://github.com/robo-technical-group/pxt-bloom-filters.git");
            }
            if (includeFtssUrl)
            {
                await writer.WriteLineAsync(" *   https://github.com/robo-technical-group/pxt-fast-ternary-string-set.git");
            }
            await writer.WriteLineAsync(" */ ");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync(@"//% weight=0 color=#000000 icon=""\uf022"" block=""Word Lists""");
            await writer.WriteLineAsync(@"namespace WordLists {");
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