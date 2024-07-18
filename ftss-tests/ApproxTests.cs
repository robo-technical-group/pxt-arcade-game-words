using ftss;

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

        private static IList<string> GetCompletions(string prefix, IList<string> elements)
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
    }
}
