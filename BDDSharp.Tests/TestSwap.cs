using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace UCLouvain.BDDSharp.Tests
{
    [TestFixture()]
    public class TestSwap : TestBDD
	{
        [Test()]
        public void TestSwapLastVariable()
        {
			var manager = new BDDManager(2);
			var n3 = manager.Create(1, manager.One, manager.Zero);
			var n4 = manager.Create(1, manager.Zero, manager.One);
			var root = manager.Create(0, n3, n4);
            var e = Assert.Catch(() =>
            {
                var res = manager.Swap(root, 1, 2);
            });

			Assert.IsInstanceOf(typeof(ArgumentException), e);
            StringAssert.Contains("1", e.Message);
		}

		[Test()]
		public void TestSwapNotAdjacentVariable()
		{
			var manager = new BDDManager(2);
			var n3 = manager.Create(1, manager.One, manager.Zero);
			var n4 = manager.Create(1, manager.Zero, manager.One);
			var root = manager.Create(0, n3, n4);
			var e = Assert.Catch(() =>
			{
				var res = manager.Swap(root, 0, 2);
			});

			Assert.IsInstanceOf(typeof(ArgumentException), e);
			StringAssert.Contains("not adjacents", e.Message);
		}

        [Test ()]
        public void TestSwapSimple ()
        {
            var manager = new BDDManager(2);
            var n3 = manager.Create(1, manager.One, manager.Zero);
            var n4 = manager.Create(1, manager.Zero, manager.One);
            var root = manager.Create(0, n3, n4);

            var dict = new Dictionary<int, string> { { 0, "a" }, { 1, "b" }, { 2, "c" } };

            var truth = BuildThruthTable(manager, root);
            Console.WriteLine(manager.ToDot(root, (x) => x.RefCount.ToString()));

            var res = manager.Swap(root, 0, 1);
            Console.WriteLine(manager.ToDot(res, (x) => x.RefCount.ToString()));

            CheckThruthTable(truth, res);
        }
        
        [Test ()]
        public void TestSwapAsymetric ()
        {
            var manager = new BDDManager (2);
            var n3 = manager.Create (1, manager.One, manager.Zero);
            var root = manager.Create (0, n3, manager.One);

            var dict = new Dictionary<int, string> { { 0, "a" }, { 1, "b" }, { 2, "c" } };

            var truth = BuildThruthTable(manager, root);
            Console.WriteLine(manager.ToDot(root, (x) => x.RefCount.ToString()));

            var res = manager.Reduce (manager.Swap (root, 0, 1));
            Console.WriteLine(manager.ToDot(res, (x) => x.RefCount.ToString()));

            CheckThruthTable(truth, res);
        }

        [Test ()]
        public void TestSwapSimple2 ()
        {
            var manager = new BDDManager(3);
            var n2 = manager.Create(2, manager.One, manager.Zero);
            var n3 = manager.Create(1, manager.One, n2);
            var n4 = manager.Create(1, manager.Zero, manager.One);
            var root = manager.Create(0, n3, n4);

            var dict = new Dictionary<int, string> { { 0, "a" }, { 1, "b" }, { 2, "c" } };
            var rdict = dict.ToDictionary((x) => x.Value, (x) => x.Key);

            var truth = BuildThruthTable(manager, root);
            Console.WriteLine(manager.ToDot(root, (x) => x.RefCount.ToString()));

            var res = manager.Swap(root, rdict["b"], rdict["c"]);
            CheckThruthTable(truth, res);
            Console.WriteLine(manager.ToDot(res, (x) => x.RefCount.ToString()));

            res = manager.Reduce(res);
            CheckThruthTable(truth, res);
            Console.WriteLine(manager.ToDot(res, (x) => x.RefCount.ToString()));
		}

		[Test()]
		public void TestSwapSimple2Chained()
		{
			var manager = new BDDManager(3);
			var n2 = manager.Create(2, manager.One, manager.Zero);
			var n3 = manager.Create(1, manager.One, n2);
			var n4 = manager.Create(1, manager.Zero, manager.One);
			var root = manager.Create(0, n3, n4);

			var dict = new Dictionary<int, string> { { 0, "a" }, { 1, "b" }, { 2, "c" } };
			var rdict = dict.ToDictionary((x) => x.Value, (x) => x.Key);
            var truth = BuildThruthTable(manager, root);

			var res = manager.Swap(root, rdict["b"], rdict["c"]);
            CheckThruthTable(truth, res);
            
			res = manager.Swap(root, rdict["a"], rdict["c"]);
            CheckThruthTable(truth, res);

			res = manager.Reduce(res);
            CheckThruthTable(truth, res);
		}
        
        [Test ()]
        public void TestSwapBug ()
        {
            var dict = new Dictionary<int, string> { 
                { 0, "x1" }, { 1, "x2" }, { 2, "x4" }, { 3, "x3" }, { 4, "x5" }, { 5, "x6" }
            };
            var rdict = dict.ToDictionary ((x) => x.Value, (x) => x.Key);

            var manager = new BDDManager (6);
            manager.GetVariableString = (x) => dict[x];

            var n9 = manager.Create (rdict["x6"], 1, 0);
            var n8 = manager.Create (rdict["x5"], 0, n9);
            var n7 = manager.Create (rdict["x5"], n9, 0);
            var n6 = manager.Create (rdict["x3"], n8, n7);
            var n5 = manager.Create (rdict["x3"], 1, n7);
            var n4 = manager.Create (rdict["x4"], n5, n6);
            var root = n4;
            var truth = BuildThruthTable(manager, root);
            Console.WriteLine(manager.ToDot(root, (x) => x.RefCount.ToString()));

			var res = manager.Swap(root, rdict["x5"], rdict["x6"]);
            CheckThruthTable(truth, res);
            Console.WriteLine(manager.ToDot(res, (x) => x.RefCount.ToString()));
		}
    }
}

