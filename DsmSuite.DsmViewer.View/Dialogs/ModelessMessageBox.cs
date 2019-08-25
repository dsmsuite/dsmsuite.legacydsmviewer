using System;
using System.Windows.Forms;

namespace DsmSuite.DsmViewer.View.Dialogs
{
    public partial class ModelessMessageBox : Form
    {
        public ModelessMessageBox( string task)
        {
            InitializeComponent();

            Task = task;
        }
        public string Task
        {
            private set { lblTask.Text = value; }
            get { return lblTask.Text;  } 
        }

        public string Message
        {
            get { return lblMessage.Text; }
            set { lblMessage.Text = value;}
        }

        private void ModelessMessageBoxLoad(object sender, EventArgs e)
        {
            BringToFront();
        }

        public void UpdateProgress( int value, string message )
        {
            if (Visible == false)
                Show();

            lblMessage.Text = message;
            progressBar1.Value = value;

            lblMessage.Refresh();
            progressBar1.Refresh();
            lblTask.Refresh();
        }
    }
}
