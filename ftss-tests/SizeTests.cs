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
            Assert.AreEqual(0, test.Size, "Test A");
            test.Add("peach");
            Assert.AreEqual(1, test.Size, "Test B");
            test.Add("peach");
            Assert.AreEqual(1, test.Size, "Test C");
        }

        [TestMethod]
        public void SizeNotDoubleDeleted()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.AreEqual(0, test.Size, "Test A");
            test.Add("peach");
            Assert.AreEqual(1, test.Size, "Test B");
            test.Delete("peach");
            Assert.AreEqual(0, test.Size, "Test C");
            test.Delete("peach");
            Assert.AreEqual(0, test.Size, "Test D");
        }

        [TestMethod]
        public void SizeWithEmptyString()
        {
            // Arrange
            FastTernaryStringSet test = [];
            // Act & Assert
            Assert.AreEqual(0, test.Size, "Test A");
            test.Add(string.Empty);
            Assert.AreEqual(1, test.Size, "Test B");
            test.Add("");
            Assert.AreEqual(1, test.Size, "Test C");
            test.Delete("");
            Assert.AreEqual(0, test.Size, "Test D");
            test.Delete(string.Empty);
            Assert.AreEqual(0, test.Size, "Test E");
            test.Add("");
            Assert.AreEqual(1, test.Size, "Test F");
            test.Add("whale");
            Assert.AreEqual(2, test.Size, "Test G");
        }
    }
}
