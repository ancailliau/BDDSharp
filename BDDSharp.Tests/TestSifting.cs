using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace UCLouvain.BDDSharp.Tests
{
    [TestFixture()]
    public class TestSifting : TestBDD
    {
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

            var truth = BuildThruthTable(manager, root);
            var res = manager.Sifting(root);
            CheckThruthTable(truth, res);
        }

        [Test()]
        public void TestDiamond()
        {
            var dict = new Dictionary<int, string> { 
                { 0, "x0" }, 
                { 1, "x1" }, 
                { 2, "x2" },
                { 3, "x3" }, 
                { 4, "x4" }, 
                { 5, "x5" }
             };
            var rdict = dict.ToDictionary ((x) => x.Value, (x) => x.Key);

            var manager = new BDDManager (6);
            manager.GetVariableString = (x) => x < 6 ? dict[x] : "sink";

            var a1 = manager.Create(rdict["x0"], 0, 1);            
            var a2 = manager.Create(rdict["x2"], 0, a1);
            var a3 = manager.Create(rdict["x2"], a1, 1);            
            var a4 = manager.Create(rdict["x3"], a2, a3);

            var truth = BuildThruthTable(manager, a4);
            var res = manager.Sifting(a4);
            CheckThruthTable(truth, res);
        }

        // from Algorithms and data structures in VLSI design, page 124
        [Test ()]
        public void TestComplex ()
        {
            var dict = new Dictionary<int, string> { { 0, "x1" }, { 1, "x3" }, { 2, "x5" },
                { 3, "x2" }, { 4, "x4" }, { 5, "x6" } };
            var rdict = dict.ToDictionary ((x) => x.Value, (x) => x.Key);

            var manager = new BDDManager (6);
            manager.GetVariableString = (x) => x < 6 ? dict[x] : "sink";

            var a13 = manager.Create (rdict["x6"], manager.One, manager.Zero);
            var a12 = manager.Create (rdict["x4"], manager.One, a13);
            var a11 = manager.Create (rdict["x4"], manager.One, manager.Zero);
            var a10 = manager.Create (rdict["x2"], manager.One, manager.Zero);
            var a9 = manager.Create (rdict["x2"], manager.One, a13);
            var a8 = manager.Create (rdict["x2"], manager.One, a11);
            var a7 = manager.Create (rdict["x2"], manager.One, a12);
            var a6 = manager.Create (rdict["x5"], a13, manager.Zero);
            var a5 = manager.Create (rdict["x5"], a12, a11);
            var a4 = manager.Create (rdict["x5"], a9, a10);
            var a3 = manager.Create (rdict["x5"], a7, a8);
            var a2 = manager.Create (rdict["x3"], a5, a6);
            var a1 = manager.Create (rdict["x3"], a3, a4);
            var a0 = manager.Create (rdict["x1"], a1, a2);

            var truth = BuildThruthTable(manager, a0);
            Console.WriteLine(manager.ToDot(a0, (x) => "x" + x.Index + " (" + x.RefCount.ToString() + ")"));

            var res = manager.Sifting (a0);
            Console.WriteLine(manager.ToDot(res, (x) => "x" + x.Index + " (" + x.RefCount.ToString() + ")"));
            
            CheckThruthTable(truth, res);
            
            Assert.AreEqual(8, manager.GetSize(res));
        }
        
        
    }
}

