using ftss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ftss_tests
{
    [TestClass]
    public class DeleteTests
    {
        [TestMethod]
        public void DeleteEmptyString()
        {
            // Arrange
            FastTernaryStringSet test = [];
            test.Add(string.Empty);
            test.Add("horse");
            // Act & Assert
            Assert.AreEqual(2, test.Size, "Test A");
            Assert.IsTrue(test.Has(""), "Test B");
            test.Delete("");
            Assert.AreEqual(1, test.Size, "Test C");
            Assert.IsFalse(test.Has(""), "Test D");
        }

        [TestMethod]
        public void DeleteNonMember()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.AreEqual(0, test.Size, "Test A");
            test.Add("dog");
            Assert.AreEqual(1, test.Size, "Test B");
            Assert.IsFalse(test.Has("cat"), "Test C");
            Assert.IsFalse(test.Delete("cat"), "Test D");
            Assert.AreEqual(1, test.Size, "Test E");
        }

        [TestMethod]
        public void DeleteMember()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.AreEqual(0, test.Size, "Test A");
            test.Add("dog");
            Assert.AreEqual(1, test.Size, "Test B");
            Assert.IsTrue(test.Has("dog"), "Test C");
            Assert.IsTrue(test.Delete("dog"), "Test D");
            Assert.AreEqual(0, test.Size, "Test E");
        }

        [TestMethod]
        public void DeleteReturnValue()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] lines = TestFiles.short_english_list
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            test.AddAll(lines);

            // Act & Assert
            foreach(string line in lines)
            {
                Assert.IsTrue(test.Delete(line), $"Test A word {line}.");
            }
            Assert.AreEqual(0, test.Size, "Test B");
            Assert.IsFalse(test.Delete(string.Empty), "Test C");
            test.Add(string.Empty);
            Assert.IsFalse(test.Delete("cat"), "Test D");
            Assert.IsTrue(test.Delete(""), "Test E");
            Assert.IsFalse(test.Delete("cat"), "Test F");
        }

        [TestMethod]
        public void DeleteShuffled()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] lines = TestFiles.short_english_list
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            test.AddAll(lines);
            Random r = new();
            int len = lines.Length;
            for (int i = 0; i < len; i++)
            {
                int swapIndex = r.Next(len);
                if (i != swapIndex)
                {
                    string temp = lines[i];
                    lines[i] = lines[swapIndex];
                    lines[swapIndex] = temp;
                }
            }
            int size = test.Size;
            int count = 0;

            // Act & Assert
            foreach (string line in lines)
            {
                Assert.AreEqual(size, test.Size, $"Test A word {line} count {count}");
                size--;
                Assert.IsTrue(test.Has(line), $"Test B word {line} count {count}");
                Assert.IsTrue(test.Delete(line), $"Test C word {line} count {count}");
                Assert.IsFalse(test.Has(line), $"Test D word {line} count {count}");
                count++;
            }
            Assert.AreEqual(0, test.Size, "Test E");
        }
    }
}
