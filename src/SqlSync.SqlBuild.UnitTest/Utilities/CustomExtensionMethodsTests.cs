using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlSync.SqlBuild.UnitTest.Utilities
{
    [TestClass]
    public class CustomExtensionMethodsTests
    {
        #region SplitIntoChunks Tests

        [TestMethod]
        public void SplitIntoChunks_EqualDivision_ReturnsEqualChunks()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3, 4, 5, 6 };

            // Act
            var chunks = list.SplitIntoChunks(3).ToList();

            // Assert
            Assert.AreEqual(3, chunks.Count);
            Assert.AreEqual(2, chunks[0].Count());
            Assert.AreEqual(2, chunks[1].Count());
            Assert.AreEqual(2, chunks[2].Count());
        }

        [TestMethod]
        public void SplitIntoChunks_UnequalDivision_DistributesEvenly()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3, 4, 5, 6, 7 };

            // Act
            var chunks = list.SplitIntoChunks(3).ToList();

            // Assert
            Assert.AreEqual(3, chunks.Count);
            // Total should be 7
            Assert.AreEqual(7, chunks.Sum(c => c.Count()));
        }

        [TestMethod]
        public void SplitIntoChunks_SingleChunk_ReturnsAllElements()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3, 4, 5 };

            // Act
            var chunks = list.SplitIntoChunks(1).ToList();

            // Assert
            Assert.AreEqual(1, chunks.Count);
            Assert.AreEqual(5, chunks[0].Count());
        }

        [TestMethod]
        public void SplitIntoChunks_MoreChunksThanElements_CreatesFewerChunks()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3 };

            // Act
            var chunks = list.SplitIntoChunks(10).ToList();

            // Assert
            // Should not create more chunks than elements
            Assert.IsTrue(chunks.Count <= list.Count);
            Assert.AreEqual(3, chunks.Sum(c => c.Count()));
        }

        [TestMethod]
        public void SplitIntoChunks_EmptyList_ReturnsEmptyChunks()
        {
            // Arrange
            var list = new List<int>();

            // Act
            var chunks = list.SplitIntoChunks(3).ToList();

            // Assert
            Assert.AreEqual(0, chunks.Sum(c => c.Count()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SplitIntoChunks_ZeroChunks_ThrowsArgumentException()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3 };

            // Act
            var chunks = list.SplitIntoChunks(0).ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SplitIntoChunks_NegativeChunks_ThrowsArgumentException()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3 };

            // Act
            var chunks = list.SplitIntoChunks(-1).ToList();
        }

        [TestMethod]
        public void SplitIntoChunks_PreservesOrder()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3, 4, 5, 6 };

            // Act
            var chunks = list.SplitIntoChunks(2).ToList();
            var flattened = chunks.SelectMany(c => c).ToList();

            // Assert
            CollectionAssert.AreEqual(list, flattened);
        }

        [TestMethod]
        public void SplitIntoChunks_WithStrings_WorksCorrectly()
        {
            // Arrange
            var list = new List<string> { "a", "b", "c", "d", "e", "f" };

            // Act
            var chunks = list.SplitIntoChunks(3).ToList();

            // Assert
            Assert.AreEqual(3, chunks.Count);
            Assert.AreEqual(6, chunks.Sum(c => c.Count()));
        }

        [TestMethod]
        public void SplitIntoChunks_LargeList_HandlesCorrectly()
        {
            // Arrange
            var list = Enumerable.Range(1, 100).ToList();

            // Act
            var chunks = list.SplitIntoChunks(7).ToList();

            // Assert
            Assert.AreEqual(7, chunks.Count);
            Assert.AreEqual(100, chunks.Sum(c => c.Count()));
        }

        [TestMethod]
        public void SplitIntoChunks_TwoElements_TwoChunks()
        {
            // Arrange
            var list = new List<int> { 1, 2 };

            // Act
            var chunks = list.SplitIntoChunks(2).ToList();

            // Assert
            Assert.AreEqual(2, chunks.Count);
            Assert.AreEqual(1, chunks[0].Count());
            Assert.AreEqual(1, chunks[1].Count());
        }

        [TestMethod]
        public void SplitIntoChunks_PrimeNumberElements_HandlesCorrectly()
        {
            // Arrange - 17 is prime
            var list = Enumerable.Range(1, 17).ToList();

            // Act
            var chunks = list.SplitIntoChunks(4).ToList();

            // Assert
            Assert.AreEqual(4, chunks.Count);
            Assert.AreEqual(17, chunks.Sum(c => c.Count()));
        }

        [TestMethod]
        public void SplitIntoChunks_AllElementsSameValue_WorksCorrectly()
        {
            // Arrange
            var list = Enumerable.Repeat(42, 10).ToList();

            // Act
            var chunks = list.SplitIntoChunks(3).ToList();

            // Assert
            Assert.AreEqual(10, chunks.Sum(c => c.Count()));
        }

        [TestMethod]
        public void SplitIntoChunks_WithObjects_WorksCorrectly()
        {
            // Arrange
            var list = new List<object> 
            { 
                new { Id = 1 }, 
                new { Id = 2 }, 
                new { Id = 3 }, 
                new { Id = 4 } 
            };

            // Act
            var chunks = list.SplitIntoChunks(2).ToList();

            // Assert
            Assert.AreEqual(2, chunks.Count);
            Assert.AreEqual(4, chunks.Sum(c => c.Count()));
        }

        [TestMethod]
        public void SplitIntoChunks_SingleElement_ReturnsOneChunk()
        {
            // Arrange
            var list = new List<int> { 42 };

            // Act
            var chunks = list.SplitIntoChunks(5).ToList();

            // Assert
            Assert.AreEqual(1, chunks.Sum(c => c.Count()));
            Assert.IsTrue(chunks.Any(c => c.Count() == 1));
        }

        [TestMethod]
        public void SplitIntoChunks_EachChunkHasExpectedElements()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3, 4 };

            // Act
            var chunks = list.SplitIntoChunks(2).ToList();

            // Assert - First chunk should have first half, second chunk should have second half
            CollectionAssert.AreEqual(new[] { 1, 2 }, chunks[0].ToList());
            CollectionAssert.AreEqual(new[] { 3, 4 }, chunks[1].ToList());
        }

        #endregion
    }
}
