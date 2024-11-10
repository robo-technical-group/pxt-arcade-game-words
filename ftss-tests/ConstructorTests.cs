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
            uint size = test.Size;
            Assert.AreEqual((uint)0, size);
        }

        [TestMethod]
        public void TestEmpty()
        {
            // Arrange
            // Act
            FastTernaryStringSet test = new((string[])[]);
            // Assert
            uint size = test.Size;
            Assert.AreEqual((uint)0, size);
        }
    }
}