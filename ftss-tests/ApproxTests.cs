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
    }
}
