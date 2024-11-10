using ftss;

namespace ftss_tests;
[TestClass]
public class GetTests
{
    [TestMethod]
    public void EmptySet()
    {
        // Arrange
        FastTernaryStringSet test = [];

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.Get(-1), "Test A");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.Get(0), "Test B");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.Get(1), "Test C");
    }

    [TestMethod]
    public void EmptyString()
    {
        // Arrange
        FastTernaryStringSet test = [];
        test.Add(String.Empty);

        // Act
        string s = test.Get(0);

        // Assert
        Assert.AreEqual(s, string.Empty);
    }

    [TestMethod]
    public async Task GetRandomWords()
    {
        // Arrange
        FastTernaryStringSet test = [];
        string[] lines = (await Common.GetResourceFileContents("short-english-list.txt"))
            .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        test.AddAll(lines);

        // Act & Assert
        Console.WriteLine("Getting random words from short word list.");
        for (int i = 0; i < 100;  i++)
        {
            int index = Random.Shared.Next(0, lines.Length - 1);
            string word = test[index];
            Console.WriteLine(word);
            Assert.AreNotEqual(0, word.Length, $"Test {i}");
        }
    }
}
