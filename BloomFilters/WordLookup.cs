using Primes;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace BloomFilters;
public class WordLookup
{
    const double PROBABILITY = 0.001;
    protected Dictionary<int, BloomFilter> _filters;
    protected string _langChars;
    protected int _wordCount;
    protected Dictionary<int, int> _wordCountByLength;

    public WordLookup()
    {
        _filters = [];
        _wordCount = 0;
        _wordCountByLength = [];
        LanguageChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        IsUpperCase = true;
    }

    public int BitsPerChar { get; protected set; }

    public string LanguageChars
    {
        get { return _langChars; }

        [MemberNotNull(nameof(_langChars))]
        set
        {
            _langChars = value ?? string.Empty;
            BitsPerChar = (int)Math.Ceiling(Math.Log2(_langChars.Length));
        }
    }
    public Dictionary<int, BloomFilter> Filters { get { return _filters; } }
    public bool IsUpperCase { get; set; }

    public void AddWord(string word)
    {
        if (!_wordCountByLength.TryGetValue(word.Length, out int wordCount))
        {
            Debug.WriteLine($"Word size {word.Length} is unexpected. Scan the file first. Aborting.");
            return;
        }

        if (!_filters.TryGetValue(word.Length, out BloomFilter? filter))
        {
            /**
             * First encounter of this length of word.
             * Initialize structures.
             * Need to find a random prime larger than the largest word value
             *   for this length of word.
             */
            Debug.WriteLine($"Creating filter for word length {word.Length}.");
            string lastWord = new(_langChars[^1], word.Length);
            string nextWord = new(_langChars[^1], word.Length + 1);
            BigInteger lastWordVal = GetWordValue(lastWord);
            BigInteger nextWordVal = GetWordValue(nextWord);
            BigInteger start = RandomBigInt.NextBigInteger(nextWordVal, lastWordVal);
            BigInteger prime = BigInteger.Zero;
            for (BigInteger i = start; i < nextWordVal; i++)
            {
                if (Miller.IsPrime(i))
                {
                    prime = i;
                    break;
                }
            }
            if (prime == BigInteger.Zero)
            {
                Debug.WriteLine($"Unable to find a prime number for word {word} length {word.Length}; aborting.");
                return;
            }
            filter = new(wordCount, prime, PROBABILITY);
            _filters.Add(word.Length, filter);
        }

        filter.AddToFilter(GetWordValue(word));
        _wordCount++;
    }

    public bool IsWordInFilter(string word)
    {
        if (_filters.TryGetValue(word.Length, out BloomFilter filter))
        {
            return filter.IsValueInFilter(GetWordValue(word));
        }
        else
        {
            return false;
        }
    }

    public BigInteger GetWordValue(string word)
    {
        BigInteger result = BigInteger.Zero;
        word = IsUpperCase ? word.ToUpperInvariant() : word.ToLowerInvariant();
        foreach (char c in word)
        {
            result = (result << BitsPerChar) + LanguageChars.IndexOf(c);
        }
        return BigInteger.Max(result, BigInteger.One);
    }

    public async Task ScanFile(string file)
    {
        using StreamReader sr = new(file);
        string? word;
        while ((word = await sr.ReadLineAsync()) is not null)
        {
            if (!_wordCountByLength.TryAdd(word.Length, 1))
            {
                _wordCountByLength[word.Length]++;
            }
        }
    }

    public async Task ScanStream(Stream s)
    {
        using StreamReader sr = new(s);
        string? word;
        while ((word = await sr.ReadLineAsync()) is not null)
        {
            if (!_wordCountByLength.TryAdd(word.Length, 1))
            {
                _wordCountByLength[word.Length]++;
            }
        }
    }
}
