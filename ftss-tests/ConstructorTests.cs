using ftss;

namespace ftss_tests
{
    [TestClass]
    public class ConstructorTests
    {
        [TestMethod]
        public void TestDefault()
        {
            // Arrange
            // Act
            FastTernaryStringSet test = [];
            // Assert
            int size = test.Size;
            Assert.AreEqual(0, size);
        }

        [TestMethod]
        public void TestEmpty()
        {
            // Arrange
            // Act
            FastTernaryStringSet test = new([]);
            // Assert
            int size = test.Size;
            Assert.AreEqual(0, size);
        }
    }
}