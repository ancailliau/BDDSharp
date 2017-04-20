using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace UCLouvain.BDDSharp.Tests
{
    [TestFixture()]
    public class TestSwap
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

			// Assert that BDD represent the function a == b
			AssertTestSwapSimple(dict, root);

            var res = manager.Swap(root, 0, 1);

            // Assert that the swapped BDD represent the function a == b
            AssertTestSwapSimple(dict, res);
        }

        private void AssertTestSwapSimple(Dictionary<int, string> dict, BDDNode res)
        {
            for (int a = 0; a <= 1; a++)
            {
                for (int b = 0; b <= 1; b++)
                {
                    var interpretation = new Dictionary<string, bool>(){
                        { "a", a == 1 },
                        { "b", b == 1 }
                    };

                    EvaluateBDD(res, dict, interpretation, a == b);
                }
            }
        }

        [Test ()]
        public void TestSwapAsymetric ()
        {
            var manager = new BDDManager (2);
            var n3 = manager.Create (1, manager.One, manager.Zero);
            var root = manager.Create (0, n3, manager.One);

            var dict = new Dictionary<int, string> { { 0, "a" }, { 1, "b" }, { 2, "c" } };
			Console.WriteLine(manager.ToDot(root, (x) => dict[x.Index]));

			// Assert that the swapped BDD represent the function !a | b
			for (int a = 0; a <= 1; a++)
			{
				for (int b = 0; b <= 1; b++)
				{
					var A = a == 1;
					var B = b == 1;
					var interpretation = new Dictionary<string, bool>(){
						{ "a", A },
						{ "b", B }
					};

                    EvaluateBDD(root, dict, interpretation, !A | B);
				}
			}

            var res = manager.Reduce (manager.Swap (root, 0, 1));

            // Assert that the swapped BDD represent the function !a | b
            for (int a = 0; a <= 1; a++)
            {
                for (int b = 0; b <= 1; b++)
                {
                    var A = a == 1;
                    var B = b == 1;
                    var interpretation = new Dictionary<string, bool>(){
                        { "a", A },
                        { "b", B }
                    };

                    EvaluateBDD(res, dict, interpretation, !A|B);
                }
            }
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
            AssertSwapSimple2(dict, root);

            var res = manager.Swap(root, rdict["b"], rdict["c"]);
            AssertSwapSimple2(dict, res);

            var res2 = manager.Reduce(res);
            AssertSwapSimple2(dict, res2);
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
			AssertSwapSimple2(dict, root);

			var res = manager.Swap(root, rdict["b"], rdict["c"]);
			res = manager.Swap(root, rdict["a"], rdict["c"]);
			AssertSwapSimple2(dict, res);

			var res2 = manager.Reduce(res);
			AssertSwapSimple2(dict, res2);
		}

        private void AssertSwapSimple2(Dictionary<int, string> dict, BDDNode res)
        {
            for (int a = 0; a <= 1; a++)
            {
                for (int b = 0; b <= 1; b++)
				{
                    for (int c = 0; c <= 1; c++)
                    {
                        var A = a == 1;
						var B = b == 1;
						var C = c == 1;
                        var interpretation = new Dictionary<string, bool>(){
	                        { "a", A },
							{ "b", B },
							{ "c", C }
	                    };

                        EvaluateBDD(res, dict, interpretation, A & B | A & !B & C | !A & !B);
                    }
                }
            }
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
            AssertTestSwapBug(dict, root);

			var res = manager.Swap(root, rdict["x5"], rdict["x6"]);
			AssertTestSwapBug(dict, res);
		}

		private void AssertTestSwapBug(Dictionary<int, string> dict, BDDNode res)
		{
			for (int a = 0; a <= 1; a++)
			{
				for (int b = 0; b <= 1; b++)
				{
					for (int c = 0; c <= 1; c++)
					{
                        for (int d = 0; d <= 1; d++)
                        {
                            var A = a == 1;
                            var B = b == 1;
							var C = c == 1;
							var D = d == 1;
                            var interpretation = new Dictionary<string, bool>(){
	                            { "x3", A },
	                            { "x4", B },
								{ "x5", C },
								{ "x6", D }
	                        };

                            EvaluateBDD(res, dict, interpretation, A & B | D & C & !A | D & !C & A);
                        }
					}
				}
			}
		}

		void EvaluateBDD(BDDNode root, Dictionary<int, string> dict, Dictionary<string, bool> interpretation, bool expect)
		{
			if (root.IsOne)
			{
				Assert.True(expect);
			}
			else if (root.IsZero)
			{
				Assert.IsFalse(expect);
			}
			else
			{
                var b = interpretation[dict[root.Index]];
				if (b)
					EvaluateBDD(root.High, dict, interpretation, expect);
				else
					EvaluateBDD(root.Low, dict, interpretation, expect);
			}
		}
    }
}

