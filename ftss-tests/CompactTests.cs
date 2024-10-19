using ftss;

namespace ftss_tests
{
    [TestClass]
    public class CompactTests
    {
        private static FastTernaryStringSet? _compactWords = null;

        private static void Init()
        {
            if (_compactWords is not null) { return; }
            _compactWords = [];
            string[] lines = TestFiles.short_english_list
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            _compactWords.AddAll(lines);
            _compactWords.Compact();
        }

        [TestMethod]
        public void CompactMatchesState()
        {
            // Arrange
            FastTernaryStringSet test = [];

            // Act & Assert
            // New set is not compacted.
            Assert.IsFalse(test.Compacted);
            test.Compact();
            // Empty sets are not compacted.
            Assert.IsFalse(test.Compacted);
            test.AddAll(new string[] { "add", "bad", "mad", });
            test.Compact();
            Assert.IsTrue(test.Compacted);
        }

        [TestMethod]
        public void CompactEmptySet()
        {
            // Arrange
            FastTernaryStringSet test = [];

            // Act & Assert
            // Compacting an empty set or a set with only the empty string
            // has no effect since these sets have no tree.
            test.Compact();
            Assert.AreEqual(0, test.Stats.Nodes);
            test.Add(string.Empty);
            test.Compact();
            Assert.AreEqual(0, test.Stats.Nodes);
            Assert.IsTrue(test.Has(""));
        }

        [TestMethod]
        public void CompactTrivialNoDups()
        {
            // Arrange
            FastTernaryStringSet test = [];
            test.Add("A");

            // Act & Assert

            // For compaction to have an effect,
            // there must be strings with common suffixes.
            test.Compact();
            Assert.AreEqual(1, test.Stats.Nodes);
            Assert.IsTrue(test.Has("A"));
            test.Add("B");
            test.Compact();
            Assert.AreEqual(2, test.Stats.Nodes);
            Assert.IsTrue(test.Has("A"));
            Assert.IsTrue(test.Has("B"));
        }
    }
}
