// #define PRINT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UCLouvain.BDDSharp
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
        public int N { get { return _n; } set { _n = value; Zero.Index = _n; One.Index = _n; } }

        int _n;
        int nextId = 0;
        IDictionary<Tuple<BDDNode, BDDNode, BDDNode>, BDDNode> ite_cache;
        List<int> variableOrder;

        public int[] VariableOrder {
            get {
                return variableOrder.ToArray ();
            }
        }

        public Func<int, string> GetVariableString {
            get ; set ;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BDDSharp.BDDManager"/> class.
        /// </summary>
        /// <param name="n">The number of variables</param>
        public BDDManager (int n)
        {
            this.Zero = Create (n, false);
            this.One = Create (n, true);
            this._n = n;
            ite_cache = new Dictionary<Tuple<BDDNode, BDDNode, BDDNode>, BDDNode> ();
            variableOrder = new List<int> (Enumerable.Range (0, n));
            if (variableOrder.Count() != n) {
                throw new ArgumentException ();
            }
            GetVariableString = (x) => x.ToString ();
        }

        public BDDNode Create (int index, int high, BDDNode low)
        {
            return new BDDNode (index, high == 0 ? Zero : One, low) { Id = nextId++ };
        }

        public BDDNode Create (int index, BDDNode high, int low)
        {
            return new BDDNode (index, high, low == 0 ? Zero : One) { Id = nextId++ };
        }

        public BDDNode Create (int index, int high, int low)
        {
            return new BDDNode (index, high == 0 ? Zero : One, low == 0 ? Zero : One) { Id = nextId++ };
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

        public int CreateVariable ()
        {
            var temp = N;
            N++;
            variableOrder.Add (temp);
            return temp;
        }

        /// <summary>
        /// Swap the specified variables
        /// </summary>
        /// <remarks>The two variables shall be adjacent. <c>index</c> shall be followed by <c>index2</c> in the variable order.</remarks>
        /// <param name="node">The root node of the BDD.</param>
        /// <param name="index">Variable.</param>
        /// <param name="index2">Variable.</param>
        public BDDNode Swap (BDDNode node, int index, int index2)
        {
            var i = variableOrder.FindIndex(x => x == index) + 1;
            if (i >= variableOrder.Count ())
                throw new ArgumentException ("'" + index + "' is the last variable in the variable order.");

            var nextIndex = variableOrder[i];
            if (index2 != nextIndex)
                throw new ArgumentException ("Cannot swap variables not adjacents.");
			
            // ntm: ignore 008b: read substitution Int32.V_2 => Int32.index2
			variableOrder[i - 1] = nextIndex; 
			variableOrder[i] = index;

			// ntm: ignore 00ad: read substitution Int32.V_2 => Int32.index2
			return SwapStep (node, index, nextIndex);
        }

        BDDNode SwapStep (BDDNode node, int currentIndex, int nextIndex) 
        {
            if (node.IsOne | node.IsZero) // ntm: ignore 000d: Or => Xor
				return node;

            if (node.Index != currentIndex) {
                node.Low = SwapStep (node.Low, currentIndex, nextIndex);
                node.High = SwapStep (node.High, currentIndex, nextIndex);

            } else {
                if (node.High.Index != nextIndex & node.Low.Index != nextIndex)
                    return node;

                var f11 = (node.High.Index == nextIndex) ? node.High.High : node.High;
                var f10 = (node.High.Index == nextIndex) ? node.High.Low : node.High;

                var f01 = (node.Low.Index == nextIndex) ? node.Low.High : node.Low;
                var f00 = (node.Low.Index == nextIndex) ? node.Low.Low : node.Low;

                var a = Create (node.Index, f11, f01);
                var b = Create (node.Index, f10, f00);

                node.Index = nextIndex;
                node.High = a;
                node.Low = b;
            }

            return node;
        }

        public BDDNode Sifting (BDDNode P)
        {
            var reverse_order = new int[N];
            for (int i = 0; i < N; i++) {
                reverse_order[variableOrder[i]] = i;
            }

            int file_index = 0;

            for (int i = 0; i < N; i++) {
                // Move variable xi through the order
                int opt_size = P.Nodes.Count ();
                int opt_pos, cur_pos, startpos = reverse_order[i];
                opt_pos = startpos;
                cur_pos = startpos;

                for (int j = startpos - 1; j >= 0; j--) {
                    cur_pos = j;
                    Swap (P, VariableOrder[j], VariableOrder[j+1]);
                    P = Reduce (P);
                    var new_size = P.Nodes.Count();
                    if (new_size < opt_size) {
                        opt_size = new_size;
                        opt_pos = j;
                    }/* else if (new_size > MaxGrowth * opt_size) {
                        
                    }*/
                }

                for (int j = cur_pos + 1; j < N; j++) {
                    cur_pos = j;
                    Swap (P, variableOrder[j-1], variableOrder[j]);
                    P = Reduce (P);
                    var new_size = P.Nodes.Count();
                    if (new_size < opt_size) {
                        opt_size = new_size;
                        opt_pos = j;
                    }
                }

                if (cur_pos > opt_pos) {
                    for (int j = cur_pos - 1; j >= opt_pos; j--) {
                        Swap (P, variableOrder[j], variableOrder[j+1]);
                        P = Reduce (P);
                    }
                } else {
                    for (int j = cur_pos + 1; j <= opt_pos; j++) {
                        Swap (P, variableOrder[j-1], variableOrder[j]);
                        P = Reduce (P);
                    }
                }
            }
            P = Reduce (P);
            return P;
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
            for (int k = N; k >= 0; k--) {
                int i = (k == N) ? N : VariableOrder[k];
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
        /// Performs the and operation between the specified f and g.
        /// </summary>
        /// <param name="f">The left node.</param>
        /// <param name="g">The right node.</param>
        public BDDNode And (BDDNode f, BDDNode g)
        {
            return ITE (f, g, Zero);
        }

        /// <summary>
        /// Performs the or operation between the specified f and g.
        /// </summary>
        /// <param name="f">The left node.</param>
        /// <param name="g">The right node.</param>
        public BDDNode Or (BDDNode f, BDDNode g)
        {
            return ITE (f, One, g);
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

            var cache_key = new Tuple<BDDNode, BDDNode, BDDNode> (f, g, h);
            if (ite_cache.ContainsKey(cache_key)) {
                return ite_cache[cache_key];
            }

            var index = new [] { f.Index, g.Index, h.Index }.Min();
            var indexSet = new HashSet<int> { index };
            var emptySet = new HashSet<int> { };

            var fv0 = Restrict(f, emptySet, indexSet);
            var gv0 = Restrict(g, emptySet, indexSet);
            var hv0 = Restrict(h, emptySet, indexSet);

            var fv1 = Restrict(f, indexSet, emptySet);
            var gv1 = Restrict(g, indexSet, emptySet);
            var hv1 = Restrict(h, indexSet, emptySet);

            return Create (index, ITE(fv1, gv1, hv1), ITE(fv0, gv0, hv0));
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
            return Create (n.Index, Negate(n.High), Negate(n.Low));
        }

        /// <summary>
        /// Returns the dot representation of the given node.
        /// </summary>
        /// <returns>The dot code.</returns>
        public string ToDot(BDDNode root, Func<BDDNode, string> labelFunction = null)
        {
            var nodes = root.Nodes.ToList();
            var t = new StringBuilder("digraph G {\n");

            if (labelFunction == null)
                labelFunction = (x) => GetVariableString (x.Index);

            for (int i = 0; i < N; i++) {
                t.Append("\tsubgraph cluster_box_"+i+" {\n");
                t.Append("\tstyle=invis;\n");
                foreach (var n in nodes.Where (x => x.Index == i)) {
                    t.Append("\t\t" + n.Id + " [label=\"" + labelFunction (n) + "\"];\n");
                }
                t.Append("\t}\n");
            }

            t.Append("\tsubgraph cluster_box_sink {\n");
            t.Append("\t" + Zero.Id + " [shape=box,label=\"0\"];\n");
            t.Append("\t" + One.Id + " [shape=box,label=\"1\"];\n");
            t.Append("\t}\n");

            foreach (var n in nodes) {
                if (n.Index < N) {
                    t.Append("\t" + n.Id + " -> " + n.High.Id + ";\n");
                    t.Append("\t" + n.Id + " -> " + n.Low.Id + " [style=dotted];\n");
                }
            }
            t.Append("}");
            return t.ToString();
        }
    }

}

