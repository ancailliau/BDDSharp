using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDDSharp
{
    /// <summary>
    /// Represents a BDD
    /// </summary>
    public class BDDManager
    {
        /// <summary>
        /// Gets the node representing zero.
        /// </summary>
        /// <value>The node zero.</value>
        public BDDNode Zero { get; private set; }

        /// <summary>
        /// Gets the node representing one.
        /// </summary>
        /// <value>The node one.</value>
        public BDDNode One  { get; private set; }

        /// <summary>
        /// The number of variables
        /// </summary>
        public int N;

        /// <summary>
        /// Initializes a new instance of the <see cref="BDDSharp.BDDManager"/> class.
        /// </summary>
        /// <param name="n">The number of variables</param>
        public BDDManager (int n)
        {
            this.N = n;
            this.Zero = new BDDNode (n + 1, false);
            this.One = new BDDNode (n + 1, true);
        }

        /*
        void RedirectEdges(List<Node> nodes, Node oldNode, Node newNode)
        {
            var incomingLow = nodes.Where(x => x.Low != null && x.Low.Identifier == oldNode.Identifier);
            foreach (var iv in incomingLow)
                iv.Low = newNode;
            
            var incomingHigh = nodes.Where(x => x.High != null && x.High.Identifier == oldNode.Identifier);
            foreach (var iv in incomingHigh) {
                iv.High = newNode;
            }
        }

        public void Reduce(Node n)
        {
            var nodes = n.Nodes.ToList();
            n.SetIdentifiers(0);
            for (int i = N; i >= 0; i--) {
                var Vi = nodes.Where(x => x.Index == i).ToList();
                var Vi2 = nodes.Where(x => x.Index == i).ToList();

                // Elimination rule
                foreach (var v in Vi) {
                    if (v.Low.Identifier == v.High.Identifier) {
                        Vi2.Remove(v);
                        RedirectEdges(nodes, v, v.Low);
                        if (v.Identifier == n.Identifier) {
                            Root = v.Low;
                        }
                    }
                }

                // Merging rule
                var oldKey = new Tuple <int, int>(0, 0);
                Node oldNode = null;
                foreach (var v in Vi2) {
                    var key = v.Key;
                    if (key.Item1 == oldKey.Item1 & key.Item2 == oldKey.Item2) {
                        //Vi.Remove (v);
                        RedirectEdges(nodes, v, oldNode);
                        if (v.Identifier == n.Identifier) {
                            Root = oldNode;
                        }
                    } else {
                        oldNode = v;
                        oldKey = v.Key;
                    }
                }
            }
        }
        */

        /// <summary>
        /// Restrict the specified bdd using the < c>positive</ c> and < c>negative</ c> sets.
        /// </summary>
        /// <param name="n">Node.</param>
        /// <param name="positive">Indexes of positive variables.</param>
        /// <param name="negative">Indexes of negative variables.</param>
        /// <param name="cache">Cache.</param>
        public BDDNode Restrict (BDDNode n, ISet<int> positive, ISet<int> negative, Dictionary<BDDNode, BDDNode> cache = null)
        {
            if (n.IsOne | n.IsZero)
                return n;

            if (cache == null)
                cache = new Dictionary<BDDNode, BDDNode>();

            if (cache.ContainsKey(n))
                return cache[n];

            BDDNode ret;
            if (negative.Contains(n.Index))
                ret = Restrict(n.Low, positive, negative, cache);
            else if (positive.Contains(n.Index))
                ret = Restrict(n.High, positive, negative, cache);
            else {
                ret = new BDDNode {
                    Low = Restrict(n.Low, positive, negative, cache),
                    High = Restrict(n.High, positive, negative, cache)
                };
                cache[n] = ret;
            }
            return ret;
        }

        /// <summary>
        /// Performs the If-Then-Else operation on nodes < c>f</ c>, < c>g</ c>, < c>h</ c>.
        /// </summary>
        /// <param name="f">Node.</param>
        /// <param name="g">Node.</param>
        /// <param name="h">Node.</param>
        public BDDNode ITE (BDDNode f, BDDNode g, BDDNode h)
        {
            // ite(f, 1, 0) = f
            if (g.IsOne & h.IsZero)
                return f;

            // ite(f, 0, 1) = !f
            if (g.IsZero & h.IsOne)
                return Negate(f);

            // ite(1, g, h) = g
            if (f.IsOne)
                return g;

            // ite(0, g, h) = h
            if (f.IsZero)
                return h;

            // ite(f, g, g) = g
            if (g == h)
                return g;

            var index = new int[] { f.Index, g.Index, h.Index }.Min();
            var indexSet = new HashSet<int> { index };
            var emptySet = new HashSet<int> { };

            var fv0 = Restrict(f, emptySet, indexSet);
            var gv0 = Restrict(g, emptySet, indexSet);
            var hv0 = Restrict(h, emptySet, indexSet);

            var fv1 = Restrict(f, indexSet, emptySet);
            var gv1 = Restrict(g, indexSet, emptySet);
            var hv1 = Restrict(h, indexSet, emptySet);

            return new BDDNode {
                Low = ITE(fv0, gv0, hv0),
                High = ITE(fv1, gv1, hv1),
            };
        }

        /// <summary>
        /// Returns the dot representation of the given node.
        /// </summary>
        /// <returns>The dot code.</returns>
        public object ToDot(BDDNode root)
        {
            var nodes = root.Nodes.ToList();
            var t = new StringBuilder("digraph G {\n");
            foreach (var n in nodes) {
                if (n.Index <= N) {
                    t.Append("\t" + n.Id + " [label=\"x" + n.Index + " (id:" + n.Id + ")\"];\n");
                    t.Append("\t" + n.Id + " -> " + n.High.Id + ";\n");
                    t.Append("\t" + n.Id + " -> " + n.Low.Id + " [style=dotted];\n");
                } else {
                    t.Append("\t" + n.Id + " [shape=box,label=\"" + (((bool)n.Value) ? "1" : "0") + " (id:" + n.Id + ")\"];\n");
                }
            }
            t.Append("}");
            return t.ToString();
        }

        /// <summary>
        /// Negate the specified node.
        /// </summary>
        /// <param name="n">The node.</param>
        public BDDNode Negate(BDDNode n)
        {
            if (n.IsZero)
                return One;
            if (n.IsOne)
                return Zero;
            return new BDDNode { Low = Negate(n.Low), High = Negate(n.High) };
        }
    }

}

