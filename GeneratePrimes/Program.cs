// See https://aka.ms/new-console-template for more information
using GeneratePrimes;
using System.Numerics;

Console.WriteLine("Hello, World!");

int passes = 0;
while (passes < 1)
{
    Console.Write("How many passes to run? ");
    if (!int.TryParse(Console.ReadLine(), out passes))
    {
        passes = 0;
    }
}

Generator g = new();
for (int i = 0; i < passes; i++)
{
    await g.Run();
}

int count = 0;
BigInteger largePrime = 0;
using (StreamReader sr = File.OpenText(Generator.OutFile))
{
    Console.Write("Reading data file. ");
    string? line;
    while ((line = await sr.ReadLineAsync()) is not null)
    {
        if (BigInteger.TryParse(line, out largePrime))
        {
            count++;
        }
    }
}
Console.Write($"Found {count:###,###,###,###} prime numbers. ");
Console.WriteLine($"Largest prime: {largePrime:###,###,###,###}.");
