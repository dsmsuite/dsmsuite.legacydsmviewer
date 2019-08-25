using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DsmSuite.DsmViewer.View.ViewModel;
using DsmSuite.DsmViewer.View.Dialogs;
using DsmSuite.DsmViewer.View.UserControls;
using DsmSuite.DsmViewer.View.Utils;
using DsmSuite.DsmViewer.Model.Model;
using DsmSuite.DsmViewer.Model.Exceptions;

namespace DsmSuite.DsmViewer.View.View
{
    public partial class MainView : Form
    {


        private MatrixControl _matrixControl;
        private IDsmModel _model;

        public MainView()
        {
            InitializeComponent();

            LoadMatrixControl();

            this.Closing += MainView_Closing;
        }

        private void MainView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_model != null && _model.IsModified)
            {
                string text = "The current model has some unsaved changes." +
                              Environment.NewLine + Environment.NewLine +
                              "Do you wish to save your changes now ? " +
                              Environment.NewLine + Environment.NewLine +
                              "They will be lost if you click No";

                DialogResult dr = MessageBox.Show(text, "DSM - Saved changes ?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dr.Equals(DialogResult.Yes))
                {
                    ICommand cmd = new CommandSave(_model);
                    cmd.Execute(null);
                }
            }
        }

        public bool EnableNodeUpButton
        {
            get { return upButton.Enabled; }
            set { upButton.Enabled = value; }
        }

        public bool EnableNodeDownButton
        {
            get { return downButton.Enabled; }
            set { downButton.Enabled = value; }
        }

        public bool EnablePartitionButton
        {
            get { return partitionButton.Enabled; }
            set { partitionButton.Enabled = value; }
        }

        private void LoadMatrixControl()
        {
            _matrixControl = new MatrixControl
            {
                Dock = DockStyle.Fill,
                Enabled = false,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(0, 0),
                Name = "_matrixControl",
                Size = new Size(840, 500)
            };
            panel1.Controls.Add(_matrixControl);
        }

        private void OpenButtonClick(object sender, EventArgs e)
        {
            DoProjectOpen(null);
        }

        private void SaveButtonClick(object sender, EventArgs e)
        {
            DoProjectSave();
        }

        private void UpButtonClick(object sender, EventArgs e)
        {
            _matrixControl.MoveSelectedNodeUp();
        }

        private void DownButtonClick(object sender, EventArgs e)
        {
            _matrixControl.MoveSelectedNodeDown();
        }

        private void PartitionButtonClick(object sender, EventArgs e)
        {
            DoPartitioning();
        }

        private void ShowCyclesButtonClick(object sender, EventArgs e)
        {
            _matrixControl.DisplayOptions.ShowCyclicRelations =
                !(_matrixControl.DisplayOptions.ShowCyclicRelations);
        }

        private void ReportButtonClick(object sender, EventArgs e)
        {
            DoShowReport();
        }

        private void BtnZoomDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;

            itmZoom1.Checked = false;
            itmZoom2.Checked = false;
            itmZoom3.Checked = false;
            itmZoom4.Checked = false;
            itmZoom5.Checked = false;

            if (item == itmZoom1)
            {
                _matrixControl.DisplayOptions.SetZoomLevel(1);
            }
            else if (item == itmZoom2)
            {
                _matrixControl.DisplayOptions.SetZoomLevel(2);
            }
            else if (item == itmZoom3)
            {
                _matrixControl.DisplayOptions.SetZoomLevel(3);
            }
            else if (item == itmZoom4)
            {
                _matrixControl.DisplayOptions.SetZoomLevel(4);
            }
            else
            {
                _matrixControl.DisplayOptions.SetZoomLevel(5);
            }
        }

        private void DoShowReport()
        {
            ICommand cmd = new CommandReport(_model);

            try
            {
                cmd.Execute(null);
            }
            catch (DsmException dsmEx)
            {
                MessageBox.Show("Unable to create the report for the following reason: " +
                    Environment.NewLine + Environment.NewLine + dsmEx,
                    "Dependency Structure Matrix",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception e)
            {
                ErrorDialog.Show(e.ToString());
            }
        }

        public void DoProjectOpen(FileInfo dsmFile)
        {
            if (ConfirmModelSaved())
            {
                IDsmModel newModel = new DsmModel(null, null);
                ICommand cmd = new CommandOpen(newModel, dsmFile);

                CursorStateHelper csh = new CursorStateHelper(this, Cursors.WaitCursor);

                Refresh();

                ModelessMessageBox msg = new ModelessMessageBox("Loading project file");
                try
                {
                    cmd.Execute(msg.UpdateProgress);

                    if (cmd.Completed)
                    {
                        SetModel(newModel);
                        _matrixControl.Size = new Size(panel1.ClientSize.Width, panel1.ClientSize.Height);
                    }
                }
                catch (DsmException dsmEx)
                {
                    MessageBox.Show("Unable to load DSM fom file for the following reason: " +
                        Environment.NewLine + Environment.NewLine + dsmEx,
                        "Dependency Structure Matrix",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                catch (Exception ex)
                {
                    ErrorDialog.Show(ex.ToString());
                }
                finally
                {
                    msg.Dispose();
                    csh.Reset();
                }

                if ((newModel != null) && (newModel.ModelFilename != null))
                {
                    this.Text = "DSM Viewer - " + newModel.ModelFilename;
                }
            }
        }

        private void DoProjectSave()
        {
            ICommand cmd = new CommandSave(_model);

            CursorStateHelper csh = new CursorStateHelper(this, Cursors.WaitCursor);

            Refresh();

            try
            {
                cmd.Execute(null);
            }
            catch (Exception ex)
            {
                ErrorDialog.Show(ex.ToString());
            }
            finally
            {
                csh.Reset();
            }
        }

        private void DoPartitioning()
        {
            ICommand cmd = new CommandPartition(_model);
            CursorStateHelper csh = new CursorStateHelper(this, Cursors.WaitCursor);

            Refresh();

            try
            {
                cmd.Execute(null);

                _matrixControl.Invalidate();
            }
            catch (Exception ex)
            {
                ErrorDialog.Show(ex.ToString());
            }
            finally
            {
                csh.Reset();
            }
        }

        private void SetModel(IDsmModel model)
        {
            _model = model;
            _matrixControl.MatrixModel = _model;
            _matrixControl.Enabled = true;
            toolStrip.Enabled = true;
        }

        private bool ConfirmModelSaved()
        {
            bool cont = true;

            if (_model != null && _model.IsModified)
            {
                string text = "The current model has some unsaved changes." +
                              Environment.NewLine + Environment.NewLine +
                              "Do you wish to save the model before continuing ?";

                DialogResult dr = MessageBox.Show(text, "Unsaved changes",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (dr.Equals(DialogResult.Yes))
                {
                    ICommand cmd = new CommandSave(_model);
                    cmd.Execute(null);
                }
                else if (dr.Equals(DialogResult.Cancel))
                {
                    cont = false;
                }
                // else No - cont  true;
            }

            return cont;
        }
    }
}
