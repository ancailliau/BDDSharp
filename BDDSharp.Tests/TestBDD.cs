using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace UCLouvain.BDDSharp.Tests
{
    public abstract class TestBDD
    {
        protected Dictionary<string, bool> BuildThruthTable (BDDManager manager, BDDNode root)
        {
            var truth = new Dictionary<string, bool>();
            AddThruthValue("", root, truth, manager.N);
            return truth;
        }
        
        protected void CheckThruthTable (Dictionary<string, bool> matrix, BDDNode node)
        {
            foreach (var kv in matrix) {
                Dictionary<int, bool> interpretation = BuildInterpretation(kv.Key);
                bool value = EvaluateBDD(node, interpretation);
                Assert.AreEqual(matrix[kv.Key], value);
            }
        }
        
        void AddThruthValue (string key, BDDNode node, Dictionary<string, bool> matrix, int acc)
        {
            if (acc == 0)
            {
                Dictionary<int, bool> interpretation = BuildInterpretation(key);
                bool value = EvaluateBDD(node, interpretation);
                matrix.Add(key, value);
                return;
            }

            AddThruthValue(key + "0", node, matrix, acc - 1);
            AddThruthValue(key + "1", node, matrix, acc - 1);
        }

        private static Dictionary<int, bool> BuildInterpretation(string key)
        {
            int index = 0;
            var interpretation = new Dictionary<int, bool>();
            foreach (var v in key)
            {
                interpretation.Add(index, v == '1');
                index++;
            }

            return interpretation;
        }

        bool EvaluateBDD(BDDNode root, Dictionary<int, bool> interpretation)
        {
            if (root.IsOne)
            {
                return true;
            }
            else if (root.IsZero)
            {
                return false;
            }
            else
            {
                var b = interpretation[root.Index];
                if (b)
                    return EvaluateBDD(root.High, interpretation);
                else
                    return EvaluateBDD(root.Low, interpretation);
            }
        }
    }
}
