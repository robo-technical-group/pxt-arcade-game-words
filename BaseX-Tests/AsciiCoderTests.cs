using BaseX;
using System.Security.Cryptography;
namespace BaseX_Tests
{
    [TestClass]
    public class AsciiCoderTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                _ = AsciiRadixCoder.Of("");
            });
        }

        [TestMethod]
        public void OneCharConstructor()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                _ = AsciiRadixCoder.Of("A");
            });
        }

        [TestMethod]
        public void RepeatedCharacterConstructor()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                _ = AsciiRadixCoder.Of("01234567980");
            });
        }

        [TestMethod]
        public void InvalidCharacterDecode()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                _ = AsciiRadixCoder.Of("01").Decode("011100X01101");
            });
        }

        private void Invert(AsciiRadixCoder coder, byte[] bytes, string testName)
        {
            CollectionAssert.AreEquivalent(bytes, coder.Decode(coder.Encode(bytes)), testName);
        }

        private static AsciiRadixCoder CreateWithBase(int b)
        {
            byte[] cs = new byte[b];
            for (int i = 0; i < b; i++)
            {
                cs[i] = (byte)i;
            }
            return AsciiRadixCoder.Of(System.Text.Encoding.ASCII.GetString(cs));
        }

        [TestMethod]
        public void InvertZeroFilled()
        {
            for (int b = 2; b <= 128; b++)
            {
                AsciiRadixCoder coder = CreateWithBase(b);
                for (int i = 0; i <= 65; i++)
                {
                    Invert(coder, new byte[i], $"Test base {b} pass {i}.");
                }
            }
        }

        [TestMethod]
        public void InvertRandom()
        {
            Random r = new();
            for (int b = 2; b <= 128; b++)
            {
                AsciiRadixCoder coder = CreateWithBase(b);
                for (int i = 0; i < 10; i++)
                {
                    byte[] randomBytes = new byte[2 + r.Next(300)];
                    RandomNumberGenerator.Fill(randomBytes);
                    Invert(coder, randomBytes, $"Test base {b} pass {i}.");
                }
            }
        }
    }
}
