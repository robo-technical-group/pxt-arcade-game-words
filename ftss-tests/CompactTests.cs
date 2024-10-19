using ftss;
using System.Diagnostics.CodeAnalysis;

namespace ftss_tests
{
    [TestClass]
    public class CompactTests
    {
        private static FastTernaryStringSet? _compactWords = null;
        private static string[] _lines = [];

        [MemberNotNull(nameof(_compactWords))]
        private static void Init()
        {
            if (_compactWords is not null) { return; }
            _compactWords = [];
            _lines = TestFiles.short_english_list
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            _compactWords.AddAll(_lines);
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

        [TestMethod]
        public void CompactBasicSuffix()
        {
            // Arrange
            FastTernaryStringSet test = [];

            // Act
            test.Clear();
            test.AddAll(new string[]
            {
                "abcing", "defing", "ghiing", "jkling", "mnoing",
            });
            int preCompactSize = test.Stats.Nodes;
            test.Compact();

            // Assert
            Assert.AreEqual(3 * (test.Size - 1), preCompactSize - test.Stats.Nodes);
        }

        [TestMethod]
        public void CompactNonTrivialSuffix()
        {
            /**
             * A more complext example than the basic test.
             * In this case, each string has a unique prefix, "a" through "d,"
             * and all have a common suffix, "ing." However, two share
             * the suffix "_aing" and two share the suffix "_bing."
             * Before compaction, there is a node for each letter
             * of each string (6 chars * 4 strings) since there are
             * no shared prefixes. After compaction, we expect a node
             * each for the prefixes, three nodes for the shared "ing,"
             * two nodes for the shared "_a," and two for the "_b"
             * (4 + 3 + 2 + 2)
             */
           
            // Arrange
            string[] words = [
                "a_aing", "b_aing", "c_bing", "d_bing",
                ];
            FastTernaryStringSet test = [];

            // Act
            test.AddAll(words);

            // Assert
            Assert.AreEqual(24, test.Stats.Nodes, "Test A");
            test.Compact();
            Assert.AreEqual(11, test.Stats.Nodes, "Test B");

            /**
             * If we add "e_ai," this will add 4 nodes,
             * since the prefix, "e," is unique, and
             * "_ai" was previously an infix, not a suffix!
             */
            // Arrange
            test.Add("e_ai");

            // Act & Assert
            test.Compact();
            Assert.AreEqual(15, test.Stats.Nodes, "Test C");

            /**
             * Whereas, if we add "fng," only one more node
             * is needed (for the "f"), since the suffix,
             * "ng," is part of the existing "ing" subtree.
             */
            // Arrange
            test.Add("fng");

            // Act & Assert
            test.Compact();
            Assert.AreEqual(16, test.Stats.Nodes, "Test D");
        }

        [TestMethod]
        public void CompactDictionary()
        {
            // Arrange
            if (_compactWords is null)
            {
                Init();
            }
            FastTernaryStringSet nonCompactWords = [];
            nonCompactWords.AddAll(_lines);

            // Act & Assert
            Assert.AreEqual(_lines.Length, _compactWords.Size, "Test A");

            /**
             * Non-trivial compacted set should have fewer nodes.
             * In fact, for our word list, it is less than half as many nodes!
             */
            Console.WriteLine($"Non-compact nodes: {nonCompactWords.Stats.Nodes}; compact nodes: {_compactWords.Stats.Nodes}");
            Assert.IsTrue(_compactWords.Stats.Nodes < nonCompactWords.Stats.Nodes, "Test B");

            // But it still should contain all of the words!
            foreach (string word in _lines)
            {
                Assert.IsTrue(_compactWords.Has(word), $"Test C word {word}");
            }
        }

        [TestMethod]
        public void CompactAutoDecompact()
        {
            // Attempting to mutate a compactd set must leave it uncompacted.
            // Arrange
            FastTernaryStringSet compactOriginal = [];
            compactOriginal.AddAll(new string[]
            {
                "alligator",
                "bass",
                "crane",
                "dove",
                "eagle",
                "flea",
                "porcupine",
            });

            // Act
            compactOriginal.Compact();

            // Assert
            Assert.IsTrue(compactOriginal.Compacted, "Test A");

            // Copying a compact set yields a compact copy.
            // Arrange & Act
            FastTernaryStringSet test = new(compactOriginal);
            // Assert
            Assert.IsTrue(test.Compacted, "Test B");

            // Adding a string already in the set has no effect.
            // Act
            test.Add("alligator");
            // Assert
            Assert.IsTrue(test.Compacted, "Test C");

            // Adding a string not in the set undoes compaction.
            // Act
            test.Add("dragonfly");
            // Assert
            Assert.IsFalse(test.Compacted, "Test D");

            // As does adding via AddAll().
            // Arrange
            test = new(compactOriginal);
            // Act
            test.AddAll(new string[] { "dragonfly", });
            // Assert
            Assert.IsFalse(test.Compacted, "Test E");

            /**
             * Likewise, deleting a string not in the set has no effect,
             * while actually deleting a string undoes compaction.
             */
            // Arrange
            test = new(compactOriginal);
            // Act
            test.Delete("zedonk");
            // Assert
            Assert.IsTrue(test.Compacted, "Test F");
            // Act
            test.Delete("alligator");
            // Assert
            Assert.IsFalse(test.Compacted, "Test G");

            // Balance() undoes compaction.
            // Arrange
            test = new(compactOriginal);
            // Act
            test.Balance();
            // Assert
            Assert.IsFalse(test.Compacted, "Test H");

            // Clear() trivially resets the compaction state.
            // Arrange
            test = new(compactOriginal);
            // Act
            test.Clear();
            // Assert
            Assert.IsFalse(test.Compacted, "Test I");
        }

        [TestMethod]
        public void CompactNonMutating()
        {
            // Arrange
            FastTernaryStringSet rhs = new(new string[] { "list", "wrestle", });
            if (_compactWords is null)
            {
                Init();
            }
            FastTernaryStringSet test = new(_compactWords);

            // Act
            test.ToList();
            test.Equals(rhs);
            test.ForEach((s) => { });
            test.GetArrangementsOf("coat");
            test.GetCompletionsOf("win");
            test.GetPartialMatchesOf("cu.");
            test.GetWithinHammingDistanceOf("cat", 1);
            test.Has("cat");
            // test.IsSubsetOf(rhs);
            // test.IsSupersetOf(rhs):
            IList<string> _ = test.Keys;
            int __ = test.Size;
            _ = test.Values;

            // Assert
            Assert.IsTrue(test.Compacted);
        }
    }
}
