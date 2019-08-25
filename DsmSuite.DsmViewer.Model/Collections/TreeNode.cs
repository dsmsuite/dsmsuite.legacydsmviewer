using System.Collections.Generic;
using System.Diagnostics;

namespace DsmSuite.DsmViewer.Model.Collections
{
    public class TreeNode<T>
    {
        internal int ChildCount;
        internal TreeNode<T> LastChild;

        public TreeNode(T nodeValue)
        {
            Depth = 0;
            IsCollapsed = true;
            IsHidden = false;
            NodeValue = nodeValue;
        }

        public TreeNode<T> FirstChild { get; internal set; }

        public TreeNode<T> PreviousSibling { get; internal set; }

        public TreeNode<T> NextSibling { get; internal set; }

        public TreeNode<T> Parent { get; internal set; }

        public T NodeValue { get; set; }

        public bool HasChildren => ChildCount > 0;

        public IList<TreeNode<T>> Children
        {
            get
            {
                IList<TreeNode<T>> list = new List<TreeNode<T>>(ChildCount);

                TreeNode<T> next = FirstChild;

                while (next != null)
                {
                    list.Add(next);
                    next = next.NextSibling;
                }

                return list;
            }
        }

        public bool CanCollapse => HasChildren;

        public bool IsHidden { get; set; }

        public int Depth { get; set; }

        public bool IsCollapsed { get; set; }
    }
}
