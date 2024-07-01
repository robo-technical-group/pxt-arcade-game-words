using ftss;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using System.Runtime.InteropServices.Marshalling;

namespace ftss_tests
{
    [TestClass]
    public class AddHasTests
    {
        [TestMethod]
        public void AddEmptyString()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act
            test.Add(String.Empty);
            // Assert
            int size = test.Size;
            Assert.AreEqual(1, size);
            bool has = test.Has(String.Empty);
            Assert.IsTrue(has);
            has = test.Has("c");
            Assert.IsFalse(has);
        }

        [TestMethod]
        public void AddLength1String()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act
            test.Add("a");
            // Assert
            bool has = test.Has("a");
            Assert.IsTrue(has);
            foreach (string t in new string[] { "", "c", "aa" })
            {
                has = test.Has(t);
                Assert.IsFalse(has, $"Test ${t} failed.");
            }
        }

        [TestMethod]
        public void AddSingleton()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act
            test.Add("cat");
            // Assert
            bool has = test.Has("cat");
            Assert.IsTrue(has);
            foreach (string t in new string[] {"", "c", "cc", "ca", "caa", "cats"})
            {
                has = test.Has(t);
                Assert.IsFalse(has, $"Test ${t} failed.");
            }
        }

        [TestMethod]
        public void AddMultipleStrings()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] words =
            [
                "moose",
                "dolphin",
                "caribou",
                "emu",
                "snake",
                "zebra",
                "narwhal",
            ];
            // Act & Assert
            foreach (string s in words)
            {
                test.Add(s);
                Assert.IsTrue(test.Has(s), $"Test ${s} failed during insert.");
            }
            foreach (string s in words)
            {
                Assert.IsTrue(test.Has(s), $"Test ${s} failed after all adds.");
            }
            Assert.AreEqual(words.Length, test.Size, "Size test failed.");
        }

        [TestMethod]
        public void AddAllSimple()
        {
            // Arrange
            string[][] tests =
                [
                    [],
                    ["ape",],
                    ["ape", "cat",],
                    ["ape", "cat", "eel",],
                ];
            // Act & Assert
            foreach (string[] t in tests)
            {
                FastTernaryStringSet test = [];
                test.AddAll(t);
                Assert.AreEqual(t.Length, test.Size, $"Size test failed for size ${t.Length}");
                foreach (string s in t)
                {
                    Assert.IsTrue(test.Has(s), $"Search test for ${s} with size ${t.Length} failed.");
                }
            }
        }

        [TestMethod]
        public void AddAllWithDuplicates()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] words = [
                    "antelope",
                    "crab",
                    "porcupine",
                    "crab",
                    "crab",
                    "crab",
                    "antelope",
                    "porcupine",
                ];
            // Act
            test.AddAll(words);
            // Assert
            Assert.AreEqual(3, test.Size);
        }

        [TestMethod]
        public void AddFromShortEnglishFile()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] lines = TestFiles.short_english_list
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // Act
            test.AddAll(lines);
            // Assert
            Assert.AreEqual(lines.Length, test.Size, "Size mismatch.");
            foreach (string line in lines)
            {
                Assert.IsTrue(test.Has(line), $"Word ${line} not found.");
            }
        }

        [TestMethod]
        public void AddAllComplexStrings()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] words = [
                "Mt. Doom",
                "a dog—smelly",
                "line 1\nline2",
                "🙂",
                "I have a pet 🐈",
                "good 🍀 luck!",
                "程序设计员在用电脑。",
                "𝄞𝅘𝅥𝅘𝅥𝅮𝅘𝅥𝅯𝅘𝅥𝅰𝄽",
                "The \0 NUL Zone",
                "max code point \udbff\udfff",
                ];
            // Act
            test.AddAll(words);
            // Assert
            Assert.IsTrue(test.Size > 0, "Size is zero?");
            Assert.AreEqual(words.Length, test.Size, "Size mismatch.");
        }

        [TestMethod]
        public void AddAllRange()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            test.AddAll([]);
            Assert.AreEqual(0, test.Size, "Test A");
            test.AddAll(["mongoose",]);
            Assert.AreEqual(1, test.Size, "Test B");
            test.AddAll(["badger", "pelican",], 0, 2);
            Assert.AreEqual(3, test.Size, "Test C");
            test.AddAll(["asp", "mouse", "oyster",], 1, 3);
            Assert.AreEqual(5, test.Size, "Test D");
            Assert.IsFalse(test.Has("asp"), "Test E");
            test.AddAll(["barracuda", "cricket", "panda", "tiger",], 0, 2);
            Assert.AreEqual(7, test.Size, "Test F");
            Assert.IsTrue(test.Has("barracuda") && test.Has("cricket"), "Test G");
            Assert.IsFalse(test.Has("panda") && test.Has("tiger"), "Test H");
            test.AddAll(["bison", "caribou", "deer", "elk", "moose",], 1);
            Assert.AreEqual(11, test.Size, "Test I");
            Assert.IsFalse(test.Has("bison"), "Test J");
            Assert.IsTrue(test.Has("caribou") && test.Has("moose"), "Test K");
        }

        [TestMethod]
        public void AddAllBadIndices()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.AddAll(["badger",], -1), "Test A");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.AddAll(["hare",], 2), "Test B");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.AddAll(["ox",], 0, -1), "Test C");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.AddAll(["carp",], 0, 2), "Test D");
        }
    }
}
