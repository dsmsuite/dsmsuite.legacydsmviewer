using System;
using System.Collections.Generic;
using DsmSuite.DsmViewer.Model.Data;

namespace DsmSuite.DsmViewer.Model.Collections
{
    public class Tree<T>
    {
        private readonly TreeNode<T> _rootTreeNode;

        public Tree(T rootElement)
        {
            _rootTreeNode = new TreeNode<T>(rootElement);
        }

        public IDictionary<T, TreeNode<T>> Lookup = new Dictionary<T, TreeNode<T>>();

        public TreeNode<T> RootTree => _rootTreeNode;

        public int Count { get; } = 0;

        public TreeNode<T> CreateNode(T theNodeValue)
        {
            TreeNode<T> newTreeNode = new TreeNode<T>(theNodeValue);
            return newTreeNode;
        }

        public void AddFirst(TreeNode<T> parent, TreeNode<T> treeNode)
        {
            if (treeNode == null)
                throw new ArgumentNullException(nameof(treeNode));

            TreeNode<T> realParent = parent ?? _rootTreeNode;
            TreeNode<T> first = realParent.FirstChild;

            treeNode.Parent = realParent;
            realParent.ChildCount++;

            if (first == null)
            {
                realParent.FirstChild = treeNode;

            }
            else
            {
                treeNode.NextSibling = first;
                first.PreviousSibling = treeNode;
                realParent.FirstChild = treeNode;
            }

            if (Lookup.ContainsKey(treeNode.NodeValue) == false)
                Lookup.Add(treeNode.NodeValue, treeNode);
        }

        public void AddLast(TreeNode<T> parent, TreeNode<T> treeNode)
        {
            if (treeNode == null)
                throw new ArgumentNullException(nameof(treeNode));

            TreeNode<T> realParent = parent ?? _rootTreeNode;
            TreeNode<T> last = realParent.LastChild;

            treeNode.Parent = realParent;
            realParent.LastChild = treeNode;
            realParent.ChildCount++;

            if (last == null)
            {
                realParent.FirstChild = treeNode;

            }
            else
            {
                last.NextSibling = treeNode;
                treeNode.PreviousSibling = last;
            }

            if (Lookup.ContainsKey(treeNode.NodeValue) == false)
                Lookup.Add(treeNode.NodeValue, treeNode);
        }

        public void Remove(TreeNode<T> treeNode)
        {
            if (treeNode == null)
                throw new ArgumentNullException(nameof(treeNode));

            treeNode.Parent.ChildCount--;

            if (treeNode.PreviousSibling == null)
            {
                treeNode.Parent.FirstChild = treeNode.NextSibling;
            }
            else
            {
                treeNode.PreviousSibling.NextSibling = treeNode.NextSibling;
            }

            if (treeNode.NextSibling == null)
            {
                treeNode.Parent.LastChild = treeNode.PreviousSibling;
            }
            else
            {
                treeNode.NextSibling.PreviousSibling = treeNode.PreviousSibling;
            }

            treeNode.Parent = null;
            treeNode.PreviousSibling = null;
            treeNode.NextSibling = null;

            Lookup.Remove(treeNode.NodeValue);
        }

        public void InsertBefore(TreeNode<T> treeNode, TreeNode<T> position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            if (treeNode == null)
                throw new ArgumentNullException(nameof(treeNode));

            TreeNode<T> parent = position.Parent;
            treeNode.Parent = parent;
            parent.ChildCount++;

            if (parent.FirstChild == position)
            {
                parent.FirstChild = treeNode;
                treeNode.NextSibling = position;
                position.PreviousSibling = treeNode;
            }
            else
            {
                treeNode.NextSibling = position;
                treeNode.PreviousSibling = position.PreviousSibling;
                position.PreviousSibling.NextSibling = treeNode;
                position.PreviousSibling = treeNode;
            }

            Lookup.Add(treeNode.NodeValue, treeNode);
        }
    }
}
