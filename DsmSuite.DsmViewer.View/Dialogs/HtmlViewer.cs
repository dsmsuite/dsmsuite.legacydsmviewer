using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DsmSuite.DsmViewer.View.Dialogs
{
    /// <summary>
    /// A form used for displaying Html text (can be used to display reports etc)
    /// </summary>
    public partial class HtmlViewer : Form
    {
        public HtmlViewer(Stream stream)
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            stream.Position = 0;
            webBrowser1.DocumentStream = stream;
            //webBrowser1.Navigating += new WebBrowserNavigatingEventHandler(webBrowser1_Navigating);
            webBrowser1.DocumentCompleted += WebBrowser1DocumentCompleted;
        }



        void WebBrowser1DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webBrowser1.DocumentStream.Close();
        }

        HtmlViewer()
        {
            InitializeComponent();
            Font sysFont = SystemFonts.MessageBoxFont;
            Font = new Font(sysFont.Name, sysFont.SizeInPoints, sysFont.Style);
        }

        /// <summary>
        /// Save the displayed content to a local file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSaveClick(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                CheckPathExists = true,
                DefaultExt = "html",
                AddExtension = true,
                OverwritePrompt = true,
                Filter = "Html (*.html)|*.html|All files (*.*)|*.*"
            };

            //FileInfo fi = new FileInfo(webBrowser1.Url.LocalPath);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(dlg.FileName, FileMode.Create);
                try
                {

                    Byte[] bytes = new Byte[webBrowser1.DocumentStream.Length];
                    webBrowser1.DocumentStream.Read(bytes, 0, bytes.Length);
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();

                    //fi.CopyTo( dlg.FileName, true );

                    //this.Text = "DSM Report - " +  dlg.FileName;
                    //webBrowser1.Url = new Uri(dlg.FileName);
                }
                catch (IOException ioe)
                {
                    MessageBox.Show("Error Saving File." + Environment.NewLine + ioe.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    ErrorDialog.Show(ex.ToString());
                }
                finally
                {
                    fs.Close();
                    webBrowser1.DocumentStream.Close();
                }
            }
        }

        private void BtnCloseClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}