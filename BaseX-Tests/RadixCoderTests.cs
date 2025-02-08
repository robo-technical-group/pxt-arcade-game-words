using BaseX;
using System.Numerics;
using System.Security.Cryptography;
namespace BaseX_Tests
{
    [TestClass]
    public sealed class RadixCoderTests
    {
        [TestMethod]
        public void ZeroConstructor()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                _ = new StaticRadixCoders.Uint8(0);
            }, "Test A");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                _ = new StaticRadixCoders.Uint16(0);
            }, "Test B");
        }

        [TestMethod]
        public void OneConstructor()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                _ = new StaticRadixCoders.Uint8(1);
            }, "Test A");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                _ = new StaticRadixCoders.Uint16(1);
            }, "Test B");
        }

        [TestMethod]
        public void ConstructorTooBig()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                _ = new StaticRadixCoders.Uint8(64).Decode([64,]);
            }, "Test A");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                _ = new StaticRadixCoders.Uint8(200).Decode([0xff,]);
            }, "Test B");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                _ = new StaticRadixCoders.Uint16(64).Decode([64,]);
            }, "Test C");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                _ = new StaticRadixCoders.Uint16(30000).Decode([0xFFFF,]);
            }, "Test D");
        }

        private static void Invert<T>(RadixCoder<T> coder, byte[] bytes, string testName) where T : IBinaryInteger<T>
        {
            CollectionAssert.AreEquivalent(bytes, coder.Decode(coder.Encode(bytes)), testName);
        }

        [TestMethod]
        public void InvertZeroFilled()
        {
            Random r = new();
            for (int u8 = 2; u8 <= 256; u8++)
            {
                StaticRadixCoders.Uint8 coder = new(u8);
                for (int i = 0; i <= 65; i++)
                {
                    Invert(coder, new byte[i], $"U8 test {u8} pass {i}.");
                }
            }
            for (int u16 = 2; u16 <= 0x10000; u16 += 1 + r.Next(200))
            {
                StaticRadixCoders.Uint16 coder = new(u16);
                for (int i = 0; i <= 65; i++)
                {
                    Invert(coder, new byte[i], $"U16 test {u16} pass {i}.");
                }
            }
        }

        [TestMethod]
        public void InvertRandom()
        {
            Random r = new();
            for (int u8 = 2; u8 <= 256; u8++)
            {
                StaticRadixCoders.Uint8 coder = new(u8);
                for (int i = 0; i < 5; i++)
                {
                    byte[] randomBytes = new byte[2 + r.Next(300)];
                    RandomNumberGenerator.Fill(randomBytes);
                    Invert(coder, randomBytes, $"U8 test {u8} pass {i}.");
                }
            }
            for (int u16 = 2; u16 <= 0x10000; u16 += 1 + r.Next(200))
            {
                StaticRadixCoders.Uint16 coder = new(u16);
                for (int i = 0; i < 5; i++)
                {
                    byte[] randomBytes = new byte[2 + r.Next(300)];
                    RandomNumberGenerator.Fill(randomBytes);
                    Invert(coder, randomBytes, $"U16 test {u16} pass {i}.");
                }
            }
        }
    }
}
