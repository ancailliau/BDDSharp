using System;
using System.Linq;
using NUnit.Framework;
using System.Threading;

namespace UCLouvain.BDDSharp.Tests
{
    [TestFixture()]
    public class TestLargeModel : TestBDD
    {
        BDDManager manager;
        Random r;
    
        [TestCase(3,1)]
        //[Ignore]
        [Timeout(10 * 1000)]
        public void TestLarge01 (int m1, int m2)
        {
			manager = new BDDManager(0);
			r = new Random();
			var bdd = Generate(0, m1, m2);
            
            Console.WriteLine ("Number of nodes in BDD: " + manager.GetSize(bdd) + "/" + manager.N);
            bdd = manager.Sifting(bdd);
            Console.WriteLine ("Number of nodes in BDD: " + manager.GetSize(bdd));
        }
        
        BDDNode Generate (int height, int max, int m2)
        {
            if (height > max) {
                var id = manager.CreateVariable();
                var v = manager.Create(id, manager.Zero, manager.One);
                //Console.WriteLine(manager.ToDot(v, (x) => "["+x.Id+"] x" + x.Index + " (" + x.RefCount.ToString() + ")"));
                return v;
            } else
            {
                var acc = GetAndOr(height, max, m2);
                //Console.WriteLine(manager.ToDot(acc, (x) => "["+x.Id+"] x" + x.Index + " (" + x.RefCount.ToString() + ")"));
                for (int i = 0; i < m2; i++)
                {
		            //if (r.NextDouble() > .5)
		            //    acc = manager.Or(acc, Generate(height + 1, max, m2));
		            //else
		                acc = manager.And(acc, Generate(height + 1, max, m2));
                    
                    //Console.WriteLine(manager.ToDot(acc, (x) => "["+x.Id+"] x" + x.Index + " (" + x.RefCount.ToString() + ")"));
                }
                //Console.WriteLine(manager.ToDot(acc, (x) => "["+x.Id+"] x" + x.Index + " (" + x.RefCount.ToString() + ")"));
                //acc.RefCount++;
                //manager.GarbageCollect();
                //acc.RefCount--;
                //Console.WriteLine(manager.ToDot(acc, (x) => "["+x.Id+"] x" + x.Index + " (" + x.RefCount.ToString() + ")"));
                //int v = manager.GetSize(acc);
                //acc = manager.Sifting(acc);
                //throw new Exception();
                //Console.WriteLine ("  Number of nodes: " + v + " -> " + manager.GetSize(acc));
                return acc;
            }
        }

        private BDDNode GetAndOr(int height, int max, int m2)
        {
            if (r.NextDouble() > .5)
                return manager.Or(Generate(height + 1, max, m2), Generate(height + 1, max, m2));
            else
                return manager.And(Generate(height + 1, max, m2), Generate(height + 1, max, m2));
        }
    }
}
