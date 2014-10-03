using NUnit.Framework;
using System;

namespace BDDSharp.Tests
{
    [TestFixture()]
    public class Test
    {
        /*
        [Test()]
        public void TestITE()
        {
            var bdd_f = new BDDManager (4);
            var b = new BDDNode (bdd_f, 1, bdd_f.One, bdd_f.Zero);
            var f = new BDDNode (bdd_f, 0, bdd_f.One, b);
            bdd_f.SetRoot (f);

            var bdd_g = new BDDManager (4);
            var c = new BDDNode (bdd_g, 2, bdd_g.One, bdd_g.Zero);
            var g = new BDDNode (bdd_g, 0, c, bdd_g.Zero);
            bdd_g.SetRoot (g);

            var bdd_h = new BDDManager (4);
            var d = new BDDNode (bdd_h, 3, bdd_h.One, bdd_h.Zero);
            var h = new BDDNode (bdd_h, 1, bdd_h.One, d);
            bdd_h.SetRoot (h);

            BDDManager res = BDDManager.ITE (bdd_f, bdd_g, bdd_h);

            Console.WriteLine (bdd_f.ToDot ());
            Console.WriteLine ("--");
            Console.WriteLine (bdd_g.ToDot ());
            Console.WriteLine ("--");
            Console.WriteLine (bdd_h.ToDot ());
            Console.WriteLine ("--");
            Console.WriteLine (res.ToDot ());
        }
        */

        //        [Test ()]
        //        public void TestRestrictToHigh ()
        //        {
        //            var bdd = new BDD (2);
        //            var n = new Node (2, 2, bdd.One, bdd.Zero);
        //            bdd.SetRoot (new Node (1, 2, n, bdd.Zero));
        //            bdd.Restrict (2, true);
        //
        //            Assert.AreEqual (1, bdd.Root.Index);
        //            Assert.AreEqual (true, bdd.Root.High.Value);
        //        }
        //
        //        [Test ()]
        //        public void TestRestrictToLow ()
        //        {
        //            var bdd = new BDD (2);
        //            var n = new Node (2, 2, bdd.Zero, bdd.One);
        //            bdd.SetRoot (new Node (1, 2, n, bdd.Zero));
        //            bdd.Restrict (2, false);
        //
        //            Assert.AreEqual (1, bdd.Root.Index);
        //            Assert.AreEqual (false, bdd.Root.High.Value);
        //        }
        //
        //        [Test ()]
        //        public void TestRestrictRootToHigh ()
        //        {
        //            var bdd = new BDD (2);
        //            var n = new Node (2, 2, bdd.One, bdd.Zero);
        //            bdd.SetRoot (new Node (1, 2, n, bdd.Zero));
        //            bdd.Restrict (1, true);
        //
        //            Assert.AreEqual (n, bdd.Root);
        //        }
        //
        //        [Test ()]
        //        public void TestRestrictRootToLow ()
        //        {
        //            var bdd = new BDD (2);
        //            var n = new Node (2, 2, bdd.One, bdd.Zero);
        //            bdd.SetRoot (new Node (1, 2, bdd.Zero, n));
        //            bdd.Restrict (1, false);
        //
        //            Assert.AreEqual (n, bdd.Root);
        //        }
        //
        //        [Test ()]
        //        public void TestRestrictRootToZero ()
        //        {
        //            var bdd = new BDD (1);
        //            bdd.SetRoot (new Node (1, 1, bdd.One, bdd.Zero));
        //            bdd.Restrict (1, false);
        //
        //            Assert.AreEqual (false, bdd.Root.Value);
        //        }
        //
        //        [Test ()]
        //        public void TestRestrictRootToOne ()
        //        {
        //            var bdd = new BDD (1);
        //            bdd.SetRoot (new Node (1, 1, bdd.One, bdd.Zero));
        //            bdd.Restrict (1, true);
        //
        //            Assert.AreEqual (true, bdd.Root.Value);
        //        }
        //
        //        [Test ()]
        //        public void TestReduce ()
        //        {
        //            var bdd = new BDD (3);
        //            var n3 = new Node (3, 3, bdd.One, bdd.One);
        //            var n4 = new Node (2, 3, n3, bdd.Zero);
        //            var n2 = new Node (2, 3, n3, bdd.Zero);
        //            bdd.SetRoot (new Node (1, 3, n2, n4));
        //            bdd.Reduce ();
        //
        //            Assert.AreEqual (2, bdd.Root.Index);
        //            Assert.AreEqual (false, bdd.Root.Low.Value);
        //            Assert.AreEqual (true, bdd.Root.High.Value);
        //        }
        //
        //        [Test ()]
        //        public void TestOr ()
        //        {
        //            var bdd = new BDD (3);
        //            var a2 = new Node (3, 3, bdd.Zero, bdd.One);
        //            var a1 = new Node (1, 3, a2, bdd.One);
        //            bdd.SetRoot (a1);
        //
        //            var bdd2 = new BDD (3);
        //            var b2 = new Node (3, 3, bdd2.One, bdd2.Zero);
        //            var b1 = new Node (2, 3, b2, bdd2.Zero);
        //            bdd2.SetRoot (b1);
        //
        //            var bdd3 = bdd.Or(bdd2);
        //            bdd3.Reduce();
        //
        //            Assert.AreEqual (1, bdd3.Root.Index);
        //            Assert.AreEqual (2, bdd3.Root.High.Index);
        //            Assert.AreEqual (3, bdd3.Root.High.Low.Index);
        //
        //            Assert.AreEqual (true, bdd3.Root.Low.Value);
        //            Assert.AreEqual (true, bdd3.Root.High.High.Value);
        //            Assert.AreEqual (true, bdd3.Root.High.Low.Low.Value);
        //            Assert.AreEqual (false, bdd3.Root.High.Low.High.Value);
        //        }
    }
}

