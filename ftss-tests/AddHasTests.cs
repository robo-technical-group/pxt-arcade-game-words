using ftss;

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
            Assert.AreEqual(test.Size, words.Length, "Size test failed.");
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
                Assert.AreEqual(test.Size, t.Length, $"Size test failed for size ${t.Length}");
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
            Assert.AreEqual(test.Size, 3);
        }
    }
}
