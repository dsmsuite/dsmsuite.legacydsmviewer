
namespace DsmSuite.DsmViewer.Model.Collections
{
    public class TreeIterator<T>
    {
        private TreeNode<T> _current;

        public TreeIterator(Tree<T> theTree)
        {
            _current = theTree.RootTree;
        }

        public TreeIterator(TreeNode<T> theTreeNode)
        {
            _current = theTreeNode;
        }

        public TreeNode<T> Next()
        {
            if (_current.FirstChild != null)
            {
                _current = _current.FirstChild;
            }
            else if (_current.NextSibling != null)
            {
                _current = _current.NextSibling;
            }
            else if (_current.Parent != null)
            {
                _current = _current.Parent;

                while (_current.NextSibling == null)
                {
                    _current = _current.Parent;

                    if (_current == null) return null;
                }

                _current = _current.NextSibling;
            }
            else
            {
                _current = null;
            }

            return _current;
        }

        public TreeNode<T> Skip()
        {
            if (_current.NextSibling != null)
            {
                _current = _current.NextSibling;
            }
            else if (_current.Parent != null)
            {
                _current = _current.Parent;

                while (_current.NextSibling == null)
                {
                    _current = _current.Parent;

                    if (_current == null) return null;
                }

                _current = _current.NextSibling;
            }

            return _current;
        }
    }
}
