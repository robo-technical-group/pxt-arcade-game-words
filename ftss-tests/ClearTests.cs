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
            Assert.AreEqual(0, test.Size, "Test A");
            test.Clear();
            Assert.AreEqual(0, test.Size, "Test B");
        }

        [TestMethod]
        public void ClearNonEmptyTree()
        {
            // Arrange
            FastTernaryStringSet test = [];

            // Act & Assert
            test.AddAll(["chicken", "duck", "whale",]);
            Assert.AreEqual(3, test.Size, "Test A");
            test.Clear();
            Assert.AreEqual(0, test.Size, "Test B");
        }

        [TestMethod]
        public void ClearWithEmptyString()
        {
            // Arrange
            FastTernaryStringSet test = [];
            test.Add("horse");
            test.Add(string.Empty);

            // Act & Assert
            Assert.AreEqual(2, test.Size, "Test A");
            test.Clear();
            Assert.AreEqual(0, test.Size, "Test B");
        }
    }
}
