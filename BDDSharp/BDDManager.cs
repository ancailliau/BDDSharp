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

        private int nextId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="BDDSharp.BDDManager"/> class.
        /// </summary>
        /// <param name="n">The number of variables</param>
        public BDDManager (int n)
        {
            this.N = n;
            this.Zero = Create (n, false);
            this.One = Create (n, true);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BDDSharp.BDDNode"/> class.
        /// </summary>
        /// <param name="index">Index of the variable the node represents</param>
        /// <param name="high">The high node (aka 1-node).</param>
        /// <param name="low">The low node (aka 0-node).</param>
        public BDDNode Create (int index, BDDNode high, BDDNode low)
        {
            return new BDDNode (index, high, low) { Id = nextId++ };
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BDDSharp.BDDNode"/> class.
        /// </summary>
        /// <param name="index">The index for the sink node (shall be <c>n+1</c> where <c>n</c> is the number of variables).</param>
        /// <param name="value">Value represented by the sink node.</param>
        public BDDNode Create (int index, bool value)
        {
            return new BDDNode (index, value) { Id = nextId++ };
        }

        /// <summary>
        /// Reduce the specified BDD.
        /// </summary>
        /// <param name="v">BDD to reduce.</param>
        public BDDNode Reduce(BDDNode v)
        {
            var nodes = v.Nodes.ToArray ();
            var size = nodes.Length;

            var subgraph = new BDDNode[size];
            var vlist = new List<BDDNode>[N + 1];

            for (int i = 0; i < size; i++) {
                if (vlist[nodes[i].Index] == null) 
                    vlist[nodes[i].Index] = new List<BDDNode> ();

                vlist[nodes[i].Index].Add (nodes[i]);
            }

            int nextid = -1;
            for (int i = N; i >= 0; i--) {
                var Q = new List<BDDNode> ();
                if (vlist[i] == null)
                    continue;

                foreach (var u in vlist[i]) {
                    if (u.Index == N) {
                        Q.Add (u);
                    } else {
                        if (u.Low.Id == u.High.Id) {
                            u.Id = u.Low.Id;
                        } else {
                            Q.Add (u);
                        }
                    }
                }

                Q.Sort ((x, y) => {
                    var xlk = x.Key.Item1;
                    var xhk = x.Key.Item2;
                    var ylk = y.Key.Item1;
                    var yhk = y.Key.Item2;
                    int res = xlk.CompareTo (ylk);
                    return res == 0 ? xhk.CompareTo (yhk) : res;
                });

                var oldKey = new Tuple<int, int> (-2, -2);
                foreach (var u in Q) {
                    if (u.Key.Equals (oldKey)) {
                        u.Id = nextid;
                    } else {
                        nextid++;
                        u.Id = nextid;
                        subgraph[nextid] = u;
                        u.Low = u.Low == null ? null : subgraph[u.Low.Id];
                        u.High = u.High == null ? null : subgraph[u.High.Id];
                        oldKey = u.Key;
                    }
                }
            }
            return subgraph[v.Id];
        }

        /// <summary>
        /// Restrict the specified bdd using the <c>positive</c> and <c>negative</c> sets.
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
            if (negative.Contains(n.Index)) {
                ret = Restrict(n.Low, positive, negative, cache);
            } else if (positive.Contains(n.Index)) {
                ret = Restrict(n.High, positive, negative, cache);
            } else {
                n.Low = Restrict(n.Low, positive, negative, cache);
                n.High = Restrict(n.High, positive, negative, cache);
                ret = n;
                cache[n] = ret;
            }
            return ret;
        }

        /// <summary>
        /// Performs the If-Then-Else operation on nodes <c>f</c>, <c>g</c>, <c>h</c>.
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

            var index = new [] { f.Index, g.Index, h.Index }.Min();
            var indexSet = new HashSet<int> { index };
            var emptySet = new HashSet<int> { };

            var fv0 = Restrict(f, emptySet, indexSet);
            var gv0 = Restrict(g, emptySet, indexSet);
            var hv0 = Restrict(h, emptySet, indexSet);

            var fv1 = Restrict(f, indexSet, emptySet);
            var gv1 = Restrict(g, indexSet, emptySet);
            var hv1 = Restrict(h, indexSet, emptySet);

            return new BDDNode {
                Index = index,
                Low = ITE(fv0, gv0, hv0),
                High = ITE(fv1, gv1, hv1),
            };
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

        /// <summary>
        /// Returns the dot representation of the given node.
        /// </summary>
        /// <returns>The dot code.</returns>
        public string ToDot(BDDNode root, Func<BDDNode, string> labelFunction = null)
        {
            if (labelFunction == null) {
                labelFunction = new Func<BDDNode, string> ((n) => "x" + n.Index + " (Id: " + n.Id + ")");
            }

            var nodes = root.Nodes.ToList();
            var t = new StringBuilder("digraph G {\n");
            foreach (var n in nodes) {
                if (n.Index < N) {
                        t.Append("\t" + n.Id + " [label=\"" + labelFunction (n) + "\"];\n");
                    t.Append("\t" + n.Id + " -> " + n.High.Id + ";\n");
                    t.Append("\t" + n.Id + " -> " + n.Low.Id + " [style=dotted];\n");
                } else {
                    t.Append("\t" + n.Id + " [shape=box,label=\"" + (((bool)n.Value) ? "1" : "0") + " (id:" + n.Id + ")\"];\n");
                }
            }
            t.Append("}");
            return t.ToString();
        }
    }

}

