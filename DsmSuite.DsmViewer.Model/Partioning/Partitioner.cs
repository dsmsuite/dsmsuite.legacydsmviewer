using System.Collections.Generic;
using DsmSuite.DsmViewer.Model.Collections;
using DsmSuite.DsmViewer.Model.Data;
using DsmSuite.DsmViewer.Model.Model;

namespace DsmSuite.DsmViewer.Model.Partioning
{
    internal class Partitioner
    {
        private readonly Tree<Element> _tree;
        private readonly IDsmModel _model;
        public Partitioner(Tree<Element> tree, IDsmModel model)
        {
            _tree = tree;
            _model = model;
        }

        public void Partition(TreeNode<Element> parent)
        {
            PartitionGroup(parent.Children);
        }

        void PartitionGroup(IList<TreeNode<Element>> nodes)
        {
            if (nodes.Count > 1)
            {
                SquareMatrix matrix = BuildPartitionMatrix(nodes);

                PartitioningAlgorithm p = new PartitioningAlgorithm(matrix);

                Vector v = p.Partition();

                ReorderNodes(nodes, v);

                // System.Diagnostics.Debug.WriteLine("reorder done");
            }
        }

        SquareMatrix BuildPartitionMatrix(IList<TreeNode<Element>> nodes)
        {
            SquareMatrix matrix = new SquareMatrix(nodes.Count);

            for (int i = 0; i < nodes.Count; i++)
            {
                Element provider = nodes[i].NodeValue;

                for (int j = 0; j < nodes.Count; j++)
                {
                    if (j != i)
                    {
                        Element consumer = nodes[j].NodeValue;

                        Relation relation = _model.GetRelation(consumer, provider);

                        if (relation != null && relation.Weight > 0)
                        {
                            matrix.Set(i, j, 1);
                        }
                        else
                        {
                            matrix.Set(i, j, 0);
                        }
                    }
                }
            }

            return matrix;
        }

        void ReorderNodes(IList<TreeNode<Element>> nodes, Vector permutationVector)
        {
            TreeNode<Element> parentTreeNode = nodes[0].Parent;

            foreach (TreeNode<Element> node in parentTreeNode.Children)
            {
                _tree.Remove(node);
            }

            for (int i = 0; i < permutationVector.Size; i++)
            {
                _tree.AddLast(parentTreeNode, nodes[permutationVector.Get(i)]);
            }
        }
    }
}
