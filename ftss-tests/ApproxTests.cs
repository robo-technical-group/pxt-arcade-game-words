using ftss;

namespace ftss_tests
{
    [TestClass]
    public class ApproxTests
    {
        /**
         * Test not necessary; pattern cannot be passed a null value.
        [TestMethod]
        public void GetArrangementsBadArgument()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => test.GetArrangementsOf(null));
        }
        */

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
            CollectionAssert.AreEquivalent(new string[] { }, (List<string>)test.GetArrangementsOf(""), "Test A");
            test.Add(string.Empty);
            CollectionAssert.AreEquivalent(new string[] { "" }, (List<string>)test.GetArrangementsOf(string.Empty), "Test B");
            CollectionAssert.AreEquivalent(new string[] { string.Empty }, (List<string>)test.GetArrangementsOf("z"), "Test C");
            CollectionAssert.AreEquivalent(new string[] { "", "a" }, (List<string>)test.GetArrangementsOf("a"), "Test D");
        }
    }
}
