using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace UCLouvain.BDDSharp.Tests
{
    [TestFixture()]
    public class TestITE
    {
        [Test()]
        public void TestSimpleITE()
        {
            var manager = new BDDManager (4);

            var b = manager.Create (1, manager.One, manager.Zero);
            var f = manager.Create (0, manager.One, b);

            var c = manager.Create (2, manager.One, manager.Zero);
            var g = manager.Create (0, c, manager.Zero);

            var d = manager.Create (3, manager.One, manager.Zero);
            var h = manager.Create (1, manager.One, d);

            var dict = new Dictionary<int, string> { { 0, "a" }, { 1, "b" }, { 2, "c" }, { 3 , "d" } };

            var res = manager.ITE (f, g, h);
            manager.Reduce (res);

            Assert.AreEqual (0, res.Index);
            Assert.AreEqual (2, res.High.Index);
            Assert.AreEqual (1, res.Low.Index);
            Assert.AreEqual (3, res.Low.Low.Index);

            Assert.AreEqual (true, res.High.High.Value);
            Assert.AreEqual (false, res.High.Low.Value);
            Assert.AreEqual (false, res.Low.High.Value);
            Assert.AreEqual (true, res.Low.Low.High.Value);
            Assert.AreEqual (false, res.Low.Low.Low.Value);
        }
    }
}

