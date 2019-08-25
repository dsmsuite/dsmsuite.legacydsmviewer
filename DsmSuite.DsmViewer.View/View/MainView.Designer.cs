using System.ComponentModel;
using System.Windows.Forms;

namespace DsmSuite.DsmViewer.View.View
{
    partial class MainView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainView));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.openButton = new System.Windows.Forms.ToolStripButton();
            this.saveButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.upButton = new System.Windows.Forms.ToolStripButton();
            this.downButton = new System.Windows.Forms.ToolStripButton();
            this.partitionButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.showCyclesButton = new System.Windows.Forms.ToolStripButton();
            this.zoomButton = new System.Windows.Forms.ToolStripSplitButton();
            this.itmZoom1 = new System.Windows.Forms.ToolStripMenuItem();
            this.itmZoom2 = new System.Windows.Forms.ToolStripMenuItem();
            this.itmZoom3 = new System.Windows.Forms.ToolStripMenuItem();
            this.itmZoom4 = new System.Windows.Forms.ToolStripMenuItem();
            this.itmZoom5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.reportButton = new System.Windows.Forms.ToolStripButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openButton,
            this.saveButton,
            this.toolStripSeparator1,
            this.upButton,
            this.downButton,
            this.partitionButton,
            this.toolStripSeparator2,
            this.showCyclesButton,
            this.zoomButton,
            this.toolStripSeparator3,
            this.reportButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(284, 25);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip";
            // 
            // openButton
            // 
            this.openButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openButton.Image = global::DsmSuite.DsmViewer.View.Properties.Resources.Open1;
            this.openButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(23, 22);
            this.openButton.Text = "Open";
            this.openButton.ToolTipText = "Open existing current project";
            this.openButton.Click += new System.EventHandler(this.OpenButtonClick);
            // 
            // saveButton
            // 
            this.saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveButton.Image = global::DsmSuite.DsmViewer.View.Properties.Resources.Save1;
            this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(23, 22);
            this.saveButton.Text = "Save";
            this.saveButton.ToolTipText = "Save current project";
            this.saveButton.Click += new System.EventHandler(this.SaveButtonClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // upButton
            // 
            this.upButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.upButton.Image = global::DsmSuite.DsmViewer.View.Properties.Resources.UpArrow1;
            this.upButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.upButton.Name = "upButton";
            this.upButton.Size = new System.Drawing.Size(23, 22);
            this.upButton.Text = "Move Up";
            this.upButton.Click += new System.EventHandler(this.UpButtonClick);
            // 
            // downButton
            // 
            this.downButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.downButton.Image = global::DsmSuite.DsmViewer.View.Properties.Resources.DownArrow;
            this.downButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.downButton.Name = "downButton";
            this.downButton.Size = new System.Drawing.Size(23, 22);
            this.downButton.Text = "Move down";
            this.downButton.Click += new System.EventHandler(this.DownButtonClick);
            // 
            // partitionButton
            // 
            this.partitionButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.partitionButton.Image = global::DsmSuite.DsmViewer.View.Properties.Resources.Partition;
            this.partitionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.partitionButton.Name = "partitionButton";
            this.partitionButton.Size = new System.Drawing.Size(23, 22);
            this.partitionButton.Text = "Partition";
            this.partitionButton.ToolTipText = "Partition modules of current selection";
            this.partitionButton.Click += new System.EventHandler(this.PartitionButtonClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // showCyclesButton
            // 
            this.showCyclesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.showCyclesButton.Image = global::DsmSuite.DsmViewer.View.Properties.Resources.HighlightCyclic_Yellow;
            this.showCyclesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showCyclesButton.Name = "showCyclesButton";
            this.showCyclesButton.Size = new System.Drawing.Size(23, 22);
            this.showCyclesButton.Text = "showCyclesButton";
            this.showCyclesButton.ToolTipText = "Show/Hide Cyclic Dependencies";
            this.showCyclesButton.Click += new System.EventHandler(this.ShowCyclesButtonClick);
            // 
            // zoomButton
            // 
            this.zoomButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itmZoom1,
            this.itmZoom2,
            this.itmZoom3,
            this.itmZoom4,
            this.itmZoom5});
            this.zoomButton.Image = global::DsmSuite.DsmViewer.View.Properties.Resources.Zoom1;
            this.zoomButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomButton.Name = "zoomButton";
            this.zoomButton.Size = new System.Drawing.Size(32, 22);
            this.zoomButton.Text = "toolStripSplitButton1";
            this.zoomButton.ToolTipText = "Zoom";
            this.zoomButton.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.BtnZoomDropDownItemClicked);
            // 
            // itmZoom1
            // 
            this.itmZoom1.CheckOnClick = true;
            this.itmZoom1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.itmZoom1.Image = global::DsmSuite.DsmViewer.View.Properties.Resources.DSM;
            this.itmZoom1.Name = "itmZoom1";
            this.itmZoom1.Size = new System.Drawing.Size(152, 22);
            this.itmZoom1.Text = "Smallest";
            // 
            // itmZoom2
            // 
            this.itmZoom2.CheckOnClick = true;
            this.itmZoom2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.itmZoom2.Name = "itmZoom2";
            this.itmZoom2.Size = new System.Drawing.Size(152, 22);
            this.itmZoom2.Text = "Smaller";
            // 
            // itmZoom3
            // 
            this.itmZoom3.Checked = true;
            this.itmZoom3.CheckOnClick = true;
            this.itmZoom3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.itmZoom3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.itmZoom3.Name = "itmZoom3";
            this.itmZoom3.Size = new System.Drawing.Size(152, 22);
            this.itmZoom3.Text = "Medium";
            // 
            // itmZoom4
            // 
            this.itmZoom4.CheckOnClick = true;
            this.itmZoom4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.itmZoom4.Name = "itmZoom4";
            this.itmZoom4.Size = new System.Drawing.Size(152, 22);
            this.itmZoom4.Text = "Larger";
            // 
            // itmZoom5
            // 
            this.itmZoom5.CheckOnClick = true;
            this.itmZoom5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.itmZoom5.Name = "itmZoom5";
            this.itmZoom5.Size = new System.Drawing.Size(152, 22);
            this.itmZoom5.Text = "Largest";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // reportButton
            // 
            this.reportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.reportButton.Image = global::DsmSuite.DsmViewer.View.Properties.Resources.Reports1;
            this.reportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.reportButton.Name = "reportButton";
            this.reportButton.Size = new System.Drawing.Size(23, 22);
            this.reportButton.Text = "Generate Report";
            this.reportButton.Click += new System.EventHandler(this.ReportButtonClick);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 25);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(284, 236);
            this.panel1.TabIndex = 1;
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainView";
            this.Text = "DSM Viewer";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ToolStrip toolStrip;
        private ToolStripButton openButton;
        private ToolStripButton saveButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton upButton;
        private ToolStripButton downButton;
        private ToolStripButton partitionButton;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton showCyclesButton;

        private ToolStripSplitButton zoomButton;
        private ToolStripMenuItem itmZoom1;
        private ToolStripMenuItem itmZoom2;
        private ToolStripMenuItem itmZoom3;
        private ToolStripMenuItem itmZoom4;
        private ToolStripMenuItem itmZoom5;

        private ToolStripButton reportButton;
        private Panel panel1;
    }
}