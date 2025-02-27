﻿using ftss;
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
            Assert.AreEqual((uint)2, test.Size, "Test A");
            Assert.IsTrue(test.Has(""), "Test B");
            test.Delete("");
            Assert.AreEqual((uint)1, test.Size, "Test C");
            Assert.IsFalse(test.Has(""), "Test D");
        }

        [TestMethod]
        public void DeleteNonMember()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.AreEqual((uint)0, test.Size, "Test A");
            test.Add("dog");
            Assert.AreEqual((uint)1, test.Size, "Test B");
            Assert.IsFalse(test.Has("cat"), "Test C");
            Assert.IsFalse(test.Delete("cat"), "Test D");
            Assert.AreEqual((uint)1, test.Size, "Test E");
        }

        [TestMethod]
        public void DeleteMember()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.AreEqual((uint)0, test.Size, "Test A");
            test.Add("dog");
            Assert.AreEqual((uint)1, test.Size, "Test B");
            Assert.IsTrue(test.Has("dog"), "Test C");
            Assert.IsTrue(test.Delete("dog"), "Test D");
            Assert.AreEqual((uint)0, test.Size, "Test E");
        }

        [TestMethod]
        public async Task DeleteReturnValue()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] lines = (await Common.GetResourceFileContents("short-english-list.txt"))
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            test.AddAll(lines);

            // Act & Assert
            foreach(string line in lines)
            {
                Assert.IsTrue(test.Delete(line), $"Test A word {line}.");
            }
            Assert.AreEqual((uint)0, test.Size, "Test B");
            Assert.IsFalse(test.Delete(string.Empty), "Test C");
            test.Add(string.Empty);
            Assert.IsFalse(test.Delete("cat"), "Test D");
            Assert.IsTrue(test.Delete(""), "Test E");
            Assert.IsFalse(test.Delete("cat"), "Test F");
        }

        [TestMethod]
        public async Task DeleteShuffled()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] lines = (await Common.GetResourceFileContents("short-english-list.txt"))
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
            uint size = test.Size;
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
            Assert.AreEqual((uint)0, test.Size, "Test E");
        }

        [TestMethod]
        public async Task DeleteAllTest()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] lines = (await Common.GetResourceFileContents("short-english-list.txt"))
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            test.AddAll(lines);
            uint size = test.Size;

            // Act & Assert
            test.DeleteAll(null);
            Assert.AreEqual(size, test.Size, "Test A");
            test.DeleteAll([]);
            Assert.AreEqual(size, test.Size, "Test B");
            test.DeleteAll(["bear",]);
            Assert.AreEqual(size - 1, test.Size, "Test C");
            test.DeleteAll(["chicken", "elephant",]);
            Assert.AreEqual(size - 3, test.Size, "Test D");
            foreach (string w in new string[] { "bear", "chicken", "elephant", })
            {
                Assert.IsFalse(test.Has(w), $"Test E word {w}");
            }
            test.DeleteAll(["goat", "hen",]);
            Assert.AreEqual(size - 5, test.Size, "Test F");
        }

        [TestMethod]
        public async Task DeleteAllReturn()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] lines = (await Common.GetResourceFileContents("short-english-list.txt"))
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            test.AddAll(lines);

            // Act & Assert
            Assert.AreEqual((uint)lines.Length, test.Size, "Test A");
            Assert.IsTrue(test.DeleteAll(lines), "Test B");
            Assert.AreEqual((uint)0, test.Size, "Test C");
            test.AddAll(["fish", "gerbil", "pigeon",]);
            Assert.AreEqual((uint)3, test.Size, "Test D");
            Assert.IsFalse(test.DeleteAll(["gerbil", "mongoose", "pigeon",]), "Test E");
            test.AddAll(["fish", "gerbil", "pigeon",]);
            Assert.IsFalse(test.DeleteAll(["mongoose", "gerbil", "pigeon",]), "Test F");
            test.AddAll(["fish", "gerbil", "pigeon",]);
            Assert.IsFalse(test.DeleteAll(["gerbil", "pigeon", "mongoose",]), "Test G");
            test.AddAll(["fish", "gerbil", "pigeon",]);
            Assert.IsFalse(test.DeleteAll(["mongoose",]), "Test H");
            test.AddAll(["fish", "gerbil", "pigeon",]);
            Assert.IsTrue(test.DeleteAll(["gerbil", "pigeon",]), "Test I");
        }
    }
}
