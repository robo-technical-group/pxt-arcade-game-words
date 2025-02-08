using ftss;
using typed_arrays;

namespace ftss_tests;
[TestClass]
public class SerializationTestsB64
{
    [TestMethod]
    public void EmptyTreeHeaderOnly()
    {
        FastTernaryStringSet test = [];
        Assert.AreEqual(8, test.ToBuffer().ByteLength, "Test A");
        RoundTrip(test, "Test B");
    }

    [TestMethod]
    public void NonEmptyTreeHasNodes()
    {
        FastTernaryStringSet test = [];
        test.Add("a");
        // HEADER + 1 node enc. + 1 char + 0 * 3 branches.
        Assert.AreEqual(10, test.ToBuffer().ByteLength, "Test A");
        RoundTrip(test, "Test B");

        test.Clear();
        test.Add("ɑ");
        // HEADER + 1 node enc. + 2 char + 0 * 3 branches.
        Assert.AreEqual(11, test.ToBuffer().ByteLength, "Test C");
        RoundTrip(test, "Test D");

        // Fails on 32-big Unicode characters.
        /*
        test.Clear();
        test.Add("𝄞");
        // Header + 1 node enc. + 3 char + 0 * 3 branches.
        Assert.AreEqual(12, test.ToBuffer().ByteLength, "Test E");
        RoundTrip(test, "Test F");
        */
    }

    [TestMethod]
    public void SmallSetRoundTrip()
    {
        // Arrange
        FastTernaryStringSet test = new(new List<string>(["", "apple", "ankle", "ball", "pi", "piano", "pink", "ukulele",]));

        // Act & Assert
        RoundTrip(test, "Test A");
        test.Compact();
        RoundTrip(test, "Test B");
    }

    [TestMethod]
    public async Task LargeSetRoundTrip()
    {
        // Arrange
        string[] lines = (await Common.GetResourceFileContents("short-english-list.txt"))
            .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        FastTernaryStringSet test = new(lines);

        // Act & Assert
        RoundTrip(test, "Test A");
        test.Compact();
        RoundTrip(test, "Test B");
    }

    [TestMethod]
    public async Task PreSerializedTest()
    {
        // Arrange
        string[] lines = (await Common.GetResourceFileContents("short-english-list.txt"))
            .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        FastTernaryStringSet test = new(lines);
        test.Compact();

        // Act
        string[] stringSet = (await Common.GetResourceFileContents("serialized-short-list-b64.txt"))
            .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        /*
        Debug.WriteLine("String set:");
        foreach (string line in stringSet)
        {
            Debug.WriteLine(line);
        }
        */
        TypedArray<byte> decoded = TypedArray<byte>.FromBase64StringSet(stringSet);
        FastTernaryStringSet decodedSet = new(decoded.Buffer);

        // Assert
        CollectionAssert.AreEquivalent((List<string>)test.ToList(), (List<string>)decodedSet.ToList());
    }

    protected static void RoundTrip(FastTernaryStringSet set, string testName)
    {
        /*
        RoundTripShort(set, testName);
        return;
        */
        ArrayBuffer buff = set.ToBuffer();
        IEnumerable<string> b64Encode = buff.ToBase64StringSet();
        /*
        Console.WriteLine("B64-encoded string set:");
        foreach (string str in b64Encode)
        {
            Console.WriteLine(str);
        }
        */
        TypedArray<byte> ui8decode = TypedArray<byte>.FromBase64StringSet(b64Encode);
        FastTernaryStringSet set2 = new(ui8decode.Buffer);
        AssertStatsEqual(set, set2, testName);
        CollectionAssert.AreEquivalent((List<string>)set.ToList(), (List<string>)set2.ToList(),
            $"{testName} sets equivalence test.");
    }

    protected static void RoundTripShort(FastTernaryStringSet set, string testName)
    {
        List<string> set1list = (List<string>)set.ToList();
        Console.WriteLine("Original set list:");
        foreach (string str in set1list)
        {
            Console.WriteLine(str);
        }
        Console.WriteLine("=====");
        Console.WriteLine("Original tree:");
        /*
        for (int i = 0; i < set.Tree.Count; i++)
        {
            Console.WriteLine($"[{i}]: {set.Tree[i]}");
        }
        Console.WriteLine("=====");
        */
        ArrayBuffer buff = set.ToBuffer();
        Console.WriteLine("Original buffer values:");
        for (int i = 0; i < buff.Bytes.Count; i++)
        {
            Console.WriteLine($"[{i}]: {buff.Bytes[i]}");
        }
        Console.WriteLine("=====");
        FastTernaryStringSet set2 = new(buff);
        AssertStatsEqual(set, set2, testName);
        /*
        Console.WriteLine("Restored tree:");
        for (int i = 0; i < set2.Tree.Count; i++)
        {
            Console.WriteLine($"[{i}]: {set2.Tree[i]}");
        }
        Console.WriteLine("=====");
        */
        List<string> set2list = (List<string>)set2.ToList();
        Console.WriteLine("Original set list (again):");
        foreach (string str in set1list)
        {
            Console.WriteLine(str);
        }
        Console.WriteLine("=====");
        Console.WriteLine("Restored set list:");
        foreach (string str in set2list)
        {
            Console.WriteLine(str);
        }
        Console.WriteLine("=====");
        CollectionAssert.AreEquivalent(set1list, set2list,
            $"{testName} sets equivalence test.");
    }

    protected static void AssertStatsEqual(FastTernaryStringSet a, FastTernaryStringSet b, string testName)
    {
        TernaryTreeStats aStats = a.Stats,
            bStats = b.Stats;
        Assert.AreEqual(aStats.Size, bStats.Size, $"{testName} size test.");
        Assert.AreEqual(aStats.Nodes, bStats.Nodes, $"{testName} nodes test.");
        Assert.AreEqual(aStats.IsCompact, bStats.IsCompact, $"{testName} compact test.");
        Assert.AreEqual(aStats.Depth, bStats.Depth, $"{testName} depth test.");
        CollectionAssert.AreEqual((List<int>)aStats.Breadth, (List<int>)bStats.Breadth, $"{testName} breadth test.");
        Assert.AreEqual(aStats.MinCodePoint, bStats.MinCodePoint, $"{testName} minCodePoint test.");
        Assert.AreEqual(aStats.MaxCodePoint, bStats.MaxCodePoint, $"{testName} maxCodePoint test.");
    }
}
