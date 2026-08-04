using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnlyV.VerseExtraction.Utils;

namespace Tests
{
    [TestClass]
    public class MostRecentlyUsedListTests
    {
        [TestMethod]
        public void Constructor_NonPositiveCapacity_Throws()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new MostRecentlyUsedList<string, int>(0));
        }

        [TestMethod]
        public void Add_ThenTryGetValue_ReturnsValue()
        {
            var list = new MostRecentlyUsedList<string, int>(2);

            list.Add("a", 1);

            var found = list.TryGetValue("a", out var value);

            Assert.IsTrue(found);
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void TryGetValue_MissingKey_ReturnsFalse()
        {
            var list = new MostRecentlyUsedList<string, int>(2);

            var found = list.TryGetValue("missing", out var value);

            Assert.IsFalse(found);
            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void Add_BeyondCapacity_EvictsLeastRecentlyUsed()
        {
            var list = new MostRecentlyUsedList<string, int>(2);

            list.Add("a", 1);
            list.Add("b", 2);
            list.Add("c", 3); // should evict "a", the least recently touched

            Assert.IsFalse(list.TryGetValue("a", out _));
            Assert.IsTrue(list.TryGetValue("b", out var b));
            Assert.IsTrue(list.TryGetValue("c", out var c));
            Assert.AreEqual(2, b);
            Assert.AreEqual(3, c);
        }

        [TestMethod]
        public void TryGetValue_RefreshesRecency_PreventsEviction()
        {
            var list = new MostRecentlyUsedList<string, int>(2);

            list.Add("a", 1);
            list.Add("b", 2);

            list.TryGetValue("a", out _); // touching "a" makes "b" the least recently used

            list.Add("c", 3); // should evict "b" instead of "a"

            Assert.IsTrue(list.TryGetValue("a", out var a));
            Assert.IsFalse(list.TryGetValue("b", out _));
            Assert.IsTrue(list.TryGetValue("c", out var c));
            Assert.AreEqual(1, a);
            Assert.AreEqual(3, c);
        }
    }
}
