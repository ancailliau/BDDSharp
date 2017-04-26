using System;
using System.Collections.Generic;
using System.Linq;

namespace UCLouvain.BDDSharp.Table
{
    public class BDDNodeBucket
    {
        /// <summary>
        /// The number of key-value pairs.
        /// </summary>
        int N;
        
        /// <summary>
        /// The linked list of key-value pairs
        /// </summary>
        Node first;
        
		/// <summary>
		/// The current node (using in iterator).
		/// </summary>
        Node current;

        /// <summary>
        /// Helper class for linked list nodes.
        /// </summary>
        private class Node
        {
            internal int index;
            internal int low;
            internal int high;

            internal WeakReference val;
            internal Node next;

            public Node(int index, int low, int high, BDDNode val, Node next)
            {
                this.index = index;
                this.low = low;
                this.high = high;

                this.val = new WeakReference(val);
                this.next = next;
            }
        }

        /// <summary>
        /// Gets the number of items stored in the bucket.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get { return N; }
        }
        
        /// <summary>
        /// Gets a value indicating whether this <see cref="T:UCLouvain.BDDSharp.BDDNodeBucket"/> is empty.
        /// </summary>
        /// <value><c>true</c> if is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get { return N == 0; }
        }

        /// <summary>
        /// Returns whether the bucket contains a node with the specified index,
        /// low and high identifiers. Returns <c>false</c> if the node stored
        /// at the specified key is not alive.
        /// </summary>
        /// <returns><c>true</c> if node is contained; otherwise, <c>false</c>.</returns>
        /// <param name="index">Index.</param>
        /// <param name="low">Low id.</param>
        /// <param name="high">High id.</param>
        public bool Contains(int index, int low, int high)
        {
            return Get(index, low, high) != null;
        }

        /// <summary>
        /// Returns whether the bucket contains a node with the specified index,
        /// low and high identifiers. Returns <c>true</c> if the node stored
        /// at the specified key is not alive.
        /// </summary>
        /// <returns><c>true</c> if node is contained; otherwise, <c>false</c>.</returns>
        /// <param name="index">Index.</param>
        /// <param name="low">Low id.</param>
        /// <param name="high">High id.</param>
        public bool ContainsKey(int index, int low, int high)
        {
            for (Node x = first; x != null; x = x.next)
            {
                if (index == x.index && low == x.low && high == x.high)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the dead references from the bucket.
        /// </summary>
        public void RemoveDead()
        {
            first = RemoveDead(first);
        }

        Node RemoveDead(Node n)
        {
            if (n == null) return null;
            if (!n.val.IsAlive)
            {
                N--;
                return n.next;
            }
            n.next = RemoveDead(n.next);
            return n;
        }

        /// <summary>
        /// Gets the <see cref="T:UCLouvain.BDDSharp.BDDNodeBucket"/> at the specified key.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="low">Low identifier.</param>
        /// <param name="high">High identifier.</param>
        public BDDNode this[int index, int low, int high]
        {
            get
            {
                return (BDDNode)Get(index, low, high);
            }

        }

        /// <summary>
        /// Get the node stored at specified index, low and high identifier.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="index">Index.</param>
        /// <param name="low">Low identifier.</param>
        /// <param name="high">High identifier.</param>
        public BDDNode Get(int index, int low, int high)
        {
            for (Node x = first; x != null; x = x.next)
            {
                if (index == x.index && low == x.low && high == x.high)
                {
                    if (x.val.IsAlive)
                        return (BDDNode)x.val.Target;
                }
            }
            return null;
        }

        /// <summary>
        /// Put the specified val at specified index, low and high identifier.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="low">Low.</param>
        /// <param name="high">High.</param>
        /// <param name="val">Value.</param>
        public void Put(int index, int low, int high, BDDNode val)
        {
            if (val == null)
            {
                Delete(index, low, high);
                return;
            }

            for (Node x = first; x != null; x = x.next)
            {
                if (index == x.index && low == x.low && high == x.high)
                {
                    if (x.val.IsAlive)
                        throw new BDDNodeBucketException(
                            "Trying to replace an alive reference.");
                    x.val = new WeakReference(val);
                    return;
                }
            }
            first = new Node(index, low, high, val, first);
            N++;
        }

        /// <summary>
        /// Delete the node at specified index, low and high identifier.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="low">Low.</param>
        /// <param name="high">High.</param>
        public void Delete(int index, int low, int high)
        {
            first = Delete(first, index, low, high);
        }

        // delete key in linked list beginning at Node x
        // warning: function call stack too large if table is large
        Node Delete(Node x, int index, int low, int high)
        {
            if (x == null) return null;
            if (index == x.index && low == x.low && high == x.high)
            {
                N--;
                return x.next;
            }
            x.next = Delete(x.next, index, low, high);
            return x;
        }
        
        /// <summary>
        /// Returns the nodes in the bucket.
        /// </summary>
        /// <returns>The nodes.</returns>
        public IEnumerable<BDDNode> Nodes()
        {
            current = first;
            while (current != null)
            {
                if (current.val.IsAlive)
                    yield return (BDDNode)current.val.Target;
                if (current != null)
                    current = current.next;
            }
        }
    }
}