using NuGet.Frameworks;
using typed_arrays;

namespace ta_tests
{
    [TestClass]
    public class TypedArrayTests
    {
        private static readonly TypedArray<byte> uint8 = new (new List<byte>([0, 1, 2, 3, 4, 5, 6, 7,]));
        private static readonly ArrayBuffer rawbuf = uint8.Buffer;

        [TestMethod]
        public void ConstructorTest1()
        {
            // Arrange
            TypedArray<sbyte> a;

            // Act
            a = new(new List<sbyte>([1, 2, 3, 4, 5, 6, 7, 8,]));

            // Assert
            Assert.AreEqual(1, a.BytesPerElement, "Test A");
            Assert.AreEqual(0, a.ByteOffset, "Test B");
            Assert.AreEqual(8, a.ByteLength, "Test C");
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            // Arrange & Act
            TypedArray<byte> a = new(new List<byte>([1, 2, 3, 4, 5, 6, 7, 8,]));

            // Assert
            Assert.AreEqual(1, a.BytesPerElement, "Test A");
            Assert.AreEqual(0, a.ByteOffset, "Test B");
            Assert.AreEqual(8, a.ByteLength, "Test C");
        }

        [TestMethod]
        public void ConstructorTest3()
        {
            // Arrange & Act
            TypedArray<Int16> a = new(new List<Int16>([1, 2, 3, 4, 5, 6, 7, 8,]));

            // Assert
            Assert.AreEqual(2, a.BytesPerElement, "Test A");
            Assert.AreEqual(0, a.ByteOffset, "Test B");
            Assert.AreEqual(16, a.ByteLength, "Test C");
        }

        [TestMethod]
        public void ConstructorTest4()
        {
            // Arrange & Act
            TypedArray<UInt16> a = new(new List<UInt16>([1, 2, 3, 4, 5, 6, 7, 8,]));

            // Assert
            Assert.AreEqual(2, a.BytesPerElement, "Test A");
            Assert.AreEqual(0, a.ByteOffset, "Test B");
            Assert.AreEqual(16, a.ByteLength, "Test C");
        }

        [TestMethod]
        public void ConstructorTest5()
        {
            // Arrange & Act
            TypedArray<int> a = new(new List<int>([1, 2, 3, 4, 5, 6, 7, 8,]));

            // Assert
            Assert.AreEqual(4, a.BytesPerElement, "Test A");
            Assert.AreEqual(0, a.ByteOffset, "Test B");
            Assert.AreEqual(32, a.ByteLength, "Test C");
        }

        [TestMethod]
        public void ConstructorTest6()
        {
            // Arrange & Act
            TypedArray<uint> a = new(new List<uint>([1, 2, 3, 4, 5, 6, 7, 8,]));

            // Assert
            Assert.AreEqual(4, a.BytesPerElement, "Test A");
            Assert.AreEqual(0, a.ByteOffset, "Test B");
            Assert.AreEqual(32, a.ByteLength, "Test C");
        }

        [TestMethod]
        public void ConstructorTest7()
        {
            CollectionAssert.AreEqual(new List<sbyte>([0, 0, 0,]), new TypedArray<sbyte>(3).ToList());
        }

        [TestMethod]
        public void ConstructorTest8()
        {
            // Arrange & Act
            TypedArray<sbyte> int8 = new();

            // Assert
            Assert.AreEqual(0, int8.Length);
        }

