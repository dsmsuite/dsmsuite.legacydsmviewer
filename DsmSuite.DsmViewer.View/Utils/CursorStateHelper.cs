using System.Windows.Forms;

namespace DsmSuite.DsmViewer.View.Utils
{
    /// <summary>
    /// Used to hold current cursor state prior to an operation and then reset it afterwards
    /// </summary>
    public class CursorStateHelper
    {
        private readonly Cursor _initialState;
        private readonly Control _ctrl;
        /// <summary>
        /// Create state for the cursor of a given control
        /// </summary>
        /// <param name="control"></param>
        /// <param name="cursor"></param>
        public CursorStateHelper(Control control, Cursor cursor)
        {
            _ctrl = control;
            _initialState = _ctrl.Cursor;
            _ctrl.Cursor = cursor;
            _ctrl.Refresh();
        }
        /// <summary>
        /// Reset the control cursor state
        /// </summary>
        public void Reset()
        {
            _ctrl.Cursor = _initialState;
            _ctrl.Refresh();
        }
    }
}
