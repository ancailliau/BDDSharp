﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UCLouvain.BDDSharp
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
        /// Sets the low node and updates the ref count.
        /// </summary>
        /// <param name="low">Low.</param>
        public void SetLow (BDDNode low) 
        {
            Low = low;
            low.RefCount++;
        }

        /// <summary>
        /// Gets or sets the high node, i.e. the node to follow when variable <c>index</c> is true.
        /// </summary>
        /// <value>The high.</value>
        public BDDNode High { get; set; }
        
        /// <summary>
        /// Sets the high node and updates the ref count.
        /// </summary>
        /// <param name="high">High.</param>
        public void SetHigh (BDDNode high) 
        {
            High = high;
            high.RefCount++;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="BDDSharp.BDDNode"/> has been visited or not.
        /// </summary>
        /// <value><c>true</c> if visited; otherwise, <c>false</c>.</value>
        public bool Mark { get; set; }

        /// <summary>
        /// Gets or sets the reference count.
        /// </summary>
        /// <value>The reference count.</value>
        public int RefCount { get; set; }

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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BDDSharp.BDDNode"/> class.
        /// </summary>
        /// <remarks>
        /// Don't use this method to create new nodes, use <see cref="BDDSharp.BDDManager.Create(int, BDDNode, BDDNode)" />.
        /// </remarks>
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
        /// <remarks>
        /// Don't use this method to create new nodes, use <see cref="BDDSharp.BDDManager.Create(int, bool)" />.
        /// </remarks>
        /// <param name="index">The index for the sink node (shall be <c>n+1</c> where <c>n</c> is the number of variables).</param>
        /// <param name="value">Value represented by the sink node.</param>
        public BDDNode(int index, bool value) : this()
        {
            this.Value = value;
            this.Index = index;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="BDDSharp.BDDNode"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="BDDSharp.BDDNode"/>.</returns>
        public override string ToString()
        {
            return string.Format("[Node: Identifier={0}, Value={1}, Index={2}, Low={3}, High={4}, RefCount={5}]",
                Id, Value, Index,
                Low != null ? Low.Id.ToString() : "null",
                High != null ? High.Id.ToString() : "null",
                RefCount);
        }
        
        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:UCLouvain.BDDSharp.BDDNode"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:UCLouvain.BDDSharp.BDDNode"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="T:UCLouvain.BDDSharp.BDDNode"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is BDDNode) {
                var node = (BDDNode)obj;
                return node.Id == Id && node.Low == Low && node.High == High;
            }
            return false;
        }

		/// <summary>
		/// Serves as a hash function for a <see cref="T:UCLouvain.BDDSharp.BDDNode"/> object.
		/// </summary>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
        public override int GetHashCode()
        {
            if (Value != null) return (bool) Value ? 1 : 0;
            return 17 * Index + 23 * (Low.Id + 23 * High.Id);
        }
    }
}

