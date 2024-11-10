using ftss;

namespace ftss_tests
{
    [TestClass]
    public class BalanceTests
    {
        [TestMethod]
        public async Task BalanceBadStructure()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] lines = (await Common.GetResourceFileContents("short-english-list.txt"))
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // Add words in worst possible order
            foreach(string line in lines)
            {
                test.Add(line);
            }
            TernaryTreeStats badStats = test.Stats;

            // Act
            test.Balance();
            TernaryTreeStats goodStats = test.Stats;

            // Assert
            CollectionAssert.AreEquivalent(lines, (List<string>)test.ToList());
            Console.WriteLine($"Bad depth: {badStats.Depth}. Good depth: {goodStats.Depth}");
            Assert.IsTrue(badStats.Depth > goodStats.Depth);
            Assert.IsFalse(test.Has(string.Empty));
        }

        [TestMethod]
        public void BalancePreserves()
        {
            // Arrange
            string[] words = [
                "",
                "a",
                "ape",
                "aphid",
                "aphids",
                "bee",
                "bees",
                "cat",
                "cats",
            ];
            FastTernaryStringSet test = [];
            // Add words in worst possible order.
            foreach (string word in words)
            {
                test.Add(word);
            }

            // Act
            test.Balance();

            // Assert
            CollectionAssert.AreEquivalent(words, (List<string>)test.ToList());
        }

        [TestMethod]
        public void BalanceEmpty()
        {
            // Arrange
            FastTernaryStringSet test = [];
            try
            {
                test.Balance();
                Assert.AreEqual<uint>(0, test.Size);
                test.Add(string.Empty);
                test.Balance();
                Assert.AreEqual((uint)1, test.Size);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
