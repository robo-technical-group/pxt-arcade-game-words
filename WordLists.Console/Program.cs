using BloomFilters;
using ftss;
using System.Numerics;
using System.Reflection;
using System.Text;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

int choice = -1;
const int EXIT = 99;
const string EXIT_STRING = "EXIT NOW";
WordLookup? filter = null;
FastTernaryStringSet? wordSet = null;

while (choice != EXIT)
{
    PrintMenu();
    bool valid = Int32.TryParse(Console.ReadLine(), out choice);
    if (!valid)
    {
        choice = -1;
        continue;
    }
    switch (choice)
    {
        case 1:
            filter = new();
            break;

        case 2:
            wordSet = [];
            break;

        case 3:
            await AddFileToBloom();
            break;

        case 4:
            await AddFileToFtss();
            break;

        case 5:
            await WriteBloomFilter();
            break;

        case 6:
            await WriteWordSet();
            break;

        case 7:
            TestBloom();
            break;

        case 8:
            TestFtss();
            break;
    }
}

async Task AddFileToBloom()
{
    filter ??= new();
    string? file = GetDataFile();
    if (file is null) { return; }
    Assembly asm = Assembly.GetExecutingAssembly();
    string resource = string.Format("WordLists.Console.Resources.{0}", file);
    Stream? stream = asm.GetManifestResourceStream(resource);
    if (stream is null)
    {
        if (File.Exists(file))
        {
            Console.WriteLine($"Adding local file {file} to word set.");
            await filter.ScanFile(file);
            using StreamReader sr = new(file);
            string? line;
            while ((line = await sr.ReadLineAsync()) is not null)
            {
                filter.AddWord(line);
            }
        }
        else
        {
            Console.WriteLine($"File {file} not found.");
        }
    }
    else
    {
        Console.WriteLine($"Adding resource file {file} to word set.");
        await filter.ScanStream(asm.GetManifestResourceStream(resource)!);
        StreamReader reader = new(stream);
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            filter.AddWord(line);
        }
        reader.Close();
        stream.Close();
    }
}

async Task AddFileToFtss()
{
    wordSet ??= [];
    string? file = GetDataFile();
    if (file is null) { return; }
    Assembly asm = Assembly.GetExecutingAssembly();
    string resource = string.Format("WordLists.Console.Resources.{0}", file);
    Stream? stream = asm.GetManifestResourceStream(resource);
    if (stream is null)
    {
        if (File.Exists(file))
        {
            Console.WriteLine($"Adding local file {file} to word set.");
            using StreamReader sr = new(file);
            string? line;
            while ((line = await sr.ReadLineAsync()) is not null)
            {
                wordSet.Add(line.ToUpperInvariant());
            }
        }
        else
        {
            Console.WriteLine($"File {file} not found.");
        }
    }
    else
    {
        Console.WriteLine($"Adding resource file {file} to word set.");
        StreamReader reader = new(stream);
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            wordSet.Add(line);
        }
        reader.Close();
        stream.Close();
    }
}

static string BigIntegerListToString(IList<BigInteger> list)
{
    StringBuilder sb = new();
    sb.Append('[');
    foreach (BigInteger i in list)
    {
        sb.Append('\'');
        sb.Append(i.ToString());
        sb.Append("',");
    }
    sb.Append(']');
    return sb.ToString();
}

string? GetDataFile()
{
    Console.WriteLine("Enter resource or local filename.");
    string? file = Console.ReadLine();
    if (file is null) { return null; }
    Assembly asm = Assembly.GetExecutingAssembly();
    string resource = string.Format("WordLists.Console.Resources.{0}", file);
    Stream? stream = asm.GetManifestResourceStream(resource);
    if (stream is null)
    {
        return File.Exists(file) ? file : null;
    }
    else
    {
        stream.Close();
        return file;
    }
}

string? GetOutputFile()
{
    Console.WriteLine("Enter local filename for output.");
    string? file = Console.ReadLine();
    if (file is null) { return null; }
    string? dir = Path.GetDirectoryName(file);
    if (dir is not null && !Directory.Exists(dir))
    {
        try
        {
            Directory.CreateDirectory(dir);
        }
        finally { }
        if (!Directory.Exists(dir))
        {
            Console.WriteLine($"Cannot create output directory {dir}; aborting.");
            return null;
        }
    }
    return file;
}

