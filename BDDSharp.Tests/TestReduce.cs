using System;
using NUnit.Framework;

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
    }
}

