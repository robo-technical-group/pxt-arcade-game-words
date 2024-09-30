using ftss;
using System.Reflection.Metadata.Ecma335;

namespace ftss_tests
{
    [TestClass]
    public class EditDistTests
    {
        protected static int GetEditDistance(string from, string to)
        {
            // A standard Levenshtein distance function.
            if (from.Length == 0)
            {
                return to.Length;
            }
            if (to.Length == 0)
            {
                return from.Length;
            }

            int[] v0 = new int[to.Length + 1];
            int[] v1 = new int[to.Length + 1];

            for (int i = 0; i <= to.Length; i++)
            {
                v0[i] = i;
            }

            for (int i = 0; i < from.Length; i++)
            {
                v1[0] = i + 1;

                for (int j = 0; j < to.Length; j++)
                {
                    int substCost = from[i] == to[j] ? 0 : 1;
                    IList<int> vals = [v1[j] + 1, v0[j + 1] + 1, v0[j] + substCost];
                    int val = vals.Min();
                    v1[j + 1] = val;
                }

                for (int j = 0; j < v0.Length; j++)
                {
                    v0[j] = v1[j];
                }
            }

            return v1[to.Length];
        }

        /**
         * <summary>
         * Verifies that `getWithinEditDist` returns the correct result by
         * checking the edit distance for every string in the set.
         * </summary>
         */
        protected static void Verify(FastTernaryStringSet set, string pattern, int dist, string testName)
        {
            IList<string> result = set.GetWithinEditDistanceOf(pattern, dist);
            int count = 0;
            foreach (string s in set.ToList())
            {
                int d = GetEditDistance(pattern, s);
                Assert.AreEqual(d <= dist, result.Contains(s), testName + " Round " + count);
                count++;
            }
        }

