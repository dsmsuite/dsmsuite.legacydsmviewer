using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DsmSuite.DsmViewer.View.ViewModel;
using DsmSuite.DsmViewer.View.Dialogs;
using DsmSuite.DsmViewer.View.Properties;
using DsmSuite.DsmViewer.View.Utils;
using DsmSuite.DsmViewer.Model.Data;
using DsmSuite.DsmViewer.Model.Model;
using DsmSuite.DsmViewer.Model.Collections;
using DsmSuite.DsmViewer.View.Reports;
using DsmSuite.DsmViewer.View.View;

namespace DsmSuite.DsmViewer.View.UserControls
{
    /// <summary>
    /// The matrix control which is the immediate parent of the TypeUserControl and MatrixPanels coordinating
    /// them in response to scrolling and resizing operations etc.
    /// </summary>
    public class MatrixControl : UserControl
    {
        private IContainer components;

        private SplitContainer _splitContainer;
        private MatrixPanel _matrix;
        private TypeUserPanel _selector;
        private HScrollBar _hScrollBar;
        private VScrollBar _vScrollBar;
        private ContextMenuStrip _cntxtMenuStrip;
        private readonly DisplayOptions _displayOptions;

        public ToolStripMenuItem CntxtItemMoveUp;
        public ToolStripMenuItem CntxtItemMoveDown;
        public ToolStripMenuItem CntxtItemShowConsumers;
        public ToolStripMenuItem CntxtItemShowProvidedInterfaces;
        public ToolStripMenuItem CntxtItemShowRequiredInterfaces;

        private readonly Brush _brush1 = new SolidBrush(Color.FromArgb(217, 231, 246)); //Brushes.LightCyan;
        private readonly Brush _brush2 = new SolidBrush(Color.FromArgb(246, 231, 217)); //Brushes.BlanchedAlmond;
        private readonly Brush _brush3 = new SolidBrush(Color.FromArgb(244, 217, 246)); //Brushes.Lavender;
        private readonly Brush _brush4 = new SolidBrush(Color.FromArgb(217, 246, 231));
        private ContextMenuStrip _cntxtMenuStripMatrixPanel;
        private ToolStripMenuItem _showRelationsToolStripMenuItem; //Brushes.MistyRose;
        private ToolStripMenuItem _showProvidersToolStripMenuItem;
        private ToolStripMenuItem _showConsumersToolStripMenuItem;

        public IDsmModel MatrixModel;

        internal TreeNode<Element> ProviderTreeNode { get; set; }
        internal TreeNode<Element> ConsumerTreeNode { get; set; }

        internal void SetCurrentModules(TreeNode<Element> provider, TreeNode<Element> consumer)
        {
            ProviderTreeNode = provider;
            ConsumerTreeNode = consumer;

            _selector.Invalidate();
            _matrix.Invalidate();
        }

        public int OffsetY;
        private ToolStripMenuItem _paritionToolStripMenuItem;
        public int OffsetX;

        public MatrixControl()
        {
            // Cet appel est requis par le Concepteur de formulaires Windows.Forms.
            InitializeComponent();

            Font sysFont = SystemFonts.MessageBoxFont;
            Font = new Font(sysFont.Name, sysFont.SizeInPoints, sysFont.Style);

            _displayOptions = new DisplayOptions(this);

            _selector.Controller = this;
            _matrix.Controller = this;

            _matrix.SizeChanged += MatrixSizeChanged;

            _hScrollBar.Value = 0;
            _hScrollBar.Minimum = 0;
            _hScrollBar.SmallChange = _displayOptions.CellHeight;

            _vScrollBar.Value = 0;
            _vScrollBar.Minimum = 0;
            _vScrollBar.SmallChange = _displayOptions.CellHeight;
        }

        internal DisplayOptions DisplayOptions => _displayOptions;

        internal void NodeListModified(bool resize)
        {
            if (resize)
            {
                CalculatePanelSizes();
            }

            ResizeControl();

            _selector.ViewRectangle = _splitContainer.Panel1.ClientRectangle;
            _selector.Invalidate();
            _matrix.ViewRectangle = _splitContainer.Panel2.ClientRectangle;
            _matrix.Invalidate();

            Update();
        }

