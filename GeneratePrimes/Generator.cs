using System.Collections;
using System.Dynamic;
using System.Numerics;

namespace GeneratePrimes;

internal class Generator
{
    protected readonly static string OUT_DIR =
        Environment.SpecialFolder.LocalApplicationData.ToString();
    protected const string OUT_FILE_NAME = "primes.txt";
    protected const int SEARCH_INTERVAL = 1_000_000_000;
    protected readonly static string OUT_FILE = Path.Combine(OUT_DIR, OUT_FILE_NAME);

    protected BitArray _bits;
    protected BigInteger _high = SEARCH_INTERVAL;
    protected BigInteger _lastPrime = 0;
    protected BigInteger _low = 1;
    protected System.Diagnostics.Stopwatch _watch;

    public Generator()
    {
        if (!Directory.Exists(OUT_DIR))
        {
            Directory.CreateDirectory(OUT_DIR);
        }

        // Figure out where we left off.
        if (File.Exists(OUT_FILE))
        {
            Console.Write("Reading primes from data file. ");
            int lineCount = 0;
            StreamReader sr = File.OpenText(OUT_FILE);
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                _ = BigInteger.TryParse(line, out _lastPrime);
                lineCount++;
            }
            sr.Close();
            Console.Write($"Read {lineCount:###,###,###,###} lines. ");
            Console.WriteLine($"Last prime found: {_lastPrime:###,###,###,###}.");
            _high = (_lastPrime / SEARCH_INTERVAL + 1) * SEARCH_INTERVAL;
            _low = _high + 1;
            _high += SEARCH_INTERVAL;
        }

        _bits = new((int)(_high - _low + 1), true);
        _watch = new();
    }

    public static string OutFile { get { return OUT_FILE; } }

    public async Task Run()
    {
        Console.Write($"Finding primes between {_low:###,###,###,###} and {_high:###,###,###,###}. ");
        System.Diagnostics.Stopwatch watch = new();
        int numPrimes = 0;
        watch.Start();
        _watch.Start();

        // Sieve of Eratosthenes.
        // Reset bits.
        for (int i = 0; i < _bits.Length; i++)
        {
            _bits[i] = true;
        }
        if (_low == 1)
        {
            _bits[0] = false;
        }

        // Clear bits for previous primes.
        if (File.Exists(OUT_FILE))
        {
            StreamReader sr = File.OpenText(OUT_FILE);
            string? line;
            while ((line = await sr.ReadLineAsync()) is not null)
            {
                if (BigInteger.TryParse(line, out BigInteger p))
                {
                    for (BigInteger i = (_low / p + 1) * p; i <= _high; i += p)
                    {
                        int index = GetIndex(i);
                        if (index > -1)
                        {
                            _bits[index] = false;
                        }
                    }
                }
            }
            sr.Close();
        }

        // Find new primes.
        for (BigInteger i = _low; i * i <= _high; i++)
        {
            int iIndex = GetIndex(i);
            if (_bits[iIndex])
            {
                for (BigInteger j = i *i; j <= _high; j += i)
                {
                    int jIndex = GetIndex(j);
                    if (jIndex > -1)
                    {
                        _bits[jIndex] = false;
                    }
                }
            }
        }

        _watch.Stop();
        watch.Stop();
        Console.WriteLine("Done.");
        Console.Write("  Writing primes. ");
        using (StreamWriter sw = File.AppendText(OutFile))
        {
            for (BigInteger i = _low; i <= _high; i++)
            {
                if (_bits[GetIndex(i)])
                {
                    await sw.WriteLineAsync(i.ToString());
                    _lastPrime = i;
                    numPrimes++;
                }
            }
        }
        Console.WriteLine("Done.");
        Console.Write($"  Primes found: {numPrimes:###,###,###,###}. ");
        Console.Write($"Last prime found: {_lastPrime:###,###,###,###}. ");
        Console.WriteLine($"Time elapsed: {watch.Elapsed.TotalSeconds:#,##0.00}.");
        _low = _high + 1;
        _high += SEARCH_INTERVAL;
    }

    protected int GetIndex(BigInteger value)
    {
        if (value < _low || value > _high)
        {
            return -1;
        }
        else
        {
            return (int)(value - _low);
        }
    }
}
