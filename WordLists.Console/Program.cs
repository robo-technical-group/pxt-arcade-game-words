using ftss;
using System.Reflection;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

int choice = -1;
const int EXIT = 99;
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
            break;

        case 2:
            wordSet = [];
            break;

        case 3:
            AddFileToBloom();
            break;

        case 4:
            await AddFileToFtss();
            break;

        case 5:
            break;

        case 6:
            await WriteWordSet();
            break;
    }
}

void AddFileToBloom()
{

}

async Task AddFileToFtss()
{
    wordSet ??= [];
    string? file = GetDataFile();
    if (file is null) { return; }
    Assembly asm = Assembly.GetExecutingAssembly();
    string resource = string.Format("GameWords.Console.Resources.{0}", file);
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

string? GetDataFile()
{
    Console.WriteLine("Enter resource or local filename.");
    string? file = Console.ReadLine();
    if (file is null) { return null; }
    Assembly asm = Assembly.GetExecutingAssembly();
    string resource = string.Format("GameWords.Console.Resources.{0}", file);
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
    Console.WriteLine($"{EXIT}. Quit");
    Console.Write("Enter selection --> ");
}

async Task WriteBloomFilter()
{

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
    await sw.WriteLineAsync(@"namespace GameWords {
    export const WORD_SET_BASE_64_STRINGS: string[] = [");
    foreach (string s in strings)
    {
        await sw.WriteAsync("\"");
        await sw.WriteAsync(s);
        await sw.WriteLineAsync("\",");
    }
    await sw.WriteLineAsync("    ]}");
}