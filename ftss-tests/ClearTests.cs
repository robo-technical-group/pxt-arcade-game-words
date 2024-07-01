using ftss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ftss_tests
{
    [TestClass]
    public class ClearTests
    {
        [TestMethod]
        public void ClearEmptyTree()
        {
            // Arrange
            FastTernaryStringSet test = [];

            // Act & Assert
            Assert.AreEqual(test.Size, 0, "Test A");
            test.Clear();
            Assert.AreEqual(test.Size, 0, "Test B");
        }

        [TestMethod]
        public void ClearNonEmptyTree()
        {
            // Arrange
            FastTernaryStringSet test = [];

            // Act & Assert
            test.AddAll(["chicken", "duck", "whale",]);
            Assert.AreEqual(test.Size, 3, "Test A");
            test.Clear();
            Assert.AreEqual(test.Size, 0, "Test B");
        }

        [TestMethod]
        public void ClearWithEmptyString()
        {
            // Arrange
            FastTernaryStringSet test = [];
            test.Add("horse");
            test.Add(string.Empty);

            // Act & Assert
            Assert.AreEqual(test.Size, 2, "Test A");
            test.Clear();
            Assert.AreEqual(test.Size, 0, "Test B");
        }
    }
}
