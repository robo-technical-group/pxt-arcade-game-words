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
            Assert.AreEqual(0, stats.Size, "Test A");
            Assert.AreEqual(0, stats.Nodes, "Test B");
            Assert.AreEqual(0, stats.Depth, "Test C");
            Assert.AreEqual(0, stats.Breadth.Length, "Test D");
            Assert.AreEqual(0, stats.MinCodePoint, "Test E");
            Assert.AreEqual(0, stats.MaxCodePoint, "Test F");
            Assert.AreEqual(0, stats.Surrogates, "Test G");
        }
    }
}
