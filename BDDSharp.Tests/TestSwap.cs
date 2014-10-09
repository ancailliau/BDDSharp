using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace BDDSharp.Tests
{
    [TestFixture()]
    public class TestSwap
    {
        [Test ()]
        public void TestSwapSimple ()
        {
            var manager = new BDDManager (2);
            var n3 = manager.Create (1, manager.One, manager.Zero);
            var n4 = manager.Create (1, manager.Zero, manager.One);
            var root = manager.Create (0, n3, n4);
        
            var dict = new Dictionary<int, string> { { 0, "a" }, { 1, "b" }, { 2, "c" } };
            Console.WriteLine(manager.ToDot (root, (x) => dict[x.Index]));

            var res = manager.Swap (root, 0, 1);
            //var res = manager.Reduce (root);
            Console.WriteLine(manager.ToDot (res, (x) => dict[x.Index]));
            /*
            Assert.AreEqual (1, res.Index);
            Assert.AreEqual (false, res.Low.Value);
            Assert.AreEqual (true, res.High.Value);*/
        }

        [Test ()]
        public void TestSwapAsymetric ()
        {
            var manager = new BDDManager (2);
            var n3 = manager.Create (1, manager.One, manager.Zero);
            var root = manager.Create (0, n3, manager.One);

            var dict = new Dictionary<int, string> { { 0, "a" }, { 1, "b" }, { 2, "c" } };
            Console.WriteLine(manager.ToDot (root, (x) => dict[x.Index]));

            var res = manager.Reduce (manager.Swap (root, 0, 1));
            //var res = manager.Reduce (root);
            Console.WriteLine(manager.ToDot (res, (x) => dict[x.Index]));
            /*
            Assert.AreEqual (1, res.Index);
            Assert.AreEqual (false, res.Low.Value);
            Assert.AreEqual (true, res.High.Value);*/
        }

        [Test ()]
        public void TestSwapSimple2 ()
        {
            var manager = new BDDManager (3);
            var n2 = manager.Create (2, manager.One, manager.Zero);
            var n3 = manager.Create (1, manager.One, n2);
            var n4 = manager.Create (1, manager.Zero, manager.One);
            var root = manager.Create (0, n3, n4);

            var dict = new Dictionary<int, string> { { 0, "a" }, { 1, "b" }, { 2, "c" } };
            var rdict = dict.ToDictionary ((x) => x.Value, (x) => x.Key);
            Console.WriteLine(manager.ToDot (root, (x) => dict[x.Index]));

            var res = manager.Swap (root, rdict["b"], rdict["c"]);
            Console.WriteLine(manager.ToDot (res, (x) => dict[x.Index]));

            var res2 = manager.Reduce (res);
            Console.WriteLine(manager.ToDot (res2, (x) => dict[x.Index]));

            /*
            Assert.AreEqual (1, res.Index);
            Assert.AreEqual (false, res.Low.Value);
            Assert.AreEqual (true, res.High.Value);*/
        }
    }
}

