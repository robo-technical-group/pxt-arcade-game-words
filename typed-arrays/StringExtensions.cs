namespace typed_arrays;

public static class StringExtensions
{
    public static IEnumerable<string> Partition(this string input, int partitionSize)
    {
        for (int i = 0; i < input.Length; i += partitionSize)
        {
            yield return input.Substring(i, Math.Min(partitionSize, input.Length - i));
        }
    }
}