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
        
        public Node Root { get; set; }

        public Node Zero { get; private set; }
        public Node One { get; private set; }


        int variablesCount;
        public BDD (int variablesCount)
        {
            this.variablesCount = variablesCount;
            this.Zero = new Node (0, variablesCount + 1);
            this.One  = new Node (1, variablesCount + 1);
        }

        void RedirectEdges (List<Node> nodes, Node oldNode, Node newNode)
        {
            var incomingLow = nodes.Where (x => x.Low != null && x.Low.Identifier == oldNode.Identifier);
            //Console.WriteLine ("incomingLow for " + oldNode + ":\n" + string.Join ("\n", incomingLow.Select (x => x.ToString ())));
            foreach (var iv in incomingLow)
                iv.Low = newNode;
            
            var incomingHigh = nodes.Where (x => x.High != null && x.High.Identifier == oldNode.Identifier);
            //Console.WriteLine ("incomingHigh for " + oldNode + ":\n" + string.Join ("\n", incomingHigh.Select (x => x.ToString ())));
            foreach (var iv in incomingHigh) {
                //Console.WriteLine ("Update " + iv);
                iv.High = newNode;
                //Console.WriteLine ("--> " + iv);
            }
            //Console.WriteLine ("incomingHigh for " + oldNode + ":\n" + string.Join ("\n", incomingHigh.Select (x => x.ToString ())));
        }

        public void Reduce ()
        {
            var nodes = Root.Nodes.ToList ();

            int i = 0;
            foreach (var node in nodes) {
                node.Identifier = i++;
            }

            for (i = variablesCount; i >= 0; i--) {
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

        public object ToDot ()
        {
            var nodes = Root.Nodes.ToList ();
            int i = 0;
            foreach (var node in nodes) {
                node.Identifier = i++;
            }

            var t = new StringBuilder ("digraph G {\n");
            foreach (var n in nodes) {
                if (n.Value == null) {
                    t.Append ("\t" + n.Identifier + " [label=\"x" + n.Index + "\"]");
                    t.Append ("\t" + n.Identifier + " -> " + n.High.Identifier + ";\n");
                    t.Append ("\t" + n.Identifier + " -> " + n.Low.Identifier + " [style=dotted];\n");
                } else {
                    t.Append ("\t" + n.Identifier + " [shape=box,label=\"" + n.Value + "\"];\n");
                }
            }
            t.Append ("}");
            return t.ToString ();
        }
    }

    public class Node {

        public int Identifier  { get; set; }
        public int? Value      { get; set; }
        public int Index       { get; set; }
        public Node Low        { get; set; }
        public Node High       { get; set; }

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

        public Node (int index, Node high, Node low)
        {
            this.Value = null;
            this.Index = index;
            this.High = high;
            this.Low = low;
        }

        public Node (int value, int index)
        {
            this.Value = value;
            this.Index = index;
            this.Low = null;
            this.High = null;
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

