using System.Numerics;
using System.Text.RegularExpressions;
namespace Primes;

public static class Miller
{
    private record struct Params(
        BigInteger N,
        BigInteger S,
        BigInteger D,
        List<int> AValues
    );

    private static int[] SMALL_PRIMES = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97,];
    public static readonly BigInteger MAX_PRIME =
        BigInteger.Parse("3_317_044_064_679_887_385_961_981".Replace("_", ""));

    private static readonly Dictionary<BigInteger, List<int>> BASES = new()
    {
        { BigInteger.Parse("2_047".Replace("_", "")), [2,] },
        { BigInteger.Parse("1_373_653".Replace("_", "")), [2, 3,] },
        { BigInteger.Parse("9_080_191".Replace("_", "")), [31, 73,] },
        { BigInteger.Parse("25_326_001".Replace("_", "")), [2, 3, 5,] },
        { BigInteger.Parse("3_215_031_751".Replace("_", "")), [2, 3, 5, 7,] },
        { BigInteger.Parse("4_759_123_141".Replace("_", "")), [2, 7, 61,] },
        { BigInteger.Parse("1_122_004_669_633".Replace("_", "")), [2, 13, 23, 1_662_803,] },
        { BigInteger.Parse("2_152_302_898_747".Replace("_", "")), [2, 3, 5, 7, 11,] },
        { BigInteger.Parse("3_474_749_660_383".Replace("_", "")), [2, 3, 5, 7, 11, 13,] },
        { BigInteger.Parse("341_550_071_728_321".Replace("_", "")), [2, 3, 5, 7, 11, 13, 17,] },
        { BigInteger.Parse("3_825_123_056_546_413_051".Replace("_", "")), [2, 3, 5, 7, 11, 13, 17, 19, 23,] },
        { BigInteger.Parse("18_446_744_073_709_551_616".Replace("_", "")), [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37,] },
        { BigInteger.Parse("318_665_857_834_031_151_167_461".Replace("_", "")), [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37,] },
        { BigInteger.Parse("3_317_044_064_679_887_385_961_981".Replace("_", "")), [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41,] },
    };

    public static bool IsPrime(BigInteger n)
    {
        if (n == 2) { return true; }
        if (n < 2 || (n & BigInteger.One) == 0) { return false; }
        Params p = new()
        {
            N = n
        };
        LoadSandD(ref p);
        LoadAValues(ref p);
        if (p.AValues is null)
        {
            // Go the long route.
            p.AValues = [];
            BigInteger maxA = BigInteger.Min(n - 2, 2 * BigInteger.Pow(BigInteger.Log2(n), 2));
            foreach (int a in SMALL_PRIMES)
            {
                if (a <= maxA)
                {
                    p.AValues.Add(a);
                }
            }
        }
        return IsPrime(p);
    }

    public static bool IsPrime(string n)
    {
        if (BigInteger.TryParse(Regex.Replace(n, "[^0-9]", ""), out BigInteger result))
        {
            return IsPrime(result);
        }
        return false;
    }

    private static bool IsPrime(Params p)
    {
        // None of these should be true, but verify anyway.
        if (
            p.AValues is null ||
            p.S <= 0 ||
            p.D <= 0 ||
            (p.D & BigInteger.One) == 0
        )
        {
            return false;
        }

        foreach (int a in p.AValues)
        {
            BigInteger x = BigInteger.ModPow(a, p.D, p.N);
            BigInteger y = BigInteger.Zero;
            for (int i = 0; i < p.S; i++)
            {
                y = BigInteger.ModPow(x, 2, p.N);
                if (
                    y == BigInteger.One &&
                    x != BigInteger.One &&
                    x != p.N - 1
                )
                {
                    return false;
                }
                x = y;
            }
            if (y != BigInteger.One)
            {
                return false;
            }
        }

        return true;
    }

    private static void LoadAValues(ref Params p)
    {
        foreach (KeyValuePair<BigInteger, List<int>> pair in BASES)
        {
            if (p.N < pair.Key)
            {
                p.AValues = pair.Value;
                break;
            }
        }
    }

    // https://github.com/jirkavrba/miller-rabin-ts
    private static void LoadSandD(ref Params p)
    {
        p.S = BigInteger.Zero;
        p.D = p.N - BigInteger.One;
        while ((p.D & BigInteger.One) == BigInteger.Zero)
        {
            p.S++;
            p.D >>= 1;
        }
    }
}
