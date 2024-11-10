using typed_arrays;
namespace ta_tests;
[TestClass]
public class DataViewTests
{
    [TestMethod]
    public void ConstructorTests()
    {
        // Arrange
        DataView d = new(new ArrayBuffer(8));

        // Act & Assert
        // Big Endian / Big Endian
        d.SetUInt32(0, 0x12345678);
        Assert.AreEqual((uint)0x12345678, d.GetUInt32(0), "Test A");

        // Little Endian / Little Endian
        d.SetUInt32(0, 0x12345678, true);
        Assert.AreEqual((uint)0x12345678, d.GetUInt32(0, true), "Test B");

        // Little Endian / Big Endian
        d.SetUInt32(0, 0x12345678, true);
        Assert.AreEqual((uint)0x78563412, d.GetUInt32(0), "Test C");

        // Big Endian / Little Endian
        d.SetUInt32(0, 0x12345678);
        Assert.AreEqual((uint)0x78563412, d.GetUInt32(0, true), "Test D");
    }

    [TestMethod]
    public void AccessorTests()
    {
        // Arrange
        TypedArray<byte> u = new(8);
        DataView d = new(u.Buffer);

        // Act & Assert
        CollectionAssert.AreEqual(new List<byte> { 0, 0, 0, 0, 0, 0, 0, 0, }, u.ToList(), "Test A");

        d.SetUInt8(0, 255);
        CollectionAssert.AreEqual(new List<byte> { 0xff, 0, 0, 0, 0, 0, 0, 0, }, u.ToList(), "Test B");

        d.SetInt8(1, -1);
        CollectionAssert.AreEqual(new List<byte> { 0xff, 0xff, 0, 0, 0, 0, 0, 0, }, u.ToList(), "Test C");

        d.SetUInt16(2, 0x1234);
        CollectionAssert.AreEqual(new List<byte> { 0xff, 0xff, 0x12, 0x34, 0, 0, 0, 0, }, u.ToList(), "Test D");

        d.SetInt16(4, -1);
        CollectionAssert.AreEqual(new List<byte> { 0xff, 0xff, 0x12, 0x34, 0xff, 0xff, 0, 0, },
            u.ToList(), "Test E");

        d.SetUInt32(1, 0x12345678);
        CollectionAssert.AreEqual(new List<byte> { 0xff, 0x12, 0x34, 0x56, 0x78, 0xff, 0, 0, },
            u.ToList(), "Test F");

        d.SetInt32(4, -2023406815);
        CollectionAssert.AreEqual(new List<byte> { 0xff, 0x12, 0x34, 0x56, 0x87, 0x65, 0x43, 0x21, },
            u.ToList(), "Test G");

        u.Set([0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87,]);
        Assert.AreEqual((byte)128, d.GetUInt8(0), "Test H");
        Assert.AreEqual((sbyte)(-127), d.GetInt8(1), "Test I");
        Assert.AreEqual((ushort)33411, d.GetUInt16(2), "Test J");
        Assert.AreEqual((short)(-31868), d.GetInt16(3), "Test K");
        Assert.AreEqual((uint)2223343239, d.GetUInt32(4), "Test L");
        Assert.AreEqual(-2105310075, d.GetInt32(2), "Test M");
    }

    [TestMethod]
    public void EndianTests()
    {
        // Arrange
        TypedArray<byte> u = new([0, 1, 2, 3, 4, 5, 6, 7,]);
        ArrayBuffer rawbuf = u.Buffer;
        DataView d;

        // Act
        d = new(rawbuf);

        // Assert
        Assert.AreEqual(8, d.ByteLength, "Test A");
        Assert.AreEqual(0, d.ByteOffset, "Test B");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => { byte _ = d.GetUInt8(-2); }, "Test C");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => { byte _ = d.GetUInt8(8); }, "Test D");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => d.SetUInt8(-2, 0), "Test E");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => d.SetUInt8(8, 0), "Test F");

        d = new(rawbuf, 2);
        Assert.AreEqual(6, d.ByteLength, "Test G");
        Assert.AreEqual(2, d.ByteOffset, "Test H");
        Assert.AreEqual((byte)7, d.GetUInt8(5), "Test I");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => { byte _ = d.GetUInt8(-2); }, "Test J");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => { byte _ = d.GetUInt8(6); }, "Test K");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => d.SetUInt8(-2, 0), "Test L");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => d.SetUInt8(6, 0), "Test M");

        d = new(rawbuf, 8);
        Assert.AreEqual(0, d.ByteLength, "Test N");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => d = new(rawbuf, -1), "Test O");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => d = new(rawbuf, 9), "Test P");

        d = new(rawbuf, 2, 4);
        Assert.AreEqual(4, d.ByteLength, "Test Q");
        Assert.AreEqual(2, d.ByteOffset, "Test R");
        Assert.AreEqual((byte)5, d.GetUInt8(3), "Test S");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => { byte _ = d.GetUInt8(-2); }, "Test T");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => { byte _ = d.GetUInt8(4); }, "Test U");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => d.SetUInt8(-2, 0), "Test V");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => d.SetUInt8(4, 0), "Test W");

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => d = new(rawbuf, 0, 9), "Test X");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => d = new(rawbuf, 8, 1), "Test Y");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => d = new(rawbuf, 9, -1), "Test Z");
    }
}
