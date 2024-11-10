using ftss;

namespace ftss_tests
{
    [TestClass]
    public class StatsTests
    {
        [TestMethod]
        public void EmptySet()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act
            TernaryTreeStats stats = test.Stats;
            // Assert
            Assert.AreEqual((uint)0, stats.Size, "Test A");
            Assert.AreEqual(0, stats.Nodes, "Test B");
            Assert.AreEqual(0, stats.Depth, "Test C");
            Assert.AreEqual(0, stats.Breadth.Count(), "Test D");
            Assert.AreEqual<uint>(0, stats.MinCodePoint, "Test E");
            Assert.AreEqual<uint>(0, stats.MaxCodePoint, "Test F");
            Assert.AreEqual(0, stats.Surrogates, "Test G");
        }

        [TestMethod]
        public void EmptyString()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act
            test.Add(string.Empty);
            TernaryTreeStats stats = test.Stats;
            // Assert
            // Empty sring increments size but adds no nodes.
            Assert.AreEqual((uint)1, stats.Size, "Test A");
            Assert.AreEqual(0, stats.Nodes, "Test B");
            Assert.AreEqual(0, stats.Depth, "Test C");
            Assert.AreEqual(0, stats.Breadth.Count(), "Test D");
            Assert.AreEqual<uint>(0, stats.MinCodePoint, "Test E");
            Assert.AreEqual<uint>(0, stats.MaxCodePoint, "Test F");
            Assert.AreEqual(0, stats.Surrogates, "Test G");
        }

        [TestMethod]
        public void SingletonLength1()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act
            test.Add("B");
            TernaryTreeStats stats = test.Stats;
            // Assert
            // Empty sring increments size but adds no nodes.
            Assert.AreEqual((uint)1, stats.Size, "Test A");
            Assert.AreEqual(1, stats.Nodes, "Test B");
            Assert.AreEqual(1, stats.Depth, "Test C");
            Assert.AreEqual(1, stats.Breadth.Count(), "Test D");
            Assert.AreEqual(1, stats.Breadth.First(), "Test D1");
            Assert.AreEqual<uint>(66, stats.MinCodePoint, "Test E");
            Assert.AreEqual<uint>(66, stats.MaxCodePoint, "Test F");
            Assert.AreEqual(0, stats.Surrogates, "Test G");
        }

        [TestMethod]
        public void SingleLeftChild()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act
            test.Add("B");
            test.Add("A");
            TernaryTreeStats stats = test.Stats;
            // Assert
            // Empty sring increments size but adds no nodes.
            Assert.AreEqual((uint)2, stats.Size, "Test A");
            Assert.AreEqual(2, stats.Nodes, "Test B");
            Assert.AreEqual(2, stats.Depth, "Test C");
            Assert.AreEqual(2, stats.Breadth.Count(), "Test D");
            Assert.AreEqual(1, stats.Breadth.First(), "Test D1");
            Assert.AreEqual<uint>(65, stats.MinCodePoint, "Test E");
            Assert.AreEqual<uint>(66, stats.MaxCodePoint, "Test F");
            Assert.AreEqual(0, stats.Surrogates, "Test G");
        }

        [TestMethod]
        public void RootWithBothChildren()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act
            test.Add("B");
            test.Add("A");
            test.Add("C");
            TernaryTreeStats stats = test.Stats;
            // Assert
            // Empty sring increments size but adds no nodes.
            Assert.AreEqual((uint)3, stats.Size, "Test A");
            Assert.AreEqual(3, stats.Nodes, "Test B");
            Assert.AreEqual(2, stats.Depth, "Test C");
            Assert.AreEqual(2, stats.Breadth.Count(), "Test D");
            Assert.AreEqual(1, stats.Breadth.First(), "Test D1");
            Assert.AreEqual<uint>(65, stats.MinCodePoint, "Test E");
            Assert.AreEqual<uint>(67, stats.MaxCodePoint, "Test F");
            Assert.AreEqual(0, stats.Surrogates, "Test G");
        }

        [TestMethod]
        public void ThreeLevelTree()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act
            test.Add("B");
            test.Add("A");
            test.Add("C");
            test.Add("D");
            TernaryTreeStats stats = test.Stats;
            // Assert
            // Empty sring increments size but adds no nodes.
            Assert.AreEqual((uint)4, stats.Size, "Test A");
            Assert.AreEqual(4, stats.Nodes, "Test B");
            Assert.AreEqual(3, stats.Depth, "Test C");
            Assert.AreEqual(3, stats.Breadth.Count(), "Test D");
            Assert.AreEqual(1, stats.Breadth.First(), "Test D1");
            Assert.AreEqual<uint>(65, stats.MinCodePoint, "Test E");
            Assert.AreEqual<uint>(68, stats.MaxCodePoint, "Test F");
            Assert.AreEqual(0, stats.Surrogates, "Test G");
        }
    }
}
