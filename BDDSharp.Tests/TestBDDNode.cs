using System;
using NUnit.Framework;

namespace UCLouvain.BDDSharp.Tests
{
    [TestFixture]
    public class TestBDDNode
    {
        [Test]
        public void TestTupleValueForOne ()
		{
			var manager = new BDDManager(1);
			Assert.AreEqual(-1, manager.One.Key.Item1);
			Assert.AreEqual(0,  manager.One.Key.Item2);
		}

		[Test]
		public void TestTupleValueForZero()
		{
			var manager = new BDDManager(1);
            Assert.AreEqual(-1, manager.Zero.Key.Item1);
			Assert.AreEqual(-1, manager.Zero.Key.Item2);
		}

		[Test]
		public void TestToString()
		{
            var manager = new BDDManager(1);
            var c = manager.Create(0,manager.Zero, manager.One);

            var str = manager.One.ToString();
			StringAssert.Contains("Identifier=1", str);
			StringAssert.Contains("Value=True", str);
			StringAssert.Contains("Low=null", str);
			StringAssert.Contains("High=null", str);

            str = manager.Zero.ToString();
            StringAssert.Contains("Identifier=0", str);
            StringAssert.Contains("Value=False", str);
            StringAssert.Contains("Low=null", str);
            StringAssert.Contains("High=null", str);

            str = c.ToString();
			StringAssert.Contains("Value=", str);
			StringAssert.Contains("Low=1", str);
			StringAssert.Contains("High=0", str);
        }
    }
}