void PrintMenu()
{
    Console.WriteLine("=====");
    Console.WriteLine("MAIN MENU");
    Console.WriteLine("=====");
    Console.WriteLine("1. Create new Bloom filter.");
    Console.WriteLine("2. Create new Ternary set.");
    Console.WriteLine("3. Add file to Bloom filter.");
    Console.WriteLine("4. Add file to Ternary set.");
    Console.WriteLine("5. Write Bloom filter to disk.");
    Console.WriteLine("6. Write Ternary set to disk.");
    Console.WriteLine("7. Test Bloom filter.");
    Console.WriteLine("8. Test Ternary set.");
    Console.WriteLine($"{EXIT}. Quit");
    Console.Write("Enter selection --> ");
}

void TestBloom()
{
    if (filter is null)
    {
        Console.WriteLine("Bloom filter has not been created.");
        return;
    }
    string input = "";
    while (input != EXIT_STRING)
    {
        Console.Write($"Enter word to search or {EXIT_STRING} to quit:  ");
        input = Console.ReadLine() ?? EXIT_STRING;
        if (EXIT_STRING.Equals(input, StringComparison.OrdinalIgnoreCase))
        {
            break;
        }
        if (filter.IsWordInFilter(input))
        {
            Console.WriteLine($"Your word {input} is likely in the database.");
        }
        else
        {
            Console.WriteLine($"Your word {input} is not in the database.");
        }
    }
}

void TestFtss()
{
    if (wordSet is null)
    {
        Console.WriteLine("Word set has not been created.");
        return;
    }
    string input = "";
    while (input != EXIT_STRING)
    {
        Console.Write($"Enter word to search or {EXIT_STRING} to quit:  ");
        input = Console.ReadLine() ?? EXIT_STRING;
        if (EXIT_STRING.Equals(input, StringComparison.OrdinalIgnoreCase))
        {
            break;
        }
        if (wordSet.Has(input))
        {
            Console.WriteLine($"Your word {input} is in the word set.");
        }
        else
        {
            Console.WriteLine($"Your word {input} is not in the word set.");
        }
    }
}

async Task WriteBloomFilter()
{
    filter ??= new();
    string? outFile = GetOutputFile();
    if (outFile is null) { return; }
    using StreamWriter sw = new(outFile);
    await sw.WriteLineAsync(@"namespace WordLists {
    const FILTERS: BloomFilter[] = [ null, null,");
    foreach (KeyValuePair<int, BloomFilter> pair in filter.Filters)
    {
        int wordLength = pair.Key;
        BloomFilter bf = pair.Value;
        await sw.WriteLineAsync($$"""
{
    // {{wordLength}}-letter words.
    n: {{bf.N}},
    m: {{bf.M}},
    k: {{bf.K}},
    p: '{{bf.P}}',
    prob: {{bf.Probability}},
    aValues: {{BigIntegerListToString(bf.AValues)}},
    bValues: {{BigIntegerListToString(bf.BValues)}},
    filter: '{{bf.Filter}}',
},
""");
    }
    await sw.WriteLineAsync(@"
]
//% fixedInstance
//% block=""Game Words 2020"" weight=100
export const GameWords2020: Bloom = new Bloom(FILTERS)
}");
}

async Task WriteWordSet()
{
    wordSet ??= [];
    wordSet.Balance();
    wordSet.Compact();
    string? outFile = GetOutputFile();
    if (outFile is null) { return; }
    IEnumerable<string> strings = wordSet.ToBuffer().ToBase64StringSet();
    using StreamWriter sw = new(outFile);
    await sw.WriteLineAsync(@"namespace WordLists {
    export const WORD_SET_BASE_64_STRINGS: string[] = [");
    foreach (string s in strings)
    {
        await sw.WriteAsync("\"");
        await sw.WriteAsync(s);
        await sw.WriteLineAsync("\",");
    }
    await sw.WriteLineAsync("    ]}");
}