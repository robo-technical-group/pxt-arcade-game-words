using System.Collections;
using System.Numerics;

namespace BloomFilters;
public class BloomFilter
{
    protected BitArray _bits;

    /// <summary>Number of items in filter.</summary>
    public int N { get; protected set; }
    /// <summary>Number of bits in the filter.</summary>
    public int M { get; protected set; }
    /// <summary>Number of hash functions.</summary>
    public int K { get; protected set; }
    /// <summary>Prime number used in hash functions.</summary>
    public BigInteger P { get; protected set; }
    /// <summary>Probability of false positives.</summary>
    public double Probability { get; protected set; }
    /// <summary>Hash function factors.</summary>
    public IList<BigInteger> AValues { get; protected set; }
    /// <summary>Hash function terms.</summary>
    public IList<BigInteger> BValues { get; protected set; }
    /// <summary>Filter bit array as a Base64 string.</summary>
    public string Filter
    {
        get
        {
            byte[] ba = new byte[M / 8];
            _bits.CopyTo(ba, 0);
            return Convert.ToBase64String(ba);
        }
    }

    public BloomFilter(int n, BigInteger p, double prob)
    {
        /**
         * https://hur.st/bloomfilter
         * n = ceil(n / (-k / log(1 - exp(log(p) / k))))
         * p = pow(1 - exp(-k / (m / n), k)
         * m = ceil((n * log(p)) / log(1 / pow(2, log(2))))
         * k = round((m / n) * log(2))
         */
        N = n;
        P = p;
        Probability = prob;
        M = (int)Math.Ceiling(n * Math.Log(prob) / Math.Log(1 /
            Math.Pow(2, Math.Log(2))));
        if (M % 8 > 0)
        {
            M = M + 8 - M % 8;
        }
        K = (int)Math.Round(M / n * Math.Log(2));
        AValues = [];
        BValues = [];
        for (int i = 0; i < K; i++)
        {
            AValues.Add(RandomBigInt.NextBigInteger(p - 1));
            while (AValues[i] == 0)
            {
                AValues[i] = RandomBigInt.NextBigInteger(p - 1);
            }
            BValues.Add(RandomBigInt.NextBigInteger(p - 1));
        }
        _bits = new(M, false);
    }

    public void AddToFilter(BigInteger value)
    {
        for (int i = 0; i < K; i++)
        {
            _bits[GetHashForValue(value, i)] = true;
        }
    }

    public bool IsValueInFilter(BigInteger value)
    {
        for (int i = 0; i < K; i++)
        {
            if (!GetFilterBit(GetHashForValue(value, i)))
            {
                return false;
            }
        }
        return true;
    }

    protected bool GetFilterBit(int location)
    {
        /**
         * Code to test Base64 access.
        int blockNum = location / 24;
        int byteNum = (location % 24) / 8;
        int bitNum = location % 8;

        string block = Filter.Substring(blockNum * 4, 4);
        byte[] bytes = Convert.FromBase64String(block);
        return (bytes[byteNum] & (1 << bitNum)) != 0;
         */

        /**
         * Simpler version.
         */
        return _bits[location];
    }

    protected int GetHashForValue(BigInteger value, int hashIndex)
    {
        // h_a,b(x) = ((ax + b) mod p) mod m
        BigInteger a = AValues[hashIndex];
        BigInteger b = BValues[hashIndex];
        BigInteger prod = a * value + b;
        return (int)(prod % P % M);
    }
}