        internal void CalculatePanelSizes()
        {
            int count = 0;

            if (MatrixModel?.Hierarchy != null)
            {
                TreeIterator<Element> iterator = new TreeIterator<Element>(MatrixModel.Hierarchy);
                TreeNode<Element> treeNode = iterator.Next();

                while (treeNode != null)
                {
                    if (treeNode.IsCollapsed)
                    {
                        count++;
                        treeNode = iterator.Skip();
                    }
                    else
                    {
                        treeNode = iterator.Next();
                    }
                }
            }

            _selector.Size = new Size(_selector.Width,
                count * _displayOptions.CellHeight + _displayOptions.RootHeight);

            _matrix.Size = new Size(count * _displayOptions.CellHeight,
                count * _displayOptions.CellHeight + _displayOptions.RootHeight);
        }

        internal int CountNbDisplayableNested(TreeNode<Element> treeNode)
        {
            int nb = 0;

            foreach (TreeNode<Element> child in treeNode.Children)
            {
                if (child.IsHidden == false)
                {
                    if (child.IsCollapsed)
                    {
                        nb++;
                    }
                    else
                    {
                        nb += CountNbDisplayableNested(child);
                    }
                }
            }

            return nb;
        }

        internal Brush GetBackgroundColour(TreeNode<Element> rowTreeNode, TreeNode<Element> colTreeNode)
        {
            int depth = 1;
            if (colTreeNode == null)
                depth = rowTreeNode.Depth;
            else
            {
                if (rowTreeNode == colTreeNode)
                    depth = rowTreeNode.Depth;
                else if (rowTreeNode?.Parent?.NodeValue != null && rowTreeNode.Parent == colTreeNode.Parent)
                    depth = rowTreeNode.Parent.Depth;
                else if (rowTreeNode?.Parent?.NodeValue != null && colTreeNode.Parent?.NodeValue != null)
                    depth = Math.Min(rowTreeNode.Parent.Depth, colTreeNode.Parent.Depth);
            }

            switch (Math.Abs(depth) % 4)
            {
                case 0: return _brush1;
                case 1: return _brush2;
                case 2: return _brush3;
                case 3: return _brush4;
                default:
                    throw new ApplicationException("Logic error in GetBackgroundColour");
            }
        }

        internal void SelectNode(TreeNode<Element> treeNode)
        {
            MatrixModel.SelectedTreeNode = treeNode;
            MatrixModel.SelectedProviderTreeNode = null;
            MatrixModel.SelectedConsumerTreeNode = null;
            NodeListModified(false);
            EnableButtons();
        }

        internal void SelectProviderNode(TreeNode<Element> treeNode)
        {
            MatrixModel.SelectedProviderTreeNode = treeNode;
            NodeListModified(false);
            EnableButtons();
        }

        internal void SelectConsumerNode(TreeNode<Element> treeNode)
        {
            MatrixModel.SelectedConsumerTreeNode = treeNode;
            NodeListModified(false);
            EnableButtons();
        }

        internal void ExpandSelectedNode()
        {
            TreeNode<Element> treeNode = MatrixModel.SelectedTreeNode;

            if (treeNode != null && treeNode.HasChildren)
            {
                treeNode.IsCollapsed = !treeNode.IsCollapsed;
                NodeListModified(true);
            }

        }

        internal void ExpandCurrentNode()
        {
            if (ProviderTreeNode == null && ConsumerTreeNode != null)
            {
                // TODO need to know if current nodes are expandable !!

                if (ConsumerTreeNode.HasChildren)
                {
                    ConsumerTreeNode.IsCollapsed = !ConsumerTreeNode.IsCollapsed;
                    NodeListModified(true);
                }
            }
        }

        internal void MoveSelectedNodeUp()
        {
            if (MatrixModel.MoveUp())
            {
                NodeListModified(false);
                EnableButtons();
            }
        }

        internal void MoveSelectedNodeDown()
        {
            if (MatrixModel.MoveDown())
            {
                NodeListModified(false);
                EnableButtons();
            }
        }

