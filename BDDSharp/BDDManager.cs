using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Numerics;
using UCLouvain.BDDSharp.Table;

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
        public BDDNode One { get; private set; }

        /// <summary>
        /// The number of variables
        /// </summary>
        public int N
        {
            get { return _n; }
            set
            {
                _n = value;
                Zero.Index = _n;
                One.Index = _n;
            }
        }

        int _n;
        public int nextId = 0;
        IDictionary<Tuple<int, int, int>, WeakReference> _ite_cache;
        List<int> _variable_order;

        const int MIN_INIT_SIZE = 4;
        object unique_table_lock = new object();
        UniqueTable[] unique_table;

        /// <summary>
        /// The max growth factor used in Sifting algorithm.
        /// </summary>
        const double MaxGrowth = 1.2;

        /// <summary>
        /// Get the BDD node corresponding to the specified index variable, low and high identifier.
        /// </summary>
        /// <returns>The unique BDD node.</returns>
        /// <param name="index">Index.</param>
        /// <param name="low">Low.</param>
        /// <param name="high">High.</param>
        BDDNode Get(int index, int low, int high)
        {
            lock (unique_table_lock)
            {
                return unique_table[index].Get(index, low, high);
            }
        }

        /// <summary>
        /// Gets the variable order.
        /// </summary>
        /// <value>The variable order.</value>
        public int[] VariableOrder
        {
            get
            {
                return _variable_order.ToArray();
            }
        }

		/// <summary>
		/// Gets or sets the function returning the string corresponding the
        /// variable at index. This is used for debugging purpose.
		/// </summary>
		/// <value>The string corresponding to the variable.</value>
        public Func<int, string> GetVariableString
        {
            get; set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BDDSharp.BDDManager"/> class.
        /// </summary>
        /// <param name="n">The number of variables</param>
        public BDDManager(int n)
        {
            this.Zero = Create(n, false);
            this.One = Create(n, true);
            
            _n = n;
            _ite_cache = new Dictionary<Tuple<int, int, int>, WeakReference>();
            _variable_order = new List<int>(Enumerable.Range(0, n));
            if (_variable_order.Count() != n)
                throw new ArgumentException();
            
            GetVariableString = (x) => x.ToString();

            lock (unique_table_lock)
            {
                int size = (n > MIN_INIT_SIZE) ? n : MIN_INIT_SIZE;
                unique_table = new UniqueTable[size];
                for (int i = 0; i < unique_table.Length; i++)
                    unique_table[i] = new UniqueTable();
            }
        }

        /// <summary>
        /// Create the BDD Node corresponding to the variable index, with
        /// high and low children.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="index">Index.</param>
        /// <param name="high">High.</param>
        /// <param name="low">Low.</param>
        public BDDNode Create(int index, int high, BDDNode low)
        {
            return Create(index, high == 0 ? Zero : One, low);
        }

        /// <summary>
        /// Create the BDD Node corresponding to the variable index, with
        /// high and low children.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="index">Index of the variable.</param>
        /// <param name="high">High node, or 1-node.</param>
        /// <param name="low">Low node, or 0-node.</param>
        public BDDNode Create(int index, BDDNode high, int low)
        {
            return Create(index, high, low == 0 ? Zero : One);
        }

        /// <summary>
        /// Create the BDD Node corresponding to the variable index, with
        /// high and low children.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="index">Index of the variable.</param>
        /// <param name="high">High node, or 1-node.</param>
        /// <param name="low">Low node, or 0-node.</param>
        public BDDNode Create(int index, int high, int low)
        {
            return Create(index, high == 0 ? Zero : One, low == 0 ? Zero : One);
        }

        /// <summary>
        /// Create the BDD Node corresponding to the variable index, with
        /// high and low children.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="index">Index of the variable.</param>
        /// <param name="high">High node, or 1-node.</param>
        /// <param name="low">Low node, or 0-node.</param>
        public BDDNode Create(int index, BDDNode high, BDDNode low)
        {
            BDDNode unique;
            lock (unique_table_lock)
            {
                unique = unique_table[index][index, low.Id, high.Id];
                if (unique != null)
                    return unique;
            }
            
            unique = new BDDNode(index, high, low) { Id = nextId++ };
            high.RefCount++;
            low.RefCount++;
            
            lock (unique_table_lock)
                unique_table[index].Put(unique);
            return unique;
        }

        /// <summary>
        /// Create the sink node corresponding to the specified value.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="index">The index for the sink node (shall be 
        //  <c>n+1</c> where <c>n</c> is the number of variables).</param>
        /// <param name="value">Low node, or 0-node.</param>
        public BDDNode Create(int index, bool value)
        {
            return new BDDNode(index, value) { Id = nextId++ };
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        /// <returns>The variable index.</returns>
        public int CreateVariable()
        {

            lock (unique_table_lock)
            {
                if (N + 1 > unique_table.Length)
                {
                    ResizeUniqueTable(unique_table.Length * 2);
                }
            }

            var temp = N;
            N++;
            _variable_order.Add(temp);
            return temp;
        }

        /// <summary>
        /// Resizes the unique table to the new specified size.
        /// </summary>
        /// <param name="new_size">New size of the unique table.</param>
        void ResizeUniqueTable(int new_size)
        {

            lock (unique_table_lock)
            {
                var n = new UniqueTable[new_size];
                for (int i = 0; i < new_size; i++)
                {
                    if (i < unique_table.Length)
                        n[i] = unique_table[i];
                    else
                        n[i] = new UniqueTable();
                }
                unique_table = n;
            }
        }

        /// <summary>
        /// Swap the specified variables
        /// </summary>
        /// <remarks>The two variables shall be adjacent. <c>index</c> shall be
        /// followed by<c>index2</c> in the variable order.</remarks>
        /// <param name="node">The root node of the BDD.</param>
        /// <param name="index">Variable index.<param>
        /// <param name="index2">Variable index.</param>
        public BDDNode Swap(BDDNode node, int index, int index2)
        {
            var i = _variable_order.FindIndex(x => x == index) + 1;
            if (i >= _variable_order.Count())
                throw new ArgumentException("'" + index + "' is the last variable in the variable order.");

            var nextIndex = _variable_order[i];
            if (index2 != nextIndex)
                throw new ArgumentException("Cannot swap variables not adjacents.");

            // ntm: ignore 008b: read substitution Int32.V_2 => Int32.index2
            _variable_order[i - 1] = nextIndex;
            _variable_order[i] = index;

            BDDNode[] nodesAtIndex;
            lock (unique_table_lock)
            {
                nodesAtIndex = unique_table[index].Nodes().ToArray();
            }
            foreach (var n in nodesAtIndex)
            {
                SwapStep(n, index, nextIndex);
            }
            
            return node;
        }

        void SwapStep(BDDNode node, int currentIndex, int nextIndex)
        {
            if (node.Value != null) // ntm: ignore 000d: Or => Xor
                return;

            if (node.Index != currentIndex)
            {
                throw new Exception(
                    string.Format("Got {0} and should be {1} in the unique table.", 
                                  node.Index, currentIndex));
            }

            if (node.High.Index != nextIndex & node.Low.Index != nextIndex)
                return;

            BDDNode f11, f10, f01, f00;
            if (node.High.Index == nextIndex)
            {
                f11 = node.High.High;
                f10 = node.High.Low;
            }
            else
            {
                f11 = node.High;
                f10 = node.High;
            }

            if (node.Low.Index == nextIndex)
            {
                f01 = node.Low.High;
                f00 = node.Low.Low;
            }
            else
            {
                f01 = node.Low;
                f00 = node.Low;
            }

            BDDNode a;
            BDDNode b;
            if (f11 == f01)
                a = f11;
            else
                a = Create(node.Index, f11, f01);

            if (f10 == f00)
                b = f10;
            else
                b = Create(node.Index, f10, f00);

            BDDNode old_low;
            BDDNode old_high;
            lock (unique_table_lock)
            {
                unique_table[node.Index].Delete(node);

                node.Index = nextIndex;

                old_low = node.Low;
				old_high = node.High;
            
				old_low.RefCount--;
				if (node.Low.RefCount == 0)
				{
					DeleteNode(node.Low);
				}
				
				old_high.RefCount--;
				if (node.High.RefCount == 0)
				{
					DeleteNode(node.High);
				}
                
                node.SetHigh(a);
                node.SetLow(b);

                unique_table[nextIndex].Put(node);
            }
        }
        
        /// <summary>
        /// Deletes the specified node.
        /// </summary>
        /// <param name="node">Node to delete.</param>
        public void DeleteNode (BDDNode node)
        {
            if (node.Value != null) return;
            if (node.RefCount != 0) return;
            
            lock (unique_table_lock)
            {
                unique_table[node.Index].Delete(node);
                node.Low.RefCount--;
                if (node.Low.RefCount == 0)
                    DeleteNode(node.Low);

                node.High.RefCount--;
                if (node.High.RefCount == 0)
                    DeleteNode(node.High);
            }
        }

        /// <summary>
        /// Applies the sifting algorithm to reduce the size of the BDD
        /// by changing the variable order.
        /// </summary>
        /// <returns>The BDD with the new variable order.</returns>
        /// <param name="root">The BDD to reduce.</param>
        public BDDNode Sifting(BDDNode root)
        {
            var initial_size = GetSize(root);
        
            var reverse_order = new int[N];
            for (int i = 0; i < N; i++)
                reverse_order[_variable_order[i]] = i;

            for (int i = 0; i < N; i++)
            {
                // Move variable xi through the order
                int opt_size = GetSize(root);
                int opt_pos, cur_pos, startpos = reverse_order[i];
                opt_pos = startpos;
                cur_pos = startpos;

                for (int j = startpos - 1; j >= 0; j--)
                {
                    cur_pos = j;
                    Swap(root, _variable_order[j], _variable_order[j + 1]);
                    
                    var new_size = GetSize(root);
                    if (new_size < opt_size)
                    {
                        opt_size = new_size;
                        opt_pos = j;
                    }
                    else if (new_size > MaxGrowth * opt_size)
                        break;
                }

                for (int j = cur_pos + 1; j < N; j++)
                {
                    cur_pos = j;
                    Swap(root, _variable_order[j - 1], _variable_order[j]);
                    var new_size = GetSize(root);
                    if (new_size < opt_size)
                    {
                        opt_size = new_size;
                        opt_pos = j;
                    }
                    else if (new_size > MaxGrowth * opt_size)
                        break;
                }

                if (cur_pos > opt_pos)
                {
                    for (int j = cur_pos - 1; j >= opt_pos; j--)
                    {
                        Swap(root, _variable_order[j], _variable_order[j + 1]);
                    }
                }
                else
                {
                    for (int j = cur_pos + 1; j <= opt_pos; j++)
                    {
                        Swap(root, _variable_order[j - 1], _variable_order[j]);
                    }
                }
            }
            return root;
        }
        
        /// <summary>
        /// Returns the size of BDD.
        /// </summary>
        /// <returns>The size.</returns>
        /// <param name="root">The BDD.</param>
        public int GetSize(BDDNode root)
        {
            return GetSize(root, new HashSet<int>());
        }

        int GetSize(BDDNode n, ISet<int> visited)
        {
            if (n == null) return 0;
            if (visited.Contains(n.Id))
                return 0;
            visited.Add(n.Id);
            return GetSize(n.Low, visited) + GetSize(n.High, visited) + 1;
        }

        /// <summary>
        /// Reduce the specified BDD.
        /// </summary>
        /// <param name="root">BDD to reduce.</param>
        public BDDNode Reduce(BDDNode root)
        {
            var nodes = root.Nodes.ToArray();
            var size = nodes.Length;

            var subgraph = new BDDNode[size];
            var vlist = new List<BDDNode>[N + 1];

            for (int i = 0; i < size; i++)
            {
                if (vlist[nodes[i].Index] == null)
                    vlist[nodes[i].Index] = new List<BDDNode>();

                vlist[nodes[i].Index].Add(nodes[i]);
            }

            int nextid = -1;
            for (int k = N; k >= 0; k--)
            {
                int i = (k == N) ? N : _variable_order[k];
                var Q = new List<BDDNode>();
                if (vlist[i] == null)
                    continue;

                foreach (var u in vlist[i])
                {
                    if (u.Index == N)
                    {
                        Q.Add(u);
                    }
                    else
                    {
                        if (u.Low.Id == u.High.Id)
                        {
                            u.Id = u.Low.Id;
                        }
                        else
                        {
                            Q.Add(u);
                        }
                    }
                }

                Q.Sort((x, y) =>
                {
                    var xlk = x.Key.Item1;
                    var xhk = x.Key.Item2;
                    var ylk = y.Key.Item1;
                    var yhk = y.Key.Item2;
                    int res = xlk.CompareTo(ylk);
                    return res == 0 ? xhk.CompareTo(yhk) : res;
                });

                var oldKey = new Tuple<int, int>(-2, -2);
                foreach (var u in Q)
                {
                    if (u.Key.Equals(oldKey))
                    {
                        u.Id = nextid;
                    }
                    else
                    {
                        nextid++;
                        u.Id = nextid;
                        subgraph[nextid] = u;
                        ////Console.WriteLine(u.Low?.Id.ToString() ?? "null" );
                        u.Low = u.Low == null ? null : subgraph[u.Low.Id];
                        u.High = u.High == null ? null : subgraph[u.High.Id];
                        oldKey = u.Key;
                    }
                }
            }
            return subgraph[root.Id];
        }

        /// <summary>
        /// Restrict the specified bdd using the <c>positive</c> and 
        /// <c>negative</c> sets.
        /// </summary>
        /// <param name="n">Node.</param>
        /// <param name="positive">Index of positive variable.</param>
        /// <param name="negative">Index of negative variable.</param>
        /// <param name="cache">Cache.</param>
        public BDDNode Restrict(BDDNode n, int positive, int negative, BDDNode[] cache = null)
        {
            if (n.Value != null)
                return n;

            BDDNode cached;
            if (cache == null)
                cache = new BDDNode[nextId];
            else if ((cached = cache[n.Id]) != null)
                return cached;

            BDDNode ret;
            if (negative == n.Index)
            {
                ret = n.Low; // Restrict(n.Low, positive, negative, cache);
            }
            else if (positive == n.Index)
            {
                ret = n.High; // Restrict(n.High, positive, negative, cache);
            }
            else
            {
                n.Low = Restrict(n.Low, positive, negative, cache);
                n.High = Restrict(n.High, positive, negative, cache);
                ret = n;
                cache[n.Id] = ret;
            }

            return ret;
        }

        /// <summary>
        /// Performs the and operation between the specified f and g.
        /// </summary>
        /// <param name="f">The left node.</param>
        /// <param name="g">The right node.</param>
        public BDDNode And(BDDNode f, BDDNode g)
        {
            return ITE(f, g, Zero);
        }

        /// <summary>
        /// Performs the or operation between the specified f and g.
        /// </summary>
        /// <param name="f">The left node.</param>
        /// <param name="g">The right node.</param>
        public BDDNode Or(BDDNode f, BDDNode g)
        {
            return ITE(f, One, g);
        }

        /// <summary>
        /// Performs the If-Then-Else operation on nodes <c>f</c>, <c>g</c>, 
        /// <c>h</c>.
        /// </summary>
        /// <param name="f">Node.</param>
        /// <param name="g">Node.</param>
        /// <param name="h">Node.</param>
        public BDDNode ITE(BDDNode f, BDDNode g, BDDNode h)
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

            var cache_key = new Tuple<int, int, int>(f.Id, g.Id, h.Id);
            if (_ite_cache.ContainsKey(cache_key))
            {
                WeakReference wr = _ite_cache[cache_key];
                if (wr.IsAlive) return (BDDNode)wr.Target;
                else _ite_cache.Remove(cache_key);
            }

            int index = f.Index;
            if (g.Index < index)
                index = g.Index;
            if (h.Index < index)
                index = h.Index;

            var fv0 = Restrict(f, -1, index);
            var gv0 = Restrict(g, -1, index);
            var hv0 = Restrict(h, -1, index);

            var fv1 = Restrict(f, index, -1);
            var gv1 = Restrict(g, index, -1);
            var hv1 = Restrict(h, index, -1);

            BDDNode node = Create(index, 
                                  ITE(fv1, gv1, hv1), 
                                  ITE(fv0, gv0, hv0));
            _ite_cache[cache_key] = new WeakReference(node);
            return node;
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
            return Create(n.Index, Negate(n.High), Negate(n.Low));
        }

        /// <summary>
        /// Returns the dot representation of the given node.
        /// </summary>
        /// <returns>The dot code.</returns>
        public string ToDot(BDDNode root, 
                            Func<BDDNode, string> labelFunction = null,
                            bool show_all = true)
        {
            lock (unique_table_lock)
            {
	            var nodes = root.Nodes.Distinct().ToList();
	            var t = new StringBuilder("digraph G {\n");
	
	            if (labelFunction == null)
	                labelFunction = (x) => GetVariableString(x.Index);
	
	            for (int i = 0; i < N; i++)
	            {
	                t.Append($"\tsubgraph cluster_box_{i} {{\n");
	                t.Append("\tstyle=invis;\n");
	                //foreach (var n in nodes.Where(x => x.Index == i))
	                foreach (var n in unique_table[i].Nodes())
	                {
	                    var color = "grey";
	                    if (nodes.Contains(n))
	                    {
	                        color = "black";
	                    }

                        if (show_all || nodes.Contains(n))
		                    t.Append($"\t\t{n.Id} [label=\"{labelFunction(n)}\", " 
		                              + $"color=\"{color}\"];\n");
	                }
	                t.Append("\t}\n");
	            }
	
	            t.Append("\tsubgraph cluster_box_sink {\n");
	            t.Append($"\t{Zero.Id} [shape=box,label=\"0 ({Zero.RefCount})\"];\n");
	            t.Append($"\t{One.Id} [shape=box,label=\"1 ({One.RefCount})\"];\n");
	            t.Append("\t}\n");
	
	            //foreach (var n in nodes)
	            for (int i = 0; i < N; i++)
	            {
	                foreach (var n in unique_table[i].Nodes())
	                {
	                    var color = "grey";
	                    if (nodes.Contains(n))
	                    {
	                        color = "black";
	                    }
	                    if (n.Index < N && (show_all || nodes.Contains(n)))
	                    {
	                        t.Append($"\t{n.Id} -> {n.High.Id} [color=\"{color}\"];\n");
	                        t.Append($"\t{n.Id} -> {n.Low.Id} [style=dotted,color=\"{color}\"];\n");
	                    }
	                }
	            }
	            t.Append("}");
	            return t.ToString();
	        }
        }
    }
}

