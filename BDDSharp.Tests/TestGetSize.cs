using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace UCLouvain.BDDSharp.Tests
{
    [TestFixture()]
    public class TestGetSize
    {
        // from Algorithms and data structures in VLSI design, page 124
        [Test()]
        public void TestComplex()
        {
            var dict = new Dictionary<int, string> { { 0, "x1" }, { 1, "x3" }, { 2, "x5" },
                { 3, "x2" }, { 4, "x4" }, { 5, "x6" } };
            var rdict = dict.ToDictionary((x) => x.Value, (x) => x.Key);

            var manager = new BDDManager(6);
            manager.GetVariableString = (x) => x < 6 ? dict[x] : "sink";

            var a13 = manager.Create(rdict["x6"], manager.One, manager.Zero);
            var a12 = manager.Create(rdict["x4"], manager.One, a13);
            var a11 = manager.Create(rdict["x4"], manager.One, manager.Zero);
            var a10 = manager.Create(rdict["x2"], manager.One, manager.Zero);
            var a9 = manager.Create(rdict["x2"], manager.One, a13);
            var a8 = manager.Create(rdict["x2"], manager.One, a11);
            var a7 = manager.Create(rdict["x2"], manager.One, a12);
            var a6 = manager.Create(rdict["x5"], a13, manager.Zero);
            var a5 = manager.Create(rdict["x5"], a12, a11);
            var a4 = manager.Create(rdict["x5"], a9, a10);
            var a3 = manager.Create(rdict["x5"], a7, a8);
            var a2 = manager.Create(rdict["x3"], a5, a6);
            var a1 = manager.Create(rdict["x3"], a3, a4);
            var a0 = manager.Create(rdict["x1"], a1, a2);

            Assert.AreEqual(3, manager.GetSize(a13));
            Assert.AreEqual(4, manager.GetSize(a12));
            Assert.AreEqual(3, manager.GetSize(a11));
            Assert.AreEqual(3, manager.GetSize(a10));
            Assert.AreEqual(4, manager.GetSize(a9));
            Assert.AreEqual(4, manager.GetSize(a8));
			Assert.AreEqual(5, manager.GetSize(a7));
            Assert.AreEqual(4, manager.GetSize(a6));
            Assert.AreEqual(6, manager.GetSize(a5));
            Assert.AreEqual(6, manager.GetSize(a4));
            Assert.AreEqual(8, manager.GetSize(a3));
            Assert.AreEqual(8, manager.GetSize(a2));
            Assert.AreEqual(12, manager.GetSize(a1));
            Assert.AreEqual(16, manager.GetSize(a0));

            var res = manager.Sifting(a0);
            Assert.AreEqual(8, manager.GetSize(res));
        }
    }
}

