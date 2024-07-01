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
    }
}
