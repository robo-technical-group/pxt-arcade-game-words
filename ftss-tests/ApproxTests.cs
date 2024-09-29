using ftss;
using System.Text.RegularExpressions;

namespace ftss_tests
{
    [TestClass]
    public class ApproxTests
    {
        [TestMethod]
        public void GetArrangementsBadArgument()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => test.GetArrangementsOf(null));
        }

        [TestMethod]
        public void GetArrangementsNoReuse()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] expected = [
                "ice",
                "ire",
                "nice",
                "rein",
                "rice",
            ];
            // Act
            test.AddAll([
                "apple",
                "baboon",
                "ice",
                "iced",
                "icicle",
                "ire",
                "mice",
                "nice",
                "niece",
                "rein",
                "rice",
                "spice",
            ]);
            // Assert
            CollectionAssert.AreEquivalent(expected, (List<string>)test.GetArrangementsOf("nicer"));
        }

        [TestMethod]
        public void GetArrangementsDuplicateChars()
        {
            // Arrange
            FastTernaryStringSet test = [];
            test.AddAll([
                "aah",
                "aardvark",
                "bar",
                "bazaar",
                "dark",
                "a",
                "aa",
                "aaa",
                "baa",
            ]);
            // Act & Assert
            CollectionAssert.AreEquivalent(new string[] { "a", "aa", "dark" }, (List<string>)test.GetArrangementsOf("ardvark"), "Test A");
            CollectionAssert.AreEquivalent(new string[] { "a", "aa", "aaa", "aardvark", "dark" }, (List<string>)test.GetArrangementsOf("aardvark"), "Test B");
            CollectionAssert.AreEquivalent(new string[] { }, (List<string>)test.GetArrangementsOf(""), "Test C");
            CollectionAssert.AreEquivalent(new string[] { "a" }, (List<string>)test.GetArrangementsOf("a"), "Test D");
            CollectionAssert.AreEquivalent(new string[] { "a", "aa" }, (List<string>)test.GetArrangementsOf("aa"), "Test E");
            CollectionAssert.AreEquivalent(new string[] { "a", "aa", "aaa" }, (List<string>)test.GetArrangementsOf("aaa"), "Test F");
            CollectionAssert.AreEquivalent(new string[] { "a", "aa", "aaa" }, (List<string>)test.GetArrangementsOf("aaaa"), "Test G");
        }

        [TestMethod]
        public void GetArrangementsEmptyString()
        {
            // Arrange
            FastTernaryStringSet test = [];
            test.AddAll(["a", "b", "c"]);
            // Act & Assert
            CollectionAssert.AreEquivalent(Array.Empty<string>(), (List<string>)test.GetArrangementsOf(""), "Test A");
            test.Add(string.Empty);
            CollectionAssert.AreEquivalent(new string[] { "" }, (List<string>)test.GetArrangementsOf(string.Empty), "Test B");
            CollectionAssert.AreEquivalent(new string[] { string.Empty }, (List<string>)test.GetArrangementsOf("z"), "Test C");
            CollectionAssert.AreEquivalent(new string[] { "", "a" }, (List<string>)test.GetArrangementsOf("a"), "Test D");
        }

        [TestMethod]
        public void GetCompletionsNull()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => test.GetCompletionsOf(null));
        }

        [TestMethod]
        public void GetCompletionsBasic()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] elements =
            [
                    "",
                    "aardvark",
                    "aardvarks",
                    "armadillo",
                    "baboon",
                    "badger",
                    "cats",
            ];
            string[] aardvarks = [ "aardvark", "aardvarks", ];
            string[] b = [ "baboon", "badger", ];
            string[] baboon = [ "baboon", ];
            test.AddAll(elements);
            // Act & Assert
            CollectionAssert.AreEquivalent(elements, (List<string>)test.GetCompletionsOf(string.Empty), "Test A");
            CollectionAssert.AreEquivalent(new string[]
            {
                "aardvark",
                "aardvarks",
                "armadillo",
            }, (List<string>)test.GetCompletionsOf("a"), "Test A");
            CollectionAssert.AreEquivalent(aardvarks, (List<string>)test.GetCompletionsOf("aa"), "Test B");
            CollectionAssert.AreEquivalent(aardvarks, (List<string>)test.GetCompletionsOf("aardvark"), "Test C");
            CollectionAssert.AreEquivalent(new string[] { "aardvarks", }, (List<string>)test.GetCompletionsOf("aardvarks"), "Test D");
            CollectionAssert.AreEquivalent(Array.Empty<string>(), (List<string>)test.GetCompletionsOf("aardvarkz"), "Test E");
            CollectionAssert.AreEquivalent(Array.Empty<string>(), (List<string>)test.GetCompletionsOf("aardvarksz"), "Test F");
            CollectionAssert.AreEquivalent(b, (List<string>)test.GetCompletionsOf("b"), "Test G");
            CollectionAssert.AreEquivalent(b, (List<string>)test.GetCompletionsOf("ba"), "Test H");
            CollectionAssert.AreEquivalent(baboon, (List<string>)test.GetCompletionsOf("bab"), "Test I");
            CollectionAssert.AreEquivalent(baboon, (List<string>)test.GetCompletionsOf("baboon"), "Test J");
            CollectionAssert.AreEquivalent(Array.Empty<string>(), (List<string>)test.GetCompletionsOf("z"), "Test K");
            CollectionAssert.AreEquivalent(Array.Empty<string>(), (List<string>)test.GetCompletionsOf("zaa"), "Test L");
            CollectionAssert.AreEquivalent(Array.Empty<string>(), (List<string>)test.GetCompletionsOf("babz"), "Test M");
        }

        protected static IList<string> GetCompletions(string prefix, IList<string> elements)
        {
            IList<string> results = [];
            foreach (string element in elements)
            {
                if (element.StartsWith(prefix))
                {
                    results.Add(element);
                }
            }
            return results;
        }

        [TestMethod]
        public void GetCompletionsAgainstWordList()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] lines = TestFiles.short_english_list
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            test.AddAll(lines);

            // Act & Assert
            CollectionAssert.AreEquivalent((List<string>)GetCompletions("z", lines), (List<string>)test.GetCompletionsOf("z"));
            CollectionAssert.AreEquivalent((List<string>)GetCompletions("wi", lines), (List<string>)test.GetCompletionsOf("wi"));
            Assert.AreEqual(14, test.GetCompletionsOf("wi").Count);
            CollectionAssert.AreEquivalent(new string[]
            {
                    "she",
                    "sheep",
                    "sheet",
                    "shelf",
            }, (List<string>)test.GetCompletionsOf("she"));
        }

        [TestMethod]
        public void GetCompletedNull()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => test.GetCompletedBy(null));
        }

        [TestMethod]
        public void GetCompletedBasic()
        {
            // Arrange
            FastTernaryStringSet test = [];
            IList<string> elements = [
                    "",
                    "aardvark",
                    "bumping",
                    "jumping",
                    "lamb",
                    "lifting",
                    "muskrat",
                    "trying",
                    "turtles",
                ];
            test.AddAll(elements);
            // Act & Assert
            CollectionAssert.AreEquivalent((List<string>)elements, (List<string>)test.GetCompletedBy(string.Empty), "Test A");
            CollectionAssert.AreEquivalent(new string[]
            {
                "bumping",
                "jumping",
                "lifting",
                "trying",
            }, (List<string>)test.GetCompletedBy("ing"), "Test B");
        }

        protected static IList<string> GetCompletedBy(string prefix, IList<string> elements)
        {
            List<string> results = [];
            foreach (string s in elements)
            {
                if (s.EndsWith(prefix))
                {
                    results.Add(s);
                }
            }
            return results;
        }

        [TestMethod]
        public void GetCompletedAgainstWordList()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] lines = TestFiles.short_english_list
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            test.AddAll(lines);

            // Act & Assert
            CollectionAssert.AreEquivalent((List<string>)GetCompletedBy("s", lines), (List<string>)test.GetCompletedBy("s"));
            CollectionAssert.AreEquivalent((List<string>)GetCompletedBy("ing", lines), (List<string>)test.GetCompletedBy("ing"));
            Assert.AreEqual(0, test.GetCompletedBy("zzz").Count);
        }

        [TestMethod]
        public void GetPartialsNull()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => test.GetPartialMatchesOf(null));
        }

        [TestMethod]
        public void GetPartialsBasic()
        {
            // Arrange
            FastTernaryStringSet test = [];
            IList<string> elements = ["a", "aa", "aaa", "aab", "aaaa", "aaaaa", "aaaab", "aaaac", ];
            test.AddAll(elements);

            // Act & Assert
            CollectionAssert.AreEquivalent(new string[] { "a" }, (List<string>)test.GetPartialMatchesOf("?", "?"), "Test A");
            Assert.AreEqual(0, test.GetPartialMatchesOf("").Count, "Test B");
            CollectionAssert.AreEquivalent(new string[] { "aa", }, (List<string>)test.GetPartialMatchesOf("a."), "Test C");
            string[] aa = ["aaa", "aab",];
            string[] aaa = ["aaa",];
            string[] aab = ["aab",];
            CollectionAssert.AreEquivalent(aa, (List<string>)test.GetPartialMatchesOf("a.."), "Test D");
            CollectionAssert.AreEquivalent(aa, (List<string>)test.GetPartialMatchesOf("aa."), "Test E");
            CollectionAssert.AreEquivalent(aa, (List<string>)test.GetPartialMatchesOf("..."), "Test F");
            CollectionAssert.AreEquivalent(aaa, (List<string>)test.GetPartialMatchesOf(".aa"), "Test G");
            CollectionAssert.AreEquivalent(aab, (List<string>)test.GetPartialMatchesOf(".ab"), "Test H");
            CollectionAssert.AreEquivalent(aaa, (List<string>)test.GetPartialMatchesOf("..a"), "Test I");
            CollectionAssert.AreEquivalent(aab, (List<string>)test.GetPartialMatchesOf("..b"), "Test J");
            CollectionAssert.AreEquivalent(aa, (List<string>)test.GetPartialMatchesOf(".a."), "Test K");
            string[] a5 = ["aaaaa", "aaaab", "aaaac",];
            CollectionAssert.AreEquivalent(a5, (List<string>)test.GetPartialMatchesOf("....."), "Test L");
            CollectionAssert.AreEquivalent(a5, (List<string>)test.GetPartialMatchesOf("aaaa."), "Test M");

            // Strings with no "don't care" can only match their exact strings.
            int count = 0;
            foreach (string el in elements)
            {
                CollectionAssert.AreEquivalent(new string[] { el, }, (List<string>)test.GetPartialMatchesOf(el), "Test N Pass " + count);
                count++;
            }
            Assert.AreEqual(0, test.GetPartialMatchesOf("Z").Count, "Test O");
        }

        [TestMethod]
        public void GetPartialsWordList()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] lines = TestFiles.short_english_list
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            test.AddAll(lines);

            // Act & Assert
            CollectionAssert.AreEquivalent(new string[] { "I", "a", }, (List<string>)test.GetPartialMatchesOf("."), "Test A");
            CollectionAssert.AreEquivalent(new string[] { "bean", "mean", }, (List<string>)test.GetPartialMatchesOf(".e.n"), "Test B");
            CollectionAssert.AreEquivalent(new string[]
            {
                "chocolate",
                "expensive",
                "furniture",
                "introduce",
                "structure",
                "substance",
                "telephone",
                "therefore",
                "vegetable",
                "xylophone",
            }, (List<string>)test.GetPartialMatchesOf("........e"), "Test C");
            CollectionAssert.AreEquivalent(new string[] { "join", "jump", "just", }, (List<string>)test.GetPartialMatchesOf("j..."), "Test D");
            CollectionAssert.AreEquivalent(new string[] { "juice", "quite", }, (List<string>)test.GetPartialMatchesOf(".u..e"), "Test E");
            CollectionAssert.AreEquivalent(new string[] { "public", }, (List<string>)test.GetPartialMatchesOf("public"), "Test F");
            CollectionAssert.AreEquivalent(new string[]
            {
                "bad",
                "bag",
                "can",
                "cap",
                "car",
                "cat",
                "day",
                "ear",
                "eat",
                "far",
                "hat",
                "man",
                "map",
                "may",
                "pan",
                "pay",
                "sad",
                "say",
                "was",
                "way",
            }, (List<string>)test.GetPartialMatchesOf(".a."), "Test G");
            CollectionAssert.AreEquivalent(new string[]
            {
                "comfortable",
                "examination",
                "grandfather",
                "grandmother",
            }, (List<string>)test.GetPartialMatchesOf("..........."), "Test H");
        }

        [TestMethod]
        public void GetPartialsEmptyString()
        {
            // Arrange
            FastTernaryStringSet test = ["", "a", "b",];
            // Act & Assert
            CollectionAssert.AreEquivalent(new string[] { "", }, (List<string>)test.GetPartialMatchesOf(""), "Test A");
            CollectionAssert.AreEquivalent(new string[] { "a", "b", }, (List<string>)test.GetPartialMatchesOf("."), "Test B");
            CollectionAssert.AreEquivalent(new string[] { "a", }, (List<string>)test.GetPartialMatchesOf("a"), "Test C");
            CollectionAssert.AreEquivalent(new string[] { "b", }, (List<string>)test.GetPartialMatchesOf("b"), "Test D");
        }

        [TestMethod]
        public void GetPartialsAltChar()
        {
            // Arrange
            FastTernaryStringSet test = ["c.t", "cat", "cot", "cup", "cut", ];
            // Act & Assert
            CollectionAssert.AreEquivalent(new string[]
            {
                "c.t",
                "cat",
                "cot",
                "cut",
            }, (List<string>)test.GetPartialMatchesOf("c?t", "?"), "Test A");
            CollectionAssert.AreEquivalent(new string[]
            {
                "c.t",
                "cat",
                "cot",
                "cup",
                "cut",
            }, (List<string>)test.GetPartialMatchesOf("c??", "?"), "Test B");
            CollectionAssert.AreEquivalent(new string[] { "cup", }, (List<string>)test.GetPartialMatchesOf("##p", "#"), "Test C");
            CollectionAssert.AreEquivalent(new string[] { "c.t", }, (List<string>)test.GetPartialMatchesOf("#.t", "#"), "Test D");
        }

        [TestMethod]
        public void GetHammingBadArg()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => test.GetWithinHammingDistanceOf(null, 0), "Test A");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => test.GetWithinHammingDistanceOf("", -1), "Test B");
        }

        [TestMethod]
        public void GetHammingZero()
        {
            // Arrange
            FastTernaryStringSet test = ["a", "aa", "aaa", "aaaa", "aac", "abc", "xyz",];
            // Act & Assert
            CollectionAssert.AreEquivalent(new string[] { "abc", }, (List<string>)test.GetWithinHammingDistanceOf("abc", 0), "Test A");
            Assert.AreEqual(0, test.GetWithinHammingDistanceOf("abz", 0).Count, "Test B");
            Assert.AreEqual(0, test.GetWithinHammingDistanceOf("azz", 0).Count, "Test C");
            Assert.AreEqual(0, test.GetWithinHammingDistanceOf("zzz", 0).Count, "Test D");
        }

        [TestMethod]
        public void GetHammingMax()
        {
            // Distance >= n matches all strings with pattern's length.
            // Arrange
            FastTernaryStringSet test = ["a", "aa", "aaa", "aaaa", "aac", "abc", "xyz",];
            string[] matches = [
                "aaa",
                "aac",
                "abc",
                "xyz",
            ];
            // Act & Assert
            CollectionAssert.AreEquivalent(matches, (List<string>)test.GetWithinHammingDistanceOf("abc", 3), "Test A");
            CollectionAssert.AreEquivalent(matches, (List<string>)test.GetWithinHammingDistanceOf("abc", 4), "Test B");
            CollectionAssert.AreEquivalent(matches, (List<string>)test.GetWithinHammingDistanceOf("abc", int.MaxValue), "Test B");
        }

        [TestMethod]
        public void GetHammingDistance()
        {
            // Distance 1..n-1 matches string <= distance.
            // Arrange
            FastTernaryStringSet test = ["a", "aa", "aaa", "aaaa", "aac", "abc", "xyz",];
            // Act & Assert
            CollectionAssert.AreEquivalent(new string[] { "aac", "abc", }, (List<string>)test.GetWithinHammingDistanceOf("abc", 1), "Test A");
            CollectionAssert.AreEquivalent(new string[] { "aaa", "aac", "abc", }, (List<string>)test.GetWithinHammingDistanceOf("abc", 2), "Test B");
        }

        [TestMethod]
        public void GetHammingCats()
        {
            // Arrange
            FastTernaryStringSet test = [];
            string[] lines = TestFiles.short_english_list
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            test.AddAll(lines);
            IList<string> match1 = lines.Where(s => Regex.IsMatch(s, @"^.at$") ||
                    Regex.IsMatch(s, @"^c.t$") ||
                    Regex.IsMatch(s, @"^ca.$")).ToList();
            IList<string> match2 = lines.Where(s => Regex.IsMatch(s, @"^..t$") ||
                    Regex.IsMatch(s, @"^c..$") ||
                    Regex.IsMatch(s, @"^.a.$")).ToList();
            IList<string> match3 = lines.Where(s => s.Length == 3).ToList();


            // Act & Assert
            CollectionAssert.AreEquivalent(new string[] { "cat", }, (List<string>)test.GetWithinHammingDistanceOf("cat", 0), "Test A");
            CollectionAssert.AreEquivalent((List<string>)match1,
                (List<string>)test.GetWithinHammingDistanceOf("cat", 1), "Test B");
            CollectionAssert.AreEquivalent((List<string>)match2,
                (List<string>)test.GetWithinHammingDistanceOf("cat", 2), "Test C");
            CollectionAssert.AreEquivalent((List<string>)match3,
                (List<string>)test.GetWithinHammingDistanceOf("cat", 3), "Test D");
        }

        [TestMethod]
        public void GetHammingEmptyString()
        {
            // Arrange
            FastTernaryStringSet test = ["", "a", "b",];
            string[] empty = ["",];
            // Act & Assert
            CollectionAssert.AreEquivalent(empty, (List<string>)test.GetWithinHammingDistanceOf("", 0), "Test A");
            CollectionAssert.AreEquivalent(empty, (List<string>)test.GetWithinHammingDistanceOf("", 1), "Test B");
            test.Delete("");
            Assert.AreEqual(0, test.GetWithinHammingDistanceOf("", 0).Count, "Test C");
        }
    }
}
