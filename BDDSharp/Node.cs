using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDDSharp
{
    /// <summary>
    /// Represents a BDD
    /// </summary>
    public class BDD {
        
        public Node Root { get; private set; }

        public Node Zero { get; private set; }
        public Node One { get; private set; }

        public int variablesCount;
        public BDD (int variablesCount)
        {
            this.variablesCount = variablesCount;
            this.Zero = new Node (false, variablesCount);
            this.One  = new Node (true, variablesCount);
        }

        public void SetRoot (Node root)
        {
            Root = root;
            root.SetIdentifier (0);
        }

        void RedirectEdges (List<Node> nodes, Node oldNode, Node newNode)
        {
            var incomingLow = nodes.Where (x => x.Low != null && x.Low.Identifier == oldNode.Identifier);
            foreach (var iv in incomingLow)
                iv.Low = newNode;
            
            var incomingHigh = nodes.Where (x => x.High != null && x.High.Identifier == oldNode.Identifier);
            foreach (var iv in incomingHigh) {
                iv.High = newNode;
            }
        }

        public void Reduce ()
        {
            var nodes = Root.Nodes.ToList ();
            Root.SetIdentifier (0);
            for (int i = variablesCount; i >= 0; i--) {
                var Vi = nodes.Where (x => x.Index == i).ToList ();
                var Vi2 = nodes.Where (x => x.Index == i).ToList ();

                // Elimination rule
                foreach (var v in Vi) {
                    if (v.Low.Identifier == v.High.Identifier) {
                        Vi2.Remove (v);
                        RedirectEdges (nodes, v, v.Low);
                        if (v.Identifier == Root.Identifier) {
                            Root = v.Low;
                        }
                    }
                }

                // Merging rule
                var oldKey = new Tuple <int, int> (0, 0);
                Node oldNode = null;
                foreach (var v in Vi2) {
                    var key = v.Key;
                    if (key.Item1 == oldKey.Item1 & key.Item2 == oldKey.Item2) {
                        //Vi.Remove (v);
                        RedirectEdges (nodes, v, oldNode);
                        if (v.Identifier == Root.Identifier) {
                            Root = oldNode;
                        }
                    } else {
                        oldNode = v;
                        oldKey = v.Key;
                    }
                }
            }
        }

        public BDD And (BDD v2)
        {
            return BDD.Apply (this, v2, (x, y) => x & y);
        }

        public BDD Or (BDD v2)
        {
            return BDD.Apply (this, v2, (x, y) => x | y);
        }

        public static BDD Apply (BDD v1, BDD v2, Func<bool?, bool?, bool?> op)
        {
            if (v1.variablesCount != v2.variablesCount)
                throw new ArgumentException ("Cannot apply on BDD not sharing common variable set");

            v1.Root.SetIdentifier(0);
            v2.Root.SetIdentifier(0);

            var bdd = new BDD (v1.variablesCount);
            Node[,] cache = new Node[v1.Root.Nodes.Count() + 1, v2.Root.Nodes.Count() + 1];
            var root = ApplyStep (v1.Root, v2.Root, cache, op, bdd);
            bdd.SetRoot(root);
            return bdd;
        }

        static Node ApplyStep (Node v1, Node v2, Node[,] cache, Func<bool?, bool?, bool?> op, BDD bdd) 
        {
            var u = cache[v1.Identifier, v2.Identifier];
            if (u != null)
                return u;

            u = new Node (bdd.variablesCount);
            var tmp = op(v1.Value, v2.Value);
            if (tmp != null) {
                var ret = bdd.One;
                if (!(bool) tmp)
                    ret = bdd.Zero;
                cache[v1.Identifier, v2.Identifier] = ret;
                return ret;
            }
            cache[v1.Identifier, v2.Identifier] = u;

            u.Index = Math.Min (v1.Index, v2.Index);

            Node vlow1, vhigh1, vlow2, vhigh2;

            if (v1.Index == u.Index) {
                vlow1 = v1.Low;
                vhigh1 = v1.High;
            } else {
                vlow1 = v1;
                vhigh1 = v1;
            }

            if (v2.Index == u.Index) {
                vlow2 = v2.Low;
                vhigh2 = v2.High;
            } else {
                vlow2 = v2;
                vhigh2 = v2;
            }

            u.Low = ApplyStep (vlow1, vlow2, cache, op, bdd);
            u.High = ApplyStep (vhigh1, vhigh2, cache, op, bdd);

            return u;
        }

        public void Restrict (int index, bool value)
        {
            if (Root.Index == index)
                Root = value ? Root.High : Root.Low;
            else
                RestrictStep (Root, index, value);
            Reduce ();
        }

        void RestrictStep (Node n, int index, bool value)
        {
            n.Mark = !n.Mark;

            if (n.Index <= n.N) {
                if (n.Mark != n.Low.Mark) {
                    if (n.Low.Index == index)
                        n.Low = value ? n.Low.High : n.Low.Low;
                    else
                        RestrictStep (n.Low, index, value);
                }
                if (n.Mark != n.High.Mark) {
                    if (n.High.Index == index)
                        n.High = value ? n.High.High : n.High.Low;
                    else
                        RestrictStep (n.High, index, value);
                }
            }
        }

        public object ToDot ()
        {
            var nodes = Root.Nodes.ToList ();
            var t = new StringBuilder ("digraph G {\n");
            foreach (var n in nodes) {
                if (n.Index <= n.N) {
                    t.Append ("\t" + n.Identifier + " [label=\"x" + n.Index + " (id:" + n.Identifier + ")\"];\n");
                    t.Append ("\t" + n.Identifier + " -> " + n.High.Identifier + ";\n");
                    t.Append ("\t" + n.Identifier + " -> " + n.Low.Identifier + " [style=dotted];\n");
                } else {
                    t.Append ("\t" + n.Identifier + " [shape=box,label=\"" + (((bool) n.Value) ? "1" : "0") + " (id:" + n.Identifier + ")\"];\n");
                }
            }
            t.Append ("}");
            return t.ToString ();
        }
    }

    public class Node {

        public int Identifier  { get; set; }
        public bool? Value     { get; set; }
        public int Index       { get; set; }
        public Node Low        { get; set; }
        public Node High       { get; set; }
        public bool Mark       { get; set; }
        public int  N          { get; set; }

        public IEnumerable<Node> Nodes {
            get {
                if (Low == null && High == null) {
                    return new [] { this };
                } else {
                    return new [] { this }.Union (Low.Nodes.Union (High.Nodes));
                }
            }
        }

        public Tuple<int, int> Key {
            get {
                return new Tuple <int, int> (Low.Identifier, High.Identifier);
            }
        }

        public Node (int n)
        {
            this.Mark = false;
            this.N = n;
        }

        public Node (int index, int n, Node high, Node low) : this(n)
        {
            this.Index = index;
            this.High = high;
            this.Low = low;
        }

        public Node (bool value, int n) : this(n)
        {
            this.Value = value;
            this.Index = n + 1;
            this.Low = null;
            this.High = null;
        }

        public int SetIdentifier (int i)
        {
            Mark = !Mark;
            Identifier = i;
            int tmp = i;
            if (Low != null && Low.Mark != Mark)
                tmp = Low.SetIdentifier (tmp + 1);
            if (High != null && High.Mark != Mark)
                tmp = High.SetIdentifier (tmp + 1);
            return tmp;
        }

        public override string ToString ()
        {
            return string.Format ("[Node: Identifier={0}, Value={1}, Index={2}, Low={3}, High={4}]", 
                Identifier, Value, Index, 
                Low != null ? Low.Identifier.ToString () : "null", 
                High != null ? High.Identifier.ToString () : "null");
        }
    }
}

