using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDDSharp
{
    /// <summary>
    /// Represents a BDD node.
    /// </summary>
    public class BDDNode
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public bool? Value { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>The index.</value>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the low node, i.e. the node to follow when variable <c>index</c> is false.
        /// </summary>
        /// <value>The low.</value>
        public BDDNode Low { get; set; }

        /// <summary>
        /// Gets or sets the high node, i.e. the node to follow when variable <c>index</c> is true.
        /// </summary>
        /// <value>The high.</value>
        public BDDNode High { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="BDDSharp.BDDNode"/> has been visited or not.
        /// </summary>
        /// <value><c>true</c> if visited; otherwise, <c>false</c>.</value>
        public bool Mark { get; set; }

        /// <summary>
        /// Gets all the nodes, including descendants.
        /// </summary>
        /// <value>The nodes.</value>
        public IEnumerable<BDDNode> Nodes {
            get {
                if (Low == null && High == null) {
                    return new [] { this };
                } else {
                    return new [] { this }.Union(Low.Nodes.Union(High.Nodes));
                }
            }
        }

        /// <summary>
        /// Gets the key composed by <c>(Low.Id, High.Id)</c>
        /// </summary>
        /// <value>The key.</value>
        public Tuple<int, int> Key {
            get {
                if (IsZero) return new Tuple <int, int>(-1, -1);
                if (IsOne) return new Tuple <int, int>(-1, 0);
                return new Tuple <int, int>(Low.Id, High.Id);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is the node one.
        /// </summary>
        /// <value><c>true</c> if this instance is one; otherwise, <c>false</c>.</value>
        public bool IsOne {
            get {
                return Value != null && ((bool)Value) == true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is the node zero.
        /// </summary>
        /// <value><c>true</c> if this instance is zero; otherwise, <c>false</c>.</value>
        public bool IsZero {
            get { return Value != null && ((bool)Value) == false; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BDDSharp.BDDNode"/> class.
        /// </summary>
        public BDDNode ()
        {
            this.Mark = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BDDSharp.BDDNode"/> class.
        /// </summary>
        /// <param name="index">Index of the variable the node represents</param>
        /// <param name="high">The high node (aka 1-node).</param>
        /// <param name="low">The low node (aka 0-node).</param>
        public BDDNode (int index, BDDNode high, BDDNode low) : this ()
        {
            this.Index = index;
            this.High = high;
            this.Low = low;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BDDSharp.BDDNode"/> class.
        /// </summary>
        /// <param name="index">The index for the sink node (shall be <c>n+1</c> where <c>n</c> is the number of variables).</param>
        /// <param name="value">Value represented by the sink node.</param>
        public BDDNode (int index, bool value) : this ()
        {
            this.Value = value;
            this.Index = index;
            this.Low = null;
            this.High = null;
        }

        /// <summary>
        /// Sets the identifiers of this node and its descendant.
        /// </summary>
        /// <returns>The identifiers.</returns>
        /// <param name="i">The index to start from.</param>
        public int SetIdentifiers(int i = 0)
        {
            Mark = !Mark;
            Id = i;
            int tmp = i;
            if (Low != null && Low.Mark != Mark)
                tmp = Low.SetIdentifiers(tmp + 1);
            if (High != null && High.Mark != Mark)
                tmp = High.SetIdentifiers(tmp + 1);
            return tmp;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="BDDSharp.BDDNode"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="BDDSharp.BDDNode"/>.</returns>
        public override string ToString()
        {
            return string.Format("[Node: Identifier={0}, Value={1}, Index={2}, Low={3}, High={4}]", 
                Id, Value, Index, 
                Low != null ? Low.Id.ToString() : "null", 
                High != null ? High.Id.ToString() : "null");
        }
    }
}

