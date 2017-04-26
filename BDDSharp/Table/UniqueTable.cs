using System;
using System.Collections.Generic;
using System.Linq;
using UCLouvain.BDDSharp;

namespace UCLouvain.BDDSharp.Table
{

    public class UniqueTable
    {
    
        /// <summary>
        /// The initial capacity.
        /// </summary>
        static readonly int INIT_CAPACITY = 4;

        /// <summary>
        /// The number of key-value pair.
        /// </summary>
        int N;
        
        /// <summary>
        /// The size of the hashtable.
        /// </summary>
        int M;
        
        /// <summary>
        /// The array with the buckets.
        /// </summary>
        BDDNodeBucket[] buckets;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="T:UCLouvain.BDDSharp.UniqueTable"/> class.
        /// </summary>
        public UniqueTable() : this(INIT_CAPACITY)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:UCLouvain.BDDSharp.UniqueTable"/> class.
        /// </summary>
        /// <param name="M">The initial number of buckets.</param>
        public UniqueTable(int M)
        {
            this.M = M;
            buckets = new BDDNodeBucket[M];
            for (int i = 0; i < M; i++)
                buckets[i] = new BDDNodeBucket();
        }

        /// <summary>
        /// Resize the hash table to the specified size.
        /// </summary>
        /// <param name="size">Chains.</param>
        void resize(int size)
        {
            UniqueTable temp = new UniqueTable(size);
            for (int i = 0; i < M; i++)
            {
                foreach (var node in buckets[i].Nodes())
                {
                    temp.Put(node.Index, node.Low.Id, node.High.Id, node);
                }
            }
            this.M = temp.M;
            this.N = temp.N;
            this.buckets = temp.buckets;
        }

        /// <summary>
        /// Removes the dead references from the table.
        /// </summary>
        public void RemoveDead()
        {
            for (int i = 0; i < M; i++)
            {
                buckets[i].RemoveDead();
            }
        }

        /// <summary>
        /// Hash the specified index, low and high identifier to produce an 
        /// index.
        /// </summary>
        /// <returns>The hash.</returns>
        /// <param name="index">Index.</param>
        /// <param name="low">Low.</param>
        /// <param name="high">High.</param>
        private int hash(int index, int low, int high)
        {
            return ((17 + index + 23 * (low + 23 * high)) & 0x7fffffff) % M;
        }

        /// <summary>
        /// Returns the number of key-value pairs in this symbol table.</summary>
        /// <returns>the number of key-value pairs in this symbol table</returns>
        public int Count
        {
            get { return N; }
        }

        /// <summary>
        /// Returns true if this symbol table is empty.</summary>
        /// <returns><c>true</c> if this symbol table is empty;
        ///        <c>false</c> otherwise</returns>
        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        /// <summary>
        /// Indexer wrapping <c>Get</c> and <c>Put</c> for convenient syntax
        /// </summary>
        /// <param name="key">key the key </param>
        /// <returns>value associated with the key</returns>
        /// <exception cref="NullReferenceException">null reference being used for value type</exception>
        public BDDNode this[int index, int low, int high]
        {
            get
            {
                return Get(index, low, high);
            }
        }

        /// <summary>
        /// Returns true if this symbol table contains the specified key.</summary>
        /// <param name="key">the key</param>
        /// <returns><c>true</c> if this symbol table contains <c>key</c>;
        ///        <c>false</c> otherwise</returns>
        /// <exception cref="ArgumentNullException">if <c>key</c> is <c>null</c></exception>
        public bool Contains(int index, int low, int high)
        {
            return Get(index, low, high) != null;
        }

        /// <summary>
        /// Returns the value associated with the specified key in this symbol table.</summary>
        /// <param name="key">the key</param>
        /// <returns>the value associated with <c>key</c> in the symbol table;
        ///        <c>null</c> if no such value</returns>
        /// <exception cref="ArgumentNullException">if <c>key</c> is <c>null</c></exception>
        public BDDNode Get(int index, int low, int high)
        {
            int i = hash(index, low, high);
            return buckets[i].Get(index, low, high);
        }

        /// <summary>
        /// Put the specified node in the table.
        /// </summary>
        /// <param name="val">Value.</param>
        public void Put(BDDNode val)
        {
            Put(val.Index, val.Low?.Id ?? -1, val.High?.Id ?? -1, val);
        }

        /// <summary>
        /// Put the specified node at index, low, and high identifier.
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

            // double table size if average length of list >= 10
            if (N >= 10 * M) resize(2 * M);

            int i = hash(index, low, high);
            if (!buckets[i].Contains(index, low, high)) N++;
            buckets[i].Put(index, low, high, val);
        }
    
        /// <summary>
        /// Delete the specified node from the table.
        /// </summary>
        /// <returns>The delete.</returns>
        /// <param name="node">Node.</param>
        public void Delete(BDDNode node)
        {
            Delete(node.Index, node.Low?.Id ?? -1, node.High?.Id ?? -1);
        }

        /// <summary>
        /// Delete the node at specified index, low and high identifier.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="low">Low.</param>
        /// <param name="high">High.</param>
        public void Delete(int index, int low, int high)
        {
            int i = hash(index, low, high);
            if (buckets[i].ContainsKey(index, low, high)) { N--; }
            buckets[i].Delete(index, low, high);

            // halve table size if average length of list <= 2
            if (M > INIT_CAPACITY && N <= 2 * M) resize(M / 2);
        }

        /// <summary>
        /// Returns the nodes contained in the table.
        /// </summary>
        /// <returns>The nodes.</returns>
        public IEnumerable<BDDNode> Nodes()
        {
            for (int i = 0; i < M; i++)
            {
                foreach (var n in buckets[i].Nodes())
                {
                    yield return n;
                }
            }
        }
    }
}