using System.Drawing;
using DsmSuite.DsmViewer.Model.Data;
using DsmSuite.DsmViewer.Model.Collections;

namespace DsmSuite.DsmViewer.View.UserControls
{
    /// <summary>
    /// Used to represent a type/namespace rectangle displayed in the TypeUserControl
    /// </summary>
    internal class NodePanel
    {
        public TreeNode<Element> TreeNode;
        public Rectangle Bounds;

        public NodePanel(TreeNode<Element> treeNode, Rectangle bounds)
        {
            TreeNode = treeNode;
            Bounds = bounds;
        }

        public bool HitTest(Point p)
        {
            return Bounds.Contains(p);
        }
    }
}