        [TestMethod]
        public void EditDistBadArgs()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => test.GetWithinEditDistanceOf(null, 0), "Test A");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.GetWithinEditDistanceOf("", -1), "Test B");
        }

        [TestMethod]
        public void EditDistEmptyTree()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("", 10).Count, "Test A");
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("a", 10).Count, "Test B");
        }

        [TestMethod]
        public void EditDistEmptyString()
        {
            // Arrange
            FastTernaryStringSet test = [];
            test.Add(string.Empty);
            string[] emptyString = ["",];

            // Act & Assert
            CollectionAssert.AreEquivalent(emptyString, (List<string>)test.GetWithinEditDistanceOf("", 0), "Test A");
            CollectionAssert.AreEquivalent(emptyString, (List<string>)test.GetWithinEditDistanceOf("", 1), "Test B");
            CollectionAssert.AreEquivalent(emptyString, (List<string>)test.GetWithinEditDistanceOf("", 2), "Test C");

            Assert.AreEqual(0, test.GetWithinEditDistanceOf("a", 0).Count, "Test D");
            CollectionAssert.AreEquivalent(emptyString, (List<string>)test.GetWithinEditDistanceOf("a", 1), "Test E");
            CollectionAssert.AreEquivalent(emptyString, (List<string>)test.GetWithinEditDistanceOf("a", 2), "Test F");

            Assert.AreEqual(0, test.GetWithinEditDistanceOf("ab", 0).Count, "Test G");
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("ab", 1).Count, "Test H");
            CollectionAssert.AreEquivalent(emptyString, (List<string>)test.GetWithinEditDistanceOf("ab", 2), "Test I");
            CollectionAssert.AreEquivalent(emptyString, (List<string>)test.GetWithinEditDistanceOf("ab", 3), "Test J");

            test.Delete("");
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("", 0).Count, "Test K");
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("", 1).Count, "Test L");
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("", 2).Count, "Test M");

            Assert.AreEqual(0, test.GetWithinEditDistanceOf("a", 0).Count, "Test N");
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("a", 1).Count, "Test O");
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("a", 2).Count, "Test P");

            Assert.AreEqual(0, test.GetWithinEditDistanceOf("ab", 0).Count, "Test Q");
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("ab", 1).Count, "Test R");
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("ab", 2).Count, "Test S");
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("ab", 3).Count, "Test T");
        }

        [TestMethod]
        public void EditDistAfterPattern()
        {
            // Arrange
            FastTernaryStringSet test = ["a", "ab", "abc", "b",];

            // Act & Assert
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("", 0).Count, "Test A");
            CollectionAssert.AreEquivalent(new string[] { "a", "b", }, (List<string>)test.GetWithinEditDistanceOf("", 1), "Test B");
            CollectionAssert.AreEquivalent(new string[] { "a", "ab", "b", }, (List<string>)test.GetWithinEditDistanceOf("", 2), "Test C");
            CollectionAssert.AreEquivalent(new string[] { "a", "ab", "abc", "b", }, (List<string>)test.GetWithinEditDistanceOf("", 3), "Test D");

            CollectionAssert.AreEquivalent(new string[] { "a", }, (List<string>)test.GetWithinEditDistanceOf("a", 1), "Test E");
            CollectionAssert.AreEquivalent(new string[] { "a", "ab", "b", }, (List<string>)test.GetWithinEditDistanceOf("a", 2), "Test F");

            test.Clear();
            test.AddAll(["ab", "abc", "abcd"]);
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("a", 0).Count, "Test G");
            CollectionAssert.AreEquivalent(new string[] { "ab", }, (List<string>)test.GetWithinEditDistanceOf("a", 1), "Test H");
            CollectionAssert.AreEquivalent(new string[] { "ab", "abc", }, (List<string>)test.GetWithinEditDistanceOf("a", 2), "Test I");
            CollectionAssert.AreEquivalent(new string[] { "ab", "abc", "abcd", }, (List<string>)test.GetWithinEditDistanceOf("a", 3), "Test J");
        }

        [TestMethod]
        public void EditDistBeforePattern()
        {
            // Arrange
            FastTernaryStringSet test = ["a", "ab", "abc", "b",];

            // Act & Assert
            CollectionAssert.AreEquivalent(new string[] { "b", }, (List<string>)test.GetWithinEditDistanceOf("b", 0), "Test A");
            CollectionAssert.AreEquivalent(new string[] { "a", "ab", "b", }, (List<string>)test.GetWithinEditDistanceOf("b", 1), "Test B");
            CollectionAssert.AreEquivalent(new string[] { "a", "ab", "abc", "b", }, (List<string>)test.GetWithinEditDistanceOf("b", 2), "Test C");
            CollectionAssert.AreEquivalent(new string[] { "a", "ab", "abc", }, (List<string>)test.GetWithinEditDistanceOf("ac", 1), "Test D");
        }

        [TestMethod]
        public void EditDistSingleSubstitution()
        {
            // Arrange
            string[] ad = ["a", "b", "c", "d",];
            FastTernaryStringSet test = new(ad);

            // Act & Assert
            Assert.AreEqual(0, test.GetWithinEditDistanceOf("z", 0).Count, "Test A");
            CollectionAssert.AreEquivalent(ad, (List<string>)test.GetWithinEditDistanceOf("z", 1), "Test B");
            CollectionAssert.AreEquivalent(ad, (List<string>)test.GetWithinEditDistanceOf("z", 2), "Test C");

            CollectionAssert.AreEquivalent(new string[] { "a", }, (List<string>)test.GetWithinEditDistanceOf("a", 0), "Test D");
            CollectionAssert.AreEquivalent(ad, (List<string>)test.GetWithinEditDistanceOf("a", 1), "Test E");
            CollectionAssert.AreEquivalent(ad, (List<string>)test.GetWithinEditDistanceOf("a", 2), "Test F");
        }

        [TestMethod]
        public void EditDistMultipleSubstitution()
        {
            // Arrange
            string[] words = [
                "bat",
                "bit",
                "bye",
                "cap",
                "cat",
                "cog",
                "cot",
                "mat",
                "oat",
                "zip",
            ];
            FastTernaryStringSet test = new(words);

            // Act & Assert
            CollectionAssert.AreEquivalent(new string[] { "cat", }, (List<string>)test.GetWithinEditDistanceOf("cat", 0), "Test A");
            CollectionAssert.AreEquivalent(new string[]
            {
                "bat",
                "cap",
                "cat",
                "cot",
                "mat",
                "oat",
            }, (List<string>)test.GetWithinEditDistanceOf("cat", 1), "Test B");
            CollectionAssert.AreEquivalent(new string[]
            {
                "bat",
                "bit",
                "cap",
                "cat",
                "cog",
                "cot",
                "mat",
                "oat",
            }, (List<string>)test.GetWithinEditDistanceOf("cat", 2), "Test C");
            CollectionAssert.AreEquivalent(words, (List<string>)test.GetWithinEditDistanceOf("cat", 3), "Test D");
            CollectionAssert.AreEquivalent(words, (List<string>)test.GetWithinEditDistanceOf("cat", 4), "Test E");
        }

        [TestMethod]
        public void EditDistDeleteStart()
        {
            // Arrange
            string[] abc = ["abc",];
            string[] def = ["def",];
            FastTernaryStringSet test = ["abc", "def", "ghi"];

            // Act & Assert
            CollectionAssert.AreEquivalent(abc, (List<string>)test.GetWithinEditDistanceOf("aabc", 1), "Test A");
            CollectionAssert.AreEquivalent(def, (List<string>)test.GetWithinEditDistanceOf("adef", 1), "Test B");
            CollectionAssert.AreEquivalent(abc, (List<string>)test.GetWithinEditDistanceOf("aaabc", 2), "Test C");
            CollectionAssert.AreEquivalent(def, (List<string>)test.GetWithinEditDistanceOf("aadef", 2), "Test D");
            CollectionAssert.AreEquivalent(abc, (List<string>)test.GetWithinEditDistanceOf("xaabc", 2), "Test E");
            CollectionAssert.AreEquivalent(def, (List<string>)test.GetWithinEditDistanceOf("xadef", 2), "Test F");
            CollectionAssert.AreEquivalent(abc, (List<string>)test.GetWithinEditDistanceOf("azabc", 2), "Test G");
            CollectionAssert.AreEquivalent(def, (List<string>)test.GetWithinEditDistanceOf("azdef", 2), "Test H");
        }

        [TestMethod]
        public void EditDistDeleteMiddle()
        {
            // Arrange
            string[] abc = ["abc",];
            string[] def = ["def",];
            FastTernaryStringSet test = ["abc", "def", "ghi"];

            // Act & Assert
            CollectionAssert.AreEquivalent(abc, (List<string>)test.GetWithinEditDistanceOf("axbc", 1), "Test A");
            CollectionAssert.AreEquivalent(def, (List<string>)test.GetWithinEditDistanceOf("deef", 1), "Test B");
            CollectionAssert.AreEquivalent(abc, (List<string>)test.GetWithinEditDistanceOf("axbc", 1), "Test C");
            CollectionAssert.AreEquivalent(def, (List<string>)test.GetWithinEditDistanceOf("deeef", 2), "Test D");
            CollectionAssert.AreEquivalent(abc, (List<string>)test.GetWithinEditDistanceOf("axxbc", 2), "Test E");
            CollectionAssert.AreEquivalent(def, (List<string>)test.GetWithinEditDistanceOf("dxeef", 2), "Test F");
            CollectionAssert.AreEquivalent(abc, (List<string>)test.GetWithinEditDistanceOf("abxbc", 2), "Test G");
            CollectionAssert.AreEquivalent(def, (List<string>)test.GetWithinEditDistanceOf("dexef", 2), "Test H");
            CollectionAssert.AreEquivalent(abc, (List<string>)test.GetWithinEditDistanceOf("abxbc", 2), "Test I");
        }

        [TestMethod]
        public void EditDistDeleteEnd()
        {
            // Arrange
            string[] abc = ["abc",];
            string[] def = ["def",];
            FastTernaryStringSet test = ["abc", "def", "ghi"];

            // Act & Assert
            CollectionAssert.AreEquivalent(abc, (List<string>)test.GetWithinEditDistanceOf("abca", 1), "Test A");
            CollectionAssert.AreEquivalent(abc, (List<string>)test.GetWithinEditDistanceOf("abcc", 1), "Test B");
            CollectionAssert.AreEquivalent(def, (List<string>)test.GetWithinEditDistanceOf("defe", 1), "Test C");
            CollectionAssert.AreEquivalent(def, (List<string>)test.GetWithinEditDistanceOf("deff", 1), "Test D");
            CollectionAssert.AreEquivalent(abc, (List<string>)test.GetWithinEditDistanceOf("abcab", 2), "Test E");
            CollectionAssert.AreEquivalent(abc, (List<string>)test.GetWithinEditDistanceOf("abcbc", 2), "Test F");
            CollectionAssert.AreEquivalent(def, (List<string>)test.GetWithinEditDistanceOf("defde", 2), "Test G");
            CollectionAssert.AreEquivalent(def, (List<string>)test.GetWithinEditDistanceOf("defef", 2), "Test H");
            CollectionAssert.AreEquivalent(new string[] { "abc", "def", }, (List<string>)test.GetWithinEditDistanceOf("abcdef", 3), "Test I");
        }

        [TestMethod]
        public void EditDists()
        {
            // Arrange
            (string, int)[] tests = [
                ("aardva", 3),
                ("ae", 4),
                ("e", 24),
                ("eeeeeeeeeeee", 24),
                ("ea", 2),
                ("ing", 5),
                ("orl", 1),
                ("orl", 2),
                ("pie", 2),
                ("restaurant", 0),
                ("estaurant", 1),
                ("resturant", 1),
                ("restauran", 1),
                ("xrestaurant", 1),
                ("restxaurant", 1),
                ("resttaurant", 1),
                ("restaurantx", 1),
                ("restxaurant", 11),
                ("rn", 3),
                ("wi", 2),
                ("world", 1),
                ("zzz", 2),
                ("", 1),
                ("", 2),
                ("", 3),
                ("", 24),
            ];
            FastTernaryStringSet test = [];
            string[] lines = TestFiles.short_english_list
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            test.AddAll(lines);

            // Act & Assert
            int count = 0;
            foreach ((string, int) t in tests)
            {
                Verify(test, t.Item1, t.Item2, "Test " + count);
                count++;
            }
        }

        public static string ToBase4String(int number)
        {
            if (number == 0) return "0";

            string result = "";
            int baseValue = 4;

            while (number > 0)
            {
                int remainder = number % baseValue;
                result = remainder.ToString() + result;
                number /= baseValue;
            }

            return result;
        }

        [TestMethod]
        public void EditDistBaseFour()
        {
            // Arrange
            IList<string> base4list = [];
            for (int i = 0; i < Math.Pow(4, 4); i++)
            {
                base4list.Add(ToBase4String(i));
            }
            FastTernaryStringSet test = new(base4list);

            string[] patterns = [
                "", "3", "32", "321", "3210",
            ];

            int count = 0;
            foreach (string pattern in patterns)
            {
                for (int i = 0; i <= 4; i++)
                {
                    Verify(test, pattern, i, "Pattern " +  count + " distance " + i);
                }
            }

            for (int i = 5; i <= 8; i++)
            {
                Verify(test, "3210", i, "Pattern 3210 distance " + i);
            }
        }
    }
}