        internal void ShowContextMenu(Point position)
        {
            if (MatrixModel.SelectedConsumerTreeNode != null && MatrixModel.SelectedProviderTreeNode != null)
            {
                _cntxtMenuStripMatrixPanel.Show(position);
            }
            else
            {
                _cntxtMenuStrip.Show(position);
            }
        }

        internal void HideContextMenu()
        {
            _cntxtMenuStrip.Hide();
        }

        internal bool ContextMenuIsVisible => _cntxtMenuStrip.Visible;

        internal void HandleKeyEvent(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    if (e.Control)
                    {
                        ScrollTo(-1, _vScrollBar.Value + _vScrollBar.LargeChange, false);
                    }
                    else
                    {
                        ScrollTo(-1, _vScrollBar.Value + _vScrollBar.SmallChange, false);
                    }
                    break;
                case Keys.Up:
                    ScrollTo(-1,
                        e.Control
                            ? Math.Max(0, _vScrollBar.Value - _vScrollBar.LargeChange)
                            : Math.Max(0, _vScrollBar.Value - _vScrollBar.SmallChange), false);
                    break;
                case Keys.Left:
                    ScrollTo(
                        e.Control
                            ? Math.Max(0, _hScrollBar.Value - _hScrollBar.LargeChange)
                            : Math.Max(0, _hScrollBar.Value - _hScrollBar.SmallChange), -1, false);
                    break;
                case Keys.Right:
                    if (e.Control)
                    {
                        ScrollTo(_hScrollBar.Value + _hScrollBar.LargeChange, -1, false);
                    }
                    else
                    {
                        ScrollTo(_hScrollBar.Value + _hScrollBar.SmallChange, -1, false);
                    }
                    break;
                case Keys.PageDown:
                    ScrollTo(-1, _vScrollBar.Value + _vScrollBar.LargeChange, false);
                    break;
                case Keys.PageUp:
                    ScrollTo(-1, Math.Max(0, _vScrollBar.Value - _vScrollBar.LargeChange), false);
                    break;
                case Keys.Home:
                    ScrollTo(0, 0, false);
                    break;
                case Keys.End:
                    ScrollTo(_hScrollBar.Maximum - _hScrollBar.LargeChange, _vScrollBar.Maximum - _vScrollBar.LargeChange, false);
                    break;
            }
        }

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants
        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MatrixControl));
            _splitContainer = new SplitContainer();
            _selector = new TypeUserPanel();
            _matrix = new MatrixPanel();
            _hScrollBar = new HScrollBar();
            _vScrollBar = new VScrollBar();
            _cntxtMenuStrip = new ContextMenuStrip(components);
            CntxtItemMoveUp = new ToolStripMenuItem();
            CntxtItemMoveDown = new ToolStripMenuItem();
            CntxtItemShowConsumers = new ToolStripMenuItem();
            CntxtItemShowProvidedInterfaces = new ToolStripMenuItem();
            CntxtItemShowRequiredInterfaces = new ToolStripMenuItem();
            _paritionToolStripMenuItem = new ToolStripMenuItem();
            _cntxtMenuStripMatrixPanel = new ContextMenuStrip(components);
            _showRelationsToolStripMenuItem = new ToolStripMenuItem();
            _showProvidersToolStripMenuItem = new ToolStripMenuItem();
            _showConsumersToolStripMenuItem = new ToolStripMenuItem();
            _splitContainer.Panel1.SuspendLayout();
            _splitContainer.Panel2.SuspendLayout();
            _splitContainer.SuspendLayout();
            _cntxtMenuStrip.SuspendLayout();
            _cntxtMenuStripMatrixPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _splitContainer
            // 
            _splitContainer.BackColor = SystemColors.ControlLight;
            _splitContainer.FixedPanel = FixedPanel.Panel1;
            _splitContainer.Location = new Point(0, 0);
            _splitContainer.Name = "_splitContainer";
            // 
            // _splitContainer.Panel1
            // 
            _splitContainer.Panel1.BackColor = SystemColors.Control;
            _splitContainer.Panel1.Controls.Add(_selector);
            // 
            // _splitContainer.Panel2
            // 
            _splitContainer.Panel2.BackColor = SystemColors.Control;
            _splitContainer.Panel2.Controls.Add(_matrix);
            _splitContainer.Size = new Size(825, 289);
            _splitContainer.SplitterDistance = 265;
            _splitContainer.SplitterWidth = 3;
            _splitContainer.TabIndex = 2;
            _splitContainer.SplitterMoved += SplitContainer1SplitterMoved;
            // 
            // _selector
            // 
            _selector.BackColor = SystemColors.Control;
            _selector.Font = new Font("Segoe UI", 9F);
            _selector.ForeColor = SystemColors.ControlText;
            _selector.Location = new Point(0, 3);
            _selector.Name = "_selector";
            _selector.Size = new Size(265, 216);
            _selector.TabIndex = 0;
            // 
            // _matrix
            // 
            _matrix.AllowDrop = true;
            _matrix.BackColor = SystemColors.Control;
            _matrix.Font = new Font("Segoe UI", 9F);
            _matrix.Location = new Point(1, 3);
            _matrix.Name = "_matrix";
            _matrix.Size = new Size(501, 216);
            _matrix.TabIndex = 0;
            // 
            // _hScrollBar
            // 
            _hScrollBar.Location = new Point(270, 292);
            _hScrollBar.Name = "_hScrollBar";
            _hScrollBar.Size = new Size(555, 17);
            _hScrollBar.TabIndex = 1;
            _hScrollBar.Scroll += HScrollBar1Scroll;
            // 
            // _vScrollBar
            // 
            _vScrollBar.Location = new Point(828, 0);
            _vScrollBar.Maximum = 160;
            _vScrollBar.Name = "_vScrollBar";
            _vScrollBar.Size = new Size(17, 289);
            _vScrollBar.SmallChange = 10;
            _vScrollBar.TabIndex = 3;
            _vScrollBar.Value = 16;
            _vScrollBar.Scroll += VScrollBar1Scroll;
            // 
            // _cntxtMenuStrip
            // 
            _cntxtMenuStrip.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _cntxtMenuStrip.Items.AddRange(new ToolStripItem[] {
            CntxtItemMoveUp,
            CntxtItemMoveDown,
            _paritionToolStripMenuItem,
            CntxtItemShowConsumers,
            CntxtItemShowProvidedInterfaces,
            CntxtItemShowRequiredInterfaces
            });
            _cntxtMenuStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            _cntxtMenuStrip.RenderMode = ToolStripRenderMode.Professional;
            _cntxtMenuStrip.Size = new Size(153, 92);
            // 
            // cntxtItemMoveUp
            // 
            CntxtItemMoveUp.Enabled = false;
            CntxtItemMoveUp.Image = Resources.UpArrow1;
            CntxtItemMoveUp.Name = "CntxtItemMoveUp";
            CntxtItemMoveUp.Size = new Size(152, 22);
            CntxtItemMoveUp.Text = "Move Up";
            CntxtItemMoveUp.Click += CntxtItemMoveUpClick;
            // 
            // cntxtItemMoveDown
            // 
            CntxtItemMoveDown.Enabled = false;
            CntxtItemMoveDown.Image = Resources.DownArrow;
            CntxtItemMoveDown.Name = "CntxtItemMoveDown";
            CntxtItemMoveDown.Size = new Size(152, 22);
            CntxtItemMoveDown.Text = "Move Down";
            CntxtItemMoveDown.Click += CntxtItemMoveDownClick;

            // 
            // CntxtItemShowConsumerElements
            // 
            CntxtItemShowConsumers.Enabled = true;
            CntxtItemShowConsumers.Name = "CntxtItemShowConsumers";
            CntxtItemShowConsumers.Size = new Size(152, 22);
            CntxtItemShowConsumers.Text = "Show Element Consumers";
            CntxtItemShowConsumers.Click += ShowElementConsumersReportToolStripMenuItemClick;

            // 
            // CntxtItemShowProviderElements
            // 
            CntxtItemShowProvidedInterfaces.Enabled = true;
            CntxtItemShowProvidedInterfaces.Name = "CntxtItemShowProvidedInterfaces";
            CntxtItemShowProvidedInterfaces.Size = new Size(152, 22);
            CntxtItemShowProvidedInterfaces.Text = "Show Element Provided Interface";
            CntxtItemShowProvidedInterfaces.Click += ShowElementProvidedInterfacesReportToolStripMenuItemClick;

            // 
            // CntxtItemShowRequiredElements
            // 
            CntxtItemShowRequiredInterfaces.Enabled = true;
            CntxtItemShowRequiredInterfaces.Name = "CntxtItemShowRequiredInterfaces";
            CntxtItemShowRequiredInterfaces.Size = new Size(152, 22);
            CntxtItemShowRequiredInterfaces.Text = "Show Element Required Interface";
            CntxtItemShowRequiredInterfaces.Click += ShowInterfaceRequiredInterfacesReportToolStripMenuItemClick;

            // 
            // paritionToolStripMenuItem
            // 
            _paritionToolStripMenuItem.Enabled = false;
            _paritionToolStripMenuItem.Image = Resources.Partition;
            _paritionToolStripMenuItem.Name = "_paritionToolStripMenuItem";
            _paritionToolStripMenuItem.Size = new Size(152, 22);
            _paritionToolStripMenuItem.Text = "Partition";
            _paritionToolStripMenuItem.Click += ParitionToolStripMenuItemClick;
            // 
            // _cntxtMenuStripMatrixPanel
            // 
            _cntxtMenuStripMatrixPanel.Items.AddRange(new ToolStripItem[] {
            _showRelationsToolStripMenuItem, _showConsumersToolStripMenuItem, _showProvidersToolStripMenuItem});
            _cntxtMenuStripMatrixPanel.Name = "_cntxtMenuStripMatrixPanel";
            _cntxtMenuStripMatrixPanel.Size = new Size(155, 26);
            // 
            // showRelationsToolStripMenuItem
            // 
            _showRelationsToolStripMenuItem.Name = "_showRelationsToolStripMenuItem";
            _showRelationsToolStripMenuItem.Size = new Size(154, 22);
            _showRelationsToolStripMenuItem.Text = "Show Relations";
            _showRelationsToolStripMenuItem.Click += ShowRelationsToolStripMenuItemClick;
            // 
            // showProvidersToolStripMenuItem
            // 
            _showProvidersToolStripMenuItem.Name = "_showProvidersToolStripMenuItem";
            _showProvidersToolStripMenuItem.Size = new Size(154, 22);
            _showProvidersToolStripMenuItem.Text = "Show Relation Providers";
            _showProvidersToolStripMenuItem.Click += ShowProvidersToolStripMenuItemClick;
            // 
            // showConsumersToolStripMenuItem
            // 
            _showConsumersToolStripMenuItem.Name = "_showConsumersToolStripMenuItem";
            _showConsumersToolStripMenuItem.Size = new Size(154, 22);
            _showConsumersToolStripMenuItem.Text = "Show Relation Consumers";
            _showConsumersToolStripMenuItem.Click += ShowConsumersToolStripMenuItemClick;
            // 
            // MatrixControl
            // 
            Controls.Add(_vScrollBar);
            Controls.Add(_splitContainer);
            Controls.Add(_hScrollBar);
            DoubleBuffered = true;
            Enabled = false;
            Name = "MatrixControl";
            Size = new Size(848, 315);
            Resize += MatrixControlResize;
            EnabledChanged += MatrixControlEnabledChanged;
            _splitContainer.Panel1.ResumeLayout(false);
            _splitContainer.Panel2.ResumeLayout(false);
            _splitContainer.ResumeLayout(false);
            _cntxtMenuStrip.ResumeLayout(false);
            _cntxtMenuStripMatrixPanel.ResumeLayout(false);
            ResumeLayout(false);

        }
        #endregion

        void MatrixSizeChanged(object sender, EventArgs e)
        {
            ResizeControl();
        }

        void SplitContainer1SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (Enabled)
            {
                _selector.Size = new Size(e.SplitX, Size.Height);

                ResizeControl();

                _selector.ViewRectangle = _splitContainer.Panel1.ClientRectangle;
                _selector.Invalidate();
                _matrix.ViewRectangle = _splitContainer.Panel2.ClientRectangle;
                _matrix.Invalidate();

                Update();
            }

        }

        void MatrixControlResize(object sender, EventArgs e)
        {
            ResizeControl();

            _selector.ViewRectangle = _splitContainer.Panel1.ClientRectangle;
            _selector.Invalidate();
            _matrix.ViewRectangle = _splitContainer.Panel2.ClientRectangle;
            _matrix.Invalidate();
            Invalidate();
            Update();
        }

        void ResizeControl()
        {
            _splitContainer.Size = new Size(
                Width - _vScrollBar.Width - 1,
                Height - _hScrollBar.Height - 1);

            int w = _splitContainer.Panel2.Width;
            int h = _splitContainer.Panel2.Height;

            if (w < 0 || w > _matrix.Width)
            {
                _hScrollBar.Visible = false;

                OffsetX = 0;
            }
            else
            {
                _hScrollBar.Location = new Point(_splitContainer.Panel2.Left, _splitContainer.Height);
                _hScrollBar.Size = new Size(_splitContainer.Panel2.Width, _hScrollBar.Height);
                _hScrollBar.Maximum = _matrix.Width;

                _hScrollBar.LargeChange = Math.Max(_splitContainer.Panel2.Width, 0);
                _hScrollBar.SmallChange = _displayOptions.CellHeight;

                // adjust offset  so that last scroll does not go past end
                if (OffsetX + w > _matrix.Width)
                {
                    OffsetX -= (OffsetX + w - _matrix.Width);
                }

                if (OffsetX < 0) OffsetX = 0;

                _hScrollBar.Value = OffsetX;

                _hScrollBar.Visible = true;
            }

            if (h < 0 || h > _matrix.Height)
            {
                _vScrollBar.Visible = false;

                OffsetY = 0;
            }
            else
            {

                _vScrollBar.Location = new Point(_splitContainer.Width, _displayOptions.RootHeight + 1);
                _vScrollBar.Size = new Size(_vScrollBar.Width, _splitContainer.Height - _displayOptions.RootHeight);
                _vScrollBar.Maximum = _matrix.Height - _displayOptions.RootHeight;
                _vScrollBar.LargeChange = Math.Max(0, _splitContainer.Panel2.Height - _displayOptions.RootHeight);
                _vScrollBar.SmallChange = _displayOptions.CellHeight;

                if (OffsetY + h > _matrix.Height)
                {
                    OffsetY -= (OffsetY + h - _matrix.Height);
                }

                if (OffsetY < 0) OffsetY = 0;

                _vScrollBar.Value = OffsetY;
                _vScrollBar.Visible = true;
            }

            _vScrollBar.Invalidate();
            _hScrollBar.Invalidate();
        }

        void VScrollBar1Scroll(object sender, ScrollEventArgs e)
        {
            ScrollTo(-1, e.NewValue, true);
        }

        void HScrollBar1Scroll(object sender, ScrollEventArgs e)
        {
            ScrollTo(e.NewValue, -1, true);
        }

        void ScrollTo(int x, int y, bool wasScrollBar)
        {
            bool scroll = false;

            if (x >= 0)
            {
                OffsetX = x;
                int w = _splitContainer.Panel2.Width;

                if (OffsetX + w > _matrix.Width)
                {
                    OffsetX -= (OffsetX + w - _matrix.Width);
                }

                if (OffsetX < 0) OffsetX = 0;

                _matrix.ViewRectangle = _splitContainer.Panel2.ClientRectangle;
                _matrix.Invalidate();

                scroll = true;

                if (!wasScrollBar)
                {
                    _hScrollBar.Value = OffsetX;
                }
            }

            if (y >= 0)
            {
                OffsetY = y;

                int h = _splitContainer.Panel2.Height;

                if (OffsetY + h > _matrix.Height)
                {
                    OffsetY -= (OffsetY + h - _matrix.Height);
                }

                if (OffsetY < 0) OffsetY = 0;

                _matrix.ViewRectangle = _splitContainer.Panel2.ClientRectangle;
                _matrix.Invalidate();

                _selector.ViewRectangle = _splitContainer.Panel1.ClientRectangle;
                _selector.Invalidate();

                scroll = true;

                if (!wasScrollBar)
                {
                    _vScrollBar.Value = OffsetY;
                }
            }

            if (scroll)
                Update();
        }

        void EnableButtons()
        {
            bool down = MatrixModel.CanMoveNodeDown();
            bool up = MatrixModel.CanMoveNodeUp();

            CntxtItemMoveDown.Enabled = down;
            CntxtItemMoveUp.Enabled = up;

            MainView main = Parent.Parent as MainView;
            if (main != null)
            {
                main.EnableNodeDownButton = down;
                main.EnableNodeUpButton = up;

                bool canPartition = (MatrixModel.SelectedTreeNode != null &&
                                     MatrixModel.SelectedTreeNode.Children.Count > 1);

                _paritionToolStripMenuItem.Enabled = canPartition;
                main.EnablePartitionButton = canPartition;
            }
        }

        void CntxtItemMoveUpClick(object sender, EventArgs e)
        {
            MoveSelectedNodeUp();
        }

        void CntxtItemMoveDownClick(object sender, EventArgs e)
        {
            MoveSelectedNodeDown();
        }

        private void MatrixControlEnabledChanged(object sender, EventArgs e)
        {
            if (Enabled && MatrixModel == null)
            {
                Enabled = false;
            }
            else if (Enabled)
            {
                _cntxtMenuStrip.Enabled = true;
            }
        }

        private void ParitionToolStripMenuItemClick(object sender, EventArgs e)
        {
            ICommand cmd = new CommandPartition(MatrixModel);
            CursorStateHelper csh = new CursorStateHelper(this, Cursors.WaitCursor);
            try
            {
                cmd.Execute(null);

                Invalidate();

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

        private void ShowRelationsToolStripMenuItemClick(object sender, EventArgs e)
        {
            CursorStateHelper csh = new CursorStateHelper(this, Cursors.WaitCursor);
            try
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                RelationsReport report = new RelationsReport(MatrixModel);
                report.WriteReport(sw);

                HtmlViewer viewer = new HtmlViewer(ms);
                viewer.Show();

                Invalidate();
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

        private void ShowElementProvidedInterfacesReportToolStripMenuItemClick(object sender, EventArgs e)
        {
            CursorStateHelper csh = new CursorStateHelper(this, Cursors.WaitCursor);
            try
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                ElementProvidedInterfacesReport report = new ElementProvidedInterfacesReport(MatrixModel);
                report.WriteReport(sw);

                HtmlViewer viewer = new HtmlViewer(ms);
                viewer.Show();

                Invalidate();
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

        private void ShowElementConsumersReportToolStripMenuItemClick(object sender, EventArgs e)
        {
            CursorStateHelper csh = new CursorStateHelper(this, Cursors.WaitCursor);
            try
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                ElementConsumersReport report = new ElementConsumersReport(MatrixModel);
                report.WriteReport(sw);

                HtmlViewer viewer = new HtmlViewer(ms);
                viewer.Show();

                Invalidate();
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

        public Relation GetRelation(Element consumer, Element provider)
        {
            return MatrixModel.GetRelation(consumer, provider);
        }

        private void ShowInterfaceRequiredInterfacesReportToolStripMenuItemClick(object sender, EventArgs e)
        {
            CursorStateHelper csh = new CursorStateHelper(this, Cursors.WaitCursor);
            try
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                ElementRequiredInterfacesReport report = new ElementRequiredInterfacesReport(MatrixModel);
                report.WriteReport(sw);

                HtmlViewer viewer = new HtmlViewer(ms);
                viewer.Show();

                Invalidate();
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

        private void ShowProvidersToolStripMenuItemClick(object sender, EventArgs e)
        {
            CursorStateHelper csh = new CursorStateHelper(this, Cursors.WaitCursor);
            try
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                CellProvidersReport report = new CellProvidersReport(MatrixModel);
                report.WriteReport(sw);

                HtmlViewer viewer = new HtmlViewer(ms);
                viewer.Show();

                Invalidate();
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

        private void ShowConsumersToolStripMenuItemClick(object sender, EventArgs e)
        {
            CursorStateHelper csh = new CursorStateHelper(this, Cursors.WaitCursor);
            try
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                CellConsumersReport report = new CellConsumersReport(MatrixModel);
                report.WriteReport(sw);

                HtmlViewer viewer = new HtmlViewer(ms);
                viewer.Show();

                Invalidate();
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
    }
}
