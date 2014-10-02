using NUnit.Framework;
using System;

namespace BDDSharp.Tests
{
    [TestFixture ()]
    public class Test
    {
        [Test ()]
        public void TestReduce ()
        {
            var bdd = new BDD (3);
            var n3 = new Node (3, 3, bdd.One, bdd.One);
            var n4 = new Node (2, 3, n3, bdd.Zero);
            var n2 = new Node (2, 3, n3, bdd.Zero);
            bdd.SetRoot (new Node (1, 3, n2, n4));
            bdd.Reduce ();

            Assert.AreEqual (2, bdd.Root.Index);
            Assert.AreEqual (false, bdd.Root.Low.Value);
            Assert.AreEqual (true, bdd.Root.High.Value);
        }

        [Test ()]
        public void TestOr ()
        {
            var bdd = new BDD (3);
            var a2 = new Node (3, 3, bdd.Zero, bdd.One);
            var a1 = new Node (1, 3, a2, bdd.One);
            bdd.SetRoot (a1);

            var bdd2 = new BDD (3);
            var b2 = new Node (3, 3, bdd2.One, bdd2.Zero);
            var b1 = new Node (2, 3, b2, bdd2.Zero);
            bdd2.SetRoot (b1);

            var bdd3 = bdd.Or(bdd2);
            bdd3.Reduce();

            Assert.AreEqual (1, bdd3.Root.Index);
            Assert.AreEqual (2, bdd3.Root.High.Index);
            Assert.AreEqual (3, bdd3.Root.High.Low.Index);

            Assert.AreEqual (true, bdd3.Root.Low.Value);
            Assert.AreEqual (true, bdd3.Root.High.High.Value);
            Assert.AreEqual (true, bdd3.Root.High.Low.Low.Value);
            Assert.AreEqual (false, bdd3.Root.High.Low.High.Value);
        }
    }
}

