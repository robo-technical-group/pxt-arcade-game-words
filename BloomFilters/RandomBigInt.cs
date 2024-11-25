using System.Numerics;
namespace BloomFilters;
public static class RandomBigInt
{
    private static readonly Random _random = new();

    public static BigInteger NextBigInteger(int bitLength)
    {
        if (bitLength < 1) return BigInteger.Zero;

        int byteLength = (bitLength + 7) / 8; // Calculate the number of bytes needed
        byte[] bytes = new byte[byteLength];
        _random.NextBytes(bytes);

        // Ensure the generated number is within the specified bit length
        bytes[^1] &= (byte)(0xFF >> (8 - (bitLength % 8)));

        return new BigInteger(bytes);
    }

    public static BigInteger NextBigInteger(BigInteger maxValue, BigInteger? minValue = null)
    {
        minValue ??= BigInteger.Zero;
        if (minValue == maxValue) { return minValue.Value; }

        BigInteger range = maxValue - minValue.Value;
        int bitLength = (int)Math.Ceiling(BigInteger.Log(range, 2));
        BigInteger randomValue;

        do
        {
            randomValue = NextBigInteger(bitLength);
        } while (randomValue >= range);

        return minValue.Value + randomValue;
    }
}
