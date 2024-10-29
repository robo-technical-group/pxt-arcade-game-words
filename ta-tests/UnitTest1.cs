namespace ta_tests
{
    [TestClass]
    public class TypedArrayTests
    {
        [TestMethod]
        public void ConstructorTest1()
        {
            // Arrange
            TypedArray a;

            // Act
            a = new Int8Array(new int[] {1, 2, 3, 4, 5, 6, 7, 8,});

            // Assert
            Assert.AreEqual(1, a.BytesPerElement, "Test A");
            Assert.AreEqual(0, a.ByteOffset, "Test B");
            Assert.AreEqual(8, a.ByteLength, "Test C");
        }
    }
}