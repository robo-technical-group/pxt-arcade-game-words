using NuGet.Frameworks;
using typed_arrays;

namespace ta_tests
{
    [TestClass]
    public class TypedArrayTests
    {
        private static Uint8Array uint8 = new (new List<int>([0, 1, 2, 3, 4, 5, 6, 7,]));
        private static ArrayBuffer rawbuf = uint8.Buffer;

        [TestMethod]
        public void ConstructorTest1()
        {
            // Arrange
            TypedArray a;

            // Act
            a = new Int8Array(new List<int>([1, 2, 3, 4, 5, 6, 7, 8,]));

            // Assert
            Assert.AreEqual(1, a.BytesPerElement, "Test A");
            Assert.AreEqual(0, a.ByteOffset, "Test B");
            Assert.AreEqual(8, a.ByteLength, "Test C");
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            // Arrange & Act
            Uint8Array a = new(new List<int>([1, 2, 3, 4, 5, 6, 7, 8,]));

            // Assert
            Assert.AreEqual(1, a.BytesPerElement, "Test A");
            Assert.AreEqual(0, a.ByteOffset, "Test B");
            Assert.AreEqual(8, a.ByteLength, "Test C");
        }

        [TestMethod]
        public void ConstructorTest3()
        {
            // Arrange & Act
            Int16Array a = new(new List<int>([1, 2, 3, 4, 5, 6, 7, 8,]));

            // Assert
            Assert.AreEqual(2, a.BytesPerElement, "Test A");
            Assert.AreEqual(0, a.ByteOffset, "Test B");
            Assert.AreEqual(16, a.ByteLength, "Test C");
        }

        [TestMethod]
        public void ConstructorTest4()
        {
            // Arrange & Act
            Uint16Array a = new(new List<int>([1, 2, 3, 4, 5, 6, 7, 8,]));

            // Assert
            Assert.AreEqual(2, a.BytesPerElement, "Test A");
            Assert.AreEqual(0, a.ByteOffset, "Test B");
            Assert.AreEqual(16, a.ByteLength, "Test C");
        }

        [TestMethod]
        public void ConstructorTest5()
        {
            // Arrange & Act
            Int32Array a = new(new List<int>([1, 2, 3, 4, 5, 6, 7, 8,]));

            // Assert
            Assert.AreEqual(4, a.BytesPerElement, "Test A");
            Assert.AreEqual(0, a.ByteOffset, "Test B");
            Assert.AreEqual(32, a.ByteLength, "Test C");
        }

        [TestMethod]
        public void ConstructorTest6()
        {
            // Arrange & Act
            Uint32Array a = new(new List<int>([1, 2, 3, 4, 5, 6, 7, 8,]));

            // Assert
            Assert.AreEqual(4, a.BytesPerElement, "Test A");
            Assert.AreEqual(0, a.ByteOffset, "Test B");
            Assert.AreEqual(32, a.ByteLength, "Test C");
        }

        [TestMethod]
        public void ConstructorTest7()
        {
            CollectionAssert.AreEqual(new List<int>([0, 0, 0,]), (List<int>)(new Int8Array(3).ToList()));
        }

        [TestMethod]
        public void ConstructorTest8()
        {
            // Arrange & Act
            Int8Array int8 = new();

            // Assert
            Assert.AreEqual(0, int8.Length);
        }

