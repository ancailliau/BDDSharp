using System;
using NUnit.Framework;
using System.Collections.Generic;

namespace BDDSharp.Tests
{
    [TestFixture()]
    public class TestReduce
    {
        [Test ()]
        public void TestReduceSimple ()
        {
            var manager = new BDDManager (3);
            var n3 = new BDDNode (2, manager.One, manager.One);
            var n4 = new BDDNode (1, n3, manager.Zero);
            var n2 = new BDDNode (1, n3, manager.Zero);
            var root = new BDDNode (0, n2, n4);
            var res = manager.Reduce (root);
        
            Console.WriteLine(manager.ToDot (root, (x) => "x" + x.Index));
            Console.WriteLine(manager.ToDot (res, (x) => "x" + x.Index));

            Assert.AreEqual (1, res.Index);
            Assert.AreEqual (false, res.Low.Value);
            Assert.AreEqual (true, res.High.Value);
        }

        // Example from http://www.inf.ed.ac.uk/teaching/courses/ar/slides/bdd-ops.pdf
        [Test ()]
        public void TestReduceInfEdAcUk ()
        {
            var manager = new BDDManager (3);
            var n1 = new BDDNode (2, manager.One, manager.Zero);
            var n2 = new BDDNode (2, manager.One, manager.Zero);

            var n3 = new BDDNode (1, n1, manager.Zero);
            var n4 = new BDDNode (1, n2, n1);
            var n5 = new BDDNode (0, n4, n3);

            var res = manager.Reduce (n5);

            Assert.AreEqual (0, res.Index);
            Assert.AreEqual (1, res.Low.Index);
            Assert.AreEqual (2, res.Low.High.Index);
            Assert.AreEqual (2, res.High.Index);

            Assert.AreEqual (false, res.Low.Low.Value);
            Assert.AreEqual (false, res.Low.High.Low.Value);
            Assert.AreEqual (false, res.High.Low.Value);

            Assert.AreEqual (true, res.High.High.Value);
            Assert.AreEqual (true, res.Low.High.High.Value);
        }

        // Example from http://www.inf.unibz.it/~artale/FM/slide7.pdf
        [Test ()]
        public void TestReduceInfUnibzIt ()
        {
            var manager = new BDDManager (4);
            var n1 = new BDDNode (3, manager.Zero, manager.Zero);
            var n2 = new BDDNode (3, manager.One, manager.Zero);
            var n3 = new BDDNode (3, manager.Zero, manager.Zero);
            var n4 = new BDDNode (3, manager.One, manager.Zero);
            var n5 = new BDDNode (3, manager.Zero, manager.Zero);
            var n6 = new BDDNode (3, manager.One, manager.Zero);
            var n7 = new BDDNode (3, manager.One, manager.One);
            var n8 = new BDDNode (3, manager.One, manager.One);

            var n9 = new BDDNode (2, n2, n1);
            var n10 = new BDDNode (2, n4, n3);
            var n11 = new BDDNode (2, n6, n5);
            var n12 = new BDDNode (2, n8, n7);

            var n13 = new BDDNode (1, n10, n9);
            var n14 = new BDDNode (1, n12, n11);

            var n15 = new BDDNode (0, n14, n13);

            var res = manager.Reduce (n15);

            Assert.AreEqual (0, res.Index);
            Assert.AreEqual (2, res.Low.Index);
            Assert.AreEqual (3, res.Low.High.Index);
            Assert.AreEqual (1, res.High.Index);
            Assert.AreEqual (2, res.High.Low.Index);

            Assert.AreEqual (false, res.Low.Low.Value);
            Assert.AreEqual (false, res.Low.High.Low.Value);
            Assert.AreEqual (false, res.High.Low.Low.Value);

            Assert.AreEqual (true, res.High.High.Value);
            Assert.AreEqual (true, res.Low.High.High.Value);
        }
    }
}

