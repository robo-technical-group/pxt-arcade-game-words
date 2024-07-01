using ftss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ftss_tests
{
    [TestClass]
    public class SizeTests
    {
        [TestMethod]
        public void SizeNotDoubleCounted()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.AreEqual(test.Size, 0, "Test A");
            test.Add("peach");
            Assert.AreEqual(test.Size, 1, "Test B");
            test.Add("peach");
            Assert.AreEqual(test.Size, 1, "Test C");
        }

        public void SizeNotDoubleDeleted()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.AreEqual(test.Size, 0, "Test A");
            test.Add("peach");
            Assert.AreEqual(test.Size, 1, "Test B");
            test.Delete("peach");
            Assert.AreEqual(test.Size, 0, "Test C");
            test.Delete("peach");
            Assert.AreEqual(test.Size, 0, "Test D");
        }
    }
}