        [TestMethod]
        public void ConstructorTest9()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                Int8Array _ = new(-1);
            });
        }

        [TestMethod]
        public void ConstructorTest10()
        {
            // Arrange & Act
            Int8Array int8 = new(4);

            // Assert
            Assert.AreEqual(1, int8.BytesPerElement, "Test A");
            Assert.AreEqual(4, int8.Length, "Test B");
            Assert.AreEqual(4, int8.ByteLength, "Test C");
            Assert.AreEqual(0, int8.ByteOffset, "Test D");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { int _ = int8.Get(-1); }, "Test E");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { int _ = int8.Get(4); }, "Test F");
        }

        [TestMethod]
        public void ConstructorTest11()
        {
            // Arrange & Act
            Int8Array int8 = new(new List<int>([1, 2, 3, 4, 5, 6,]));

            // Assert
            Assert.AreEqual(6, int8.Length, "Test A");
            Assert.AreEqual(6, int8.ByteLength, "Test B");
            Assert.AreEqual(0, int8.ByteOffset, "Test C");
            Assert.AreEqual(4, int8.Get(3), "Test D");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { int _ = int8.Get(-1); }, "Test E");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { int _ = int8.Get(6); }, "Test F");
        }

        [TestMethod]
        public void ConstructorTest12()
        {
            // Arrange & Act
            Int8Array int8 = new(rawbuf);

            // Assert
            Assert.AreEqual(8, int8.Length, "Test A");
            Assert.AreEqual(8, int8.ByteLength, "Test B");
            Assert.AreEqual(0, int8.ByteOffset, "Test C");
            Assert.AreEqual(0, int8.Get(0), "Test D");
            Assert.AreEqual(7, int8.Get(7), "Test E");

            // Act
            int8.Set(new List<int>([111,]));

            // Assert
            Assert.AreEqual(111, int8.Get(0), "Test F");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { int _ = int8.Get(-1); }, "Test G");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { int _ = int8.Get(8); }, "Test H");
        }

        [TestMethod]
        public void ConstructorTest13()
        {
            // Arrange & Act
            Int8Array int8 = new(rawbuf, 2);

            // Assert
            Assert.AreEqual(6, int8.Length, "Test A");
            Assert.AreEqual(6, int8.ByteLength, "Test B");
            Assert.AreEqual(2, int8.ByteOffset, "Test C");
            Assert.AreEqual(0, int8.Get(0), "Test D");
            Assert.AreEqual(7, int8.Get(5), "Test E");

            // Act
            int8.Set(new List<int>([112,]));

            // Assert
            Assert.AreEqual(112, int8.Get(0), "Test F");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { int _ = int8.Get(-1); }, "Test G");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { int _ = int8.Get(6); }, "Test H");
        }

        [TestMethod]
        public void ConstructorTest14()
        {
            // Arrange & Act
            Int8Array int8 = new(rawbuf, 8);

            // Assert
            Assert.AreEqual(0, int8.Length);
        }

        [TestMethod]
        public void ConstructorTest15()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { Int8Array _ = new(rawbuf, -1); }, "Test A");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { Int8Array _ = new(rawbuf, 9); }, "Test B");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { Int32Array _ = new(rawbuf, -1); }, "Test C");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { Int32Array _ = new(rawbuf, 5); }, "Test D");
        }

        [TestMethod]
        public void ConstructorTest16()
        {
            // Arrange & Act
            Int8Array int8 = new(rawbuf, 2, 4);

            // Assert
            Assert.AreEqual(4, int8.Length, "Test A");
            Assert.AreEqual(4, int8.ByteLength, "Test B");
            Assert.AreEqual(2, int8.ByteOffset, "Test C");
            Assert.AreEqual(0, int8.Get(0), "Test D");
            Assert.AreEqual(5, int8.Get(3), "Test E");

            // Act
            int8.Set(new List<int>([113,]));

            // Assert
            Assert.AreEqual(113, int8.Get(0), "Test F");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { int _ = int8.Get(-1); }, "Test G");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { int _ = int8.Get(4); }, "Test H");
        }

        [TestMethod]
        public void ConstructorTest17()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { Int8Array _ = new(rawbuf, 0, 9); }, "Test A");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { Int8Array _ = new(rawbuf, 8, 1); }, "Test B");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { Int8Array _ = new(rawbuf, 9, -1); }, "Test C");
        }

        [TestMethod]
        public void CloneTests()
        {
            // Arrange
            List<int> intSrc = new List<int>([1, 2, 3, 4, 5, 6, 7, 8,]);

            // Act
            Int32Array src = new(intSrc);
            Int32Array dst = new(src);

            // Assert
            CollectionAssert.AreEqual(intSrc, (List<int>)dst.ToList(), "Test A");

            // Act
            src.Set(new List<int>([99,]));

            // Assert
            CollectionAssert.AreEqual(new List<int>([99, 2, 3, 4, 5, 6, 7, 8,]), (List<int>)src.ToList(), "Test B");
            CollectionAssert.AreEqual(intSrc, (List<int>)dst.ToList(), "Test C");
        }

        [TestMethod]
        public void Conversions()
        {
            // Arrange
            Uint8Array uint8 = new(new List<int>([1, 2, 3, 4,]));

            // Act
            Uint16Array uint16 = new(uint8.Buffer);
            Uint32Array uint32 = new(uint8.Buffer);

            // Assert
            CollectionAssert.AreEqual(new List<int>([1, 2, 3, 4,]), (List<int>)uint8.ToList(), "Test A");

            // Act
            uint16.Set([0xffff,]);

            // Assert
            CollectionAssert.AreEqual(new List<int>([0xff, 0xff, 3, 4,]), (List<int>)uint8.ToList(), "Test B");

            // Act
            uint16.Set([0xeeee,], 1);

            // Assert
            CollectionAssert.AreEqual(new List<int>([0xff, 0xff, 0xee, 0xee,]), (List<int>)uint8.ToList(), "Test C");

            // Act
            uint32.Set([0x11111111,]);

            // Assert
            Assert.AreEqual(0x1111, uint16.Get(0), "Test D");
            Assert.AreEqual(0x1111, uint16.Get(1), "Test E");
            CollectionAssert.AreEqual(new List<int>([0x11, 0x11, 0x11, 0x11,]), (List<int>)uint8.ToList(), "Test F");
        }

        [TestMethod]
        public void SignedConversions()
        {
            // Arrange
            Int8Array int8 = new(1);

            Uint8Array uint8 = new(int8.Buffer);
            uint8.Set([123,]);
            Assert.AreEqual(123, int8.Get(0));

            uint8.Set([161,]);
            Assert.AreEqual(-95, int8.Get(0));

            int8.Set([-120,]);
            Assert.AreEqual(136, uint8.Get(0));

            int8.Set([-1,]);
            Assert.AreEqual(0xff, uint8.Get(0));

            // Arrange
            Int16Array int16 = new(1);
            
            Uint16Array uint16 = new(int16.Buffer);
            uint16.Set([3210,]);
            Assert.AreEqual(3210, int16.Get(0));

            uint16.Set([49232,]);
            Assert.AreEqual(-16304, int16.Get(0));

            int16.Set([-16384,]);
            Assert.AreEqual(49152, uint16.Get(0));

            int16.Set([-1,]);
            Assert.AreEqual(0xffff, uint16.Get(0));

            // Arrange
            Int32Array int32 = new(1);

            Uint32Array uint32 = new(int32.Buffer);
            uint32.Set([0x80706050,]);
            Assert.AreEqual(-2140118960, int32.Get(0));

            int32.Set([-2023406815,]);
            Assert.AreEqual(0x87654321, uint32.Get(0));

            int32.Set([-1,]);
            Assert.AreEqual(0xffffffff, uint32.Get(0));
        }
    }
}