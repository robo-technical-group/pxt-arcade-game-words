﻿using typed_arrays;

namespace ta_tests
{
    [TestClass]
    public class ArrayBufferTests
    {
        [TestMethod]
        public void ConstructionZeroLength()
        {
            // Arrange
            ArrayBuffer b = new();

            // Act & Assert
            Assert.AreEqual(0, b.ByteLength);
        }

        [TestMethod]
        public void ConstructionByteLengths()
        {
            // Arrange
            int[] lengths = { 0, 1, 123, };
            for (int test = 0; test < lengths.Length; test++)
            {
                // Act
                int length = lengths[test];
                ArrayBuffer b = new(length);

                // Assert
                Assert.AreEqual(length, b.ByteLength, $"Test {length}.");
            }
        }

        [TestMethod]
        public void ConstructionBadLengths()
        {
            ArrayBuffer b;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => b = new(-1));
        }

        [TestMethod]
        public void SliceTests()
        {
            // Arrange
            ArrayBuffer buf = Create(new List<uint>([ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, ]));

            // Act & Assert
            Assert.AreEqual(10, buf.ByteLength, "Test A");
            Assert.AreEqual(5, buf.Slice(5).ByteLength);
            Assert.AreEqual(2, buf.Slice(-2).ByteLength);
            Assert.AreEqual(2, buf.Slice(-4, -2).ByteLength);
            Assert.AreEqual(5, buf.Slice(-1000, 5).ByteLength);
            CollectionAssert.AreEqual(new uint[] { 5, 6, 7, 8, 9, }, (List<uint>)buf.Slice(5).ToList());
            CollectionAssert.AreEqual(new uint[] {0, 1, 2, 3, 4, }, (List<uint>)buf.Slice(0, 5).ToList());
            CollectionAssert.AreEqual(new uint[] { 5, 6, }, (List<uint>)buf.Slice(5, 7).ToList());
            CollectionAssert.AreEqual(new uint[] { 6, 7, }, (List<uint>)buf.Slice(-4, -2).ToList());
            CollectionAssert.AreEqual(new uint[] { 2, 3, 4, 5, 6, 7, }, (List<uint>)buf.Slice(2, -2).ToList());
        }

        protected ArrayBuffer Create(IList<uint> bytes)
        {
            ArrayBuffer buffer = new ArrayBuffer(bytes.Count);
            TypedArray<byte> array = new(buffer);
            for (int i = 0; i < bytes.Count; i++)
            {
                array.Set(i, bytes[i]);
            }
            return buffer;
        }
    }
}
