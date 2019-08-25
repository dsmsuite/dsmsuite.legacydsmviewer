using System.Drawing;
using System.Windows.Forms;

namespace DsmSuite.DsmViewer.View.Dialogs
{
    /// <summary>
    /// Dialog for reporting unexepcted errors and sending report to author
    /// </summary>
    public partial class ErrorDialog : Form
    {
        internal ErrorDialog()
        {
            InitializeComponent();
            Font sysFont = SystemFonts.MessageBoxFont;
            Font = new Font(sysFont.Name, sysFont.SizeInPoints, sysFont.Style);
        }

        public ErrorDialog(string errorText)
        {
            InitializeComponent();

            txtBoxError.Text = errorText;
        }

        public static void Show(string msg)
        {
            ErrorDialog dlg = new ErrorDialog(msg);
            dlg.ShowDialog();
            dlg.Dispose();
        }
    }
}