        [TestMethod]
        public void ConstructorTest9()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                TypedArray<sbyte> _ = new(-1);
            });
        }

        [TestMethod]
        public void ConstructorTest10()
        {
            // Arrange & Act
            TypedArray<sbyte> int8 = new(4);

            // Assert
            Assert.AreEqual(1, int8.BytesPerElement, "Test A");
            Assert.AreEqual(4, int8.Length, "Test B");
            Assert.AreEqual(4, int8.ByteLength, "Test C");
            Assert.AreEqual(0, int8.ByteOffset, "Test D");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { sbyte _ = int8.Get(-1); }, "Test E");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { sbyte _ = int8.Get(4); }, "Test F");
        }

        [TestMethod]
        public void ConstructorTest11()
        {
            // Arrange & Act
            TypedArray<sbyte> int8 = new(new List<sbyte>([1, 2, 3, 4, 5, 6,]));

            // Assert
            Assert.AreEqual(6, int8.Length, "Test A");
            Assert.AreEqual(6, int8.ByteLength, "Test B");
            Assert.AreEqual(0, int8.ByteOffset, "Test C");
            Assert.AreEqual(4, int8.Get(3), "Test D");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { sbyte _ = int8.Get(-1); }, "Test E");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { sbyte _ = int8.Get(6); }, "Test F");
        }

        [TestMethod]
        public void ConstructorTest12()
        {
            // Arrange & Act
            TypedArray<sbyte> int8 = new(rawbuf);

            // Assert
            Assert.AreEqual(8, int8.Length, "Test A");
            Assert.AreEqual(8, int8.ByteLength, "Test B");
            Assert.AreEqual(0, int8.ByteOffset, "Test C");
            Assert.AreEqual(0, int8.Get(0), "Test D");
            Assert.AreEqual(7, int8.Get(7), "Test E");

            // Act
            int8.Set(new List<sbyte>([111,]));

            // Assert
            Assert.AreEqual(111, int8.Get(0), "Test F");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { sbyte _ = int8.Get(-1); }, "Test G");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { sbyte _ = int8.Get(8); }, "Test H");
        }

        [TestMethod]
        public void ConstructorTest13()
        {
            // Arrange & Act
            TypedArray<sbyte> int8 = new(rawbuf, 2);

            // Assert
            Assert.AreEqual(6, int8.Length, "Test A");
            Assert.AreEqual(6, int8.ByteLength, "Test B");
            Assert.AreEqual(2, int8.ByteOffset, "Test C");
            // Assert.AreEqual(2, int8.Get(0), "Test D"); // Not guaranteed.
            Assert.AreEqual(7, int8.Get(5), "Test E");

            // Act
            int8.Set(new List<sbyte>([112,]));

            // Assert
            Assert.AreEqual((sbyte)112, int8.Get(0), "Test F");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { sbyte _ = int8.Get(-1); }, "Test G");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { sbyte _ = int8.Get(6); }, "Test H");
        }

        [TestMethod]
        public void ConstructorTest14()
        {
            // Arrange & Act
            TypedArray<sbyte> int8 = new(rawbuf, 8);

            // Assert
            Assert.AreEqual(0, int8.Length);
        }

        [TestMethod]
        public void ConstructorTest15()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { TypedArray<sbyte> _ = new(rawbuf, -1); }, "Test A");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { TypedArray<sbyte> _ = new(rawbuf, 9); }, "Test B");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { TypedArray<int> _ = new(rawbuf, -1); }, "Test C");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { TypedArray<int> _ = new(rawbuf, 5); }, "Test D");
        }

        [TestMethod]
        public void ConstructorTest16()
        {
            // Arrange & Act
            TypedArray<sbyte> int8 = new(rawbuf, 2, 4);

            // Assert
            Assert.AreEqual(4, int8.Length, "Test A");
            Assert.AreEqual(4, int8.ByteLength, "Test B");
            Assert.AreEqual(2, int8.ByteOffset, "Test C");
            // Assert.AreEqual(0, int8.Get(0), "Test D"); // Not guaranteed.
            Assert.AreEqual(5, int8.Get(3), "Test E");

            // Act
            int8.Set(new List<sbyte>([113,]));

            // Assert
            Assert.AreEqual(113, int8.Get(0), "Test F");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { sbyte _ = int8.Get(-1); }, "Test G");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { sbyte _ = int8.Get(4); }, "Test H");
        }

        [TestMethod]
        public void ConstructorTest17()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { TypedArray<sbyte> _ = new(rawbuf, 0, 9); }, "Test A");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { TypedArray<sbyte> _ = new(rawbuf, 8, 1); }, "Test B");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { TypedArray<sbyte> _ = new(rawbuf, 9, -1); }, "Test C");
        }

        [TestMethod]
        public void CloneTests()
        {
            // Arrange
            List<int> intSrc = new([1, 2, 3, 4, 5, 6, 7, 8,]);

            // Act
            TypedArray<int> src = new(intSrc);
            TypedArray<int> dst = TypedArray<int>.FromTypedArray(src);

            // Assert
            CollectionAssert.AreEqual(intSrc, dst.ToList(), "Test A");

            // Act
            src.Set(new List<int>([99,]));

            // Assert
            CollectionAssert.AreEqual(new List<int>([99, 2, 3, 4, 5, 6, 7, 8,]), src.ToList(), "Test B");
            CollectionAssert.AreEqual(intSrc, dst.ToList(), "Test C");
        }

        [TestMethod]
        public void Conversions()
        {
            // Arrange
            TypedArray<byte> uint8 = new(new List<byte>([1, 2, 3, 4,]));

            // Act
            TypedArray<UInt16> uint16 = new(uint8.Buffer);
            TypedArray<uint> uint32 = new(uint8.Buffer);

            // Assert
            CollectionAssert.AreEqual(new List<byte>([1, 2, 3, 4,]), uint8.ToList(), "Test A");

            // Act
            uint16.Set([0xffff,]);

            // Assert
            CollectionAssert.AreEqual(new List<byte>([0xff, 0xff, 3, 4,]), uint8.ToList(), "Test B");

            // Act
            uint16.Set([0xeeee,], 1);

            // Assert
            CollectionAssert.AreEqual(new List<byte>([0xff, 0xff, 0xee, 0xee,]), uint8.ToList(), "Test C");

            // Act
            uint32.Set([0x11111111,]);

            // Assert
            Assert.AreEqual(0x1111, uint16.Get(0), "Test D");
            Assert.AreEqual(0x1111, uint16.Get(1), "Test E");
            CollectionAssert.AreEqual(new List<byte>([0x11, 0x11, 0x11, 0x11,]), uint8.ToList(), "Test F");
        }

        [TestMethod]
        public void SignedConversions()
        {
            // Arrange
            TypedArray<sbyte> int8 = new(1);

            TypedArray<byte> uint8 = new(int8.Buffer);
            uint8.Set([123,]);
            Assert.AreEqual(123, int8.Get(0));

            uint8.Set([161,]);
            Assert.AreEqual<sbyte>(-95, int8.Get(0));

            int8.Set([-120,]);
            Assert.AreEqual(136, uint8.Get(0));

            int8.Set([-1,]);
            Assert.AreEqual(0xff, uint8.Get(0));

            // Arrange
            TypedArray<Int16> int16 = new(1);
            
            TypedArray<UInt16> uint16 = new(int16.Buffer);
            uint16.Set([3210,]);
            Assert.AreEqual(3210, int16.Get(0));

            uint16.Set([49232,]);
            Assert.AreEqual(-16304, int16.Get(0));

            int16.Set([-16384,]);
            Assert.AreEqual(49152, uint16.Get(0));

            int16.Set([-1,]);
            Assert.AreEqual(0xffff, uint16.Get(0));

            // Arrange
            TypedArray<int> int32 = new(1);

            TypedArray<uint> uint32 = new(int32.Buffer);
            uint32.Set([0x80706050,]);
            Assert.AreEqual(-2140118960, int32.Get(0));

            int32.Set([-2023406815,]);
            Assert.AreEqual(0x87654321, uint32.Get(0));

            int32.Set([-1,]);
            Assert.AreEqual(0xffffffff, uint32.Get(0));
        }
    }
}