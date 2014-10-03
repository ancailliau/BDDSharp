using NUnit.Framework;
using System;

namespace BDDSharp.Tests
{
    [TestFixture()]
    public class TestITE
    {
        [Test()]
        public void TestSimpleITE()
        {
            var manager = new BDDManager (4);

            var b = new BDDNode (1, manager.One, manager.Zero);
            var f = new BDDNode (0, manager.One, b);

            var c = new BDDNode (2, manager.One, manager.Zero);
            var g = new BDDNode (0, c, manager.Zero);

            var d = new BDDNode (3, manager.One, manager.Zero);
            var h = new BDDNode (1, manager.One, d);

            var res = manager.ITE (f, g, h);

            Console.WriteLine (manager.ToDot (f));
            Console.WriteLine ("--");
            Console.WriteLine (manager.ToDot (g));
            Console.WriteLine ("--");
            Console.WriteLine (manager.ToDot (h));
            Console.WriteLine ("--");
            Console.WriteLine (manager.ToDot (res));
        }
    }
}

