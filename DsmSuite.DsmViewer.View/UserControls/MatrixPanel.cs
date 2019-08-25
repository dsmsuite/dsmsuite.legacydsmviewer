using System;
using System.Drawing;
using System.Windows.Forms;
using DsmSuite.DsmViewer.View.Properties;
using DsmSuite.DsmViewer.Model.Data;
using DsmSuite.DsmViewer.Model.Collections;
using DsmSuite.DsmViewer.Model.Model;

namespace DsmSuite.DsmViewer.View.UserControls
{
    /// <summary>
    /// This is the Matrix control showing the weights between the different types
    /// </summary>
    public class MatrixPanel : UserControl
    {
        //private System.ComponentModel.IContainer components;

        // TODO Correct public fields
        public bool HideClosedSets = false;
        public MatrixControl Controller;
        public Rectangle ViewRectangle;

        readonly Pen _borderPen;
        readonly Brush _fcBrush;
        readonly StringFormat _vStringFormat;

        readonly LayoutHelper _hLayout;  //list of horizontal panels
        readonly LayoutHelper _vLayout;  // list of vertical panels

        //NodePanel    _hPanel; // current horizontal panel
        //NodePanel    _vPanel; // current vertical panel

        readonly ToolTip _tooltip;
        readonly Timer _ttTimer;

        //Image _imgExpanded;
        readonly Image _imgCollapsed;

        /// <summary>
        /// Contstructor
        /// </summary>
		public MatrixPanel()
        {
            // Cet appel est requis par le Concepteur de formulaires Windows.Forms.
            InitializeComponent();

            Font sysFont = SystemFonts.MessageBoxFont;
            Font = new Font(sysFont.Name, sysFont.SizeInPoints, sysFont.Style);

            _borderPen = new Pen(Brushes.DarkGray, 1);
            //_fcPen         = new Pen(Brushes.Black, 1);
            _fcBrush = Brushes.Black;
            _vStringFormat = new StringFormat(StringFormatFlags.DirectionVertical);

            _tooltip = new ToolTip();
            _ttTimer = new Timer {Interval = 4000};
            _ttTimer.Tick += TtTimerTick;

            _hLayout = new LayoutHelper();
            _vLayout = new LayoutHelper();

            //_imgExpanded = DsmSuite.DsmViewer.Properties.Resources.Expanded;
            _imgCollapsed = Resources.Collapsed;

        }

        void TtTimerTick(object sender, EventArgs e)
        {
            if (_tooltip.Active)
            {
                _tooltip.Active = false;
                _ttTimer.Stop();
            }
        }

        #region Code généré par le Concepteur de composants
        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // MatrixUserControl
            // 
            BackColor = SystemColors.Window;
            Size = new Size(669, 366);
            DoubleClick += MatrixPanelDoubleClick;
            MouseLeave += MatrixPanelMouseLeave;
            MouseMove += MatrixPanelMouseMove;
            MouseClick += MatrixPanelMouseClick;
            ResumeLayout(false);

        }
        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Controller?.MatrixModel != null)
            {
                using (Graphics g = e.Graphics) // FIXME - is using necessary ?
                {
                    _hLayout.Clear();
                    _vLayout.Clear();

                    using (Bitmap image = new Bitmap(ViewRectangle.Width, ViewRectangle.Height))
                    {
                        using (Graphics g1 = Graphics.FromImage(image))
                        {
                            g1.Clip = new Region(ViewRectangle);

                            try
                            {
                                Draw(g1);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.StackTrace);
                            }
                            g.DrawImageUnscaled(image, 0, 0);
                        }
                    }
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Controller?.MatrixModel == null) // for designer
            {
                base.OnPaintBackground(e);
            }
        }

        void Draw(Graphics g)
        {
            int y = -Controller.OffsetY + Controller.DisplayOptions.RootHeight;

            TreeIterator<Element> iterator = new TreeIterator<Element>(Controller.MatrixModel.Hierarchy);

            TreeNode<Element> treeNode = iterator.Next();

            //
            // RootTree has priority - so it is painted last but its panel is saved first in the hList
            if (treeNode != null)
            {
                Rectangle rootBounds = new Rectangle(
                    -Controller.OffsetX, 0,
                    Size.Width, Controller.DisplayOptions.RootHeight);

                // note  for root hpanel treeNode of nodepanel is null
                _hLayout.Add(new NodePanel(null, rootBounds));
            }

            while (treeNode != null)
            {
                if (treeNode.IsCollapsed == false || treeNode.IsHidden)
                {
                    treeNode = iterator.Next();
                }
                else
                {
                    DrawPanel(g, treeNode, y);
                    y += Controller.DisplayOptions.CellHeight;

                    treeNode = iterator.Skip();
                }
            }

            Size = new Size(y + Controller.OffsetY - Controller.DisplayOptions.RootHeight + 1, y + Controller.OffsetY + 1);

            DrawGroupingSquares(g);

            DrawRootPanel(g);

        }

        void DrawGroupingSquares(Graphics g)
        {
            TreeIterator<Element> iterator = new TreeIterator<Element>(Controller.MatrixModel.Hierarchy);
            TreeNode<Element> treeNode = iterator.Next();

            int xPos = -Controller.OffsetX + 1;
            int yPos = -Controller.OffsetY + Controller.DisplayOptions.RootHeight + 1;
            while (treeNode != null)
            {
                if (treeNode.IsHidden == false)
                {
                    if (treeNode.IsCollapsed == false)
                    {
                        int width = Controller.CountNbDisplayableNested(treeNode) * Controller.DisplayOptions.CellHeight;
                        Rectangle rect = new Rectangle(xPos, yPos, width - 1, width - 1);

                        if (g.Clip.IsVisible(rect))
                        {
                            g.DrawRectangle(new Pen(Brushes.DarkGray, 2), rect);
                        }
                        treeNode = iterator.Next();
                    }
                    else
                    {
                        xPos += Controller.DisplayOptions.CellHeight;
                        yPos += Controller.DisplayOptions.CellHeight;
                        treeNode = iterator.Skip();
                    }
                }
                else
                {
                    treeNode = iterator.Next();
                }

            }
        }

        private void DrawRootPanel(Graphics g)
        {
            int stateDisplay = 0;  // tri-state optimisation 0 not started dispaying, 1 currently displaying
                                   // 2 finished displaying and can therefore break out of the loop

            int x = -Controller.OffsetX;

            TreeIterator<Element> iterator = new TreeIterator<Element>(Controller.MatrixModel.Hierarchy);
            TreeNode<Element> treeNode = iterator.Next();
            while (treeNode != null && stateDisplay != 2)
            {
                Element module = treeNode.NodeValue;

                if (treeNode.IsCollapsed == false || treeNode.IsHidden)
                {
                    treeNode = iterator.Next();
                }
                else
                {
                    Rectangle cell = new Rectangle(x, 0,
                        Controller.DisplayOptions.CellHeight, Controller.DisplayOptions.RootHeight - 2);

                    if (g.Clip.IsVisible(cell))
                    {
                        // for each visible cell we create a vertical panel in vLayout of
                        // height of _matrix
                        Rectangle vPanelRec =
                            new Rectangle(x, 0, Controller.DisplayOptions.CellHeight, Size.Height);
                        _vLayout.Add(new NodePanel(treeNode, vPanelRec));

                        g.FillRectangle(
                            Controller.ConsumerTreeNode == treeNode
                                ? Brushes.White
                                : Controller.GetBackgroundColour(treeNode, null), cell);

                        if (treeNode.HasChildren)
                        {
                            g.DrawImage(_imgCollapsed, cell.Left + (cell.Width / 2.0f) - 4, cell.Top + 2);
                        }

                        g.DrawString(module.Id.ToString(), Controller.DisplayOptions.TextFont,
                            _fcBrush, x, 10, _vStringFormat);

                        g.DrawRectangle(_borderPen, cell);
                        g.DrawLine(new Pen(Brushes.White, 1),
                            cell.Left, cell.Bottom + 1, cell.Right, cell.Bottom + 1);

                        if (stateDisplay == 0)
                            stateDisplay++;
                    }
                    else if (stateDisplay == 1)
                        stateDisplay++; // finished displaying

                    x += Controller.DisplayOptions.CellHeight;

                    treeNode = iterator.Skip();
                }
            }
        }

        void DrawPanel(Graphics g, TreeNode<Element> rowTreeNode, int y)
        {
            Rectangle rowBounds = new Rectangle(0, y, Size.Width, Controller.DisplayOptions.CellHeight);

            if (g.Clip.IsVisible(rowBounds))
            {
                _hLayout.Add(new NodePanel(rowTreeNode, rowBounds));

                // can draw some of the row
                int x = -Controller.OffsetX;

                TreeIterator<Element> iterator = new TreeIterator<Element>(Controller.MatrixModel.Hierarchy);

                TreeNode<Element> treeNode = iterator.Next();

                Element rowModule = rowTreeNode.NodeValue;

                int stateDisplay = 0;
                // trisstate optimisation 0 not started dispaying, 1 currently displaying
                // 2 finished displaying and can therefore break out of the loop

                while (treeNode != null && stateDisplay != 2)
                {
                    Element module = treeNode.NodeValue;

                    if (treeNode.IsCollapsed == false || treeNode.IsHidden)
                    {
                        treeNode = iterator.Next();
                    }
                    else
                    {
                        Rectangle cell = new Rectangle(
                            x, y, Controller.DisplayOptions.CellHeight, Controller.DisplayOptions.CellHeight);

                        if (g.Clip.IsVisible(cell))
                        {
                            if (treeNode == rowTreeNode) // the diagonal
                            {
                                g.FillRectangle(Controller.GetBackgroundColour(rowTreeNode, treeNode), cell); // TODO - OK
                            }
                            else
                            {
                                if (Controller.DisplayOptions.ShowCyclicRelations &&
                                    Controller.MatrixModel.HasCyclicRelation(module, rowModule))
                                {
                                    g.FillRectangle(Brushes.Yellow, cell);
                                }
                                //else if ( (_hPanel != null && _hPanel.TreeNode == rowTreeNode ) ||
                                //    (_vPanel != null && _vPanel.TreeNode == treeNode ) )
                                else if (Controller.ProviderTreeNode == rowTreeNode ||
                                    Controller.ConsumerTreeNode == treeNode)
                                {
                                    g.FillRectangle(Brushes.White, cell);
                                }
                                else
                                {
                                    g.FillRectangle(Controller.GetBackgroundColour(rowTreeNode, treeNode), cell);
                                    //if (rowTreeNode.Parent == treeNode.Parent )
                                    //{
                                    //    g.FillRectangle(
                                    //        Controller.GetBackgroundColour(rowTreeNode.Parent), cell); // TODO - OK
                                    //}
                                    //else
                                    //{
                                    //    //g.FillRectangle(Brushes.AliceBlue, cell);
                                    //    g.FillRectangle(
                                    //       Controller.GetBackgroundColour( treeNode ), cell );
                                    //}
                                }
                                DrawWeight(g, rowModule, module, cell);
                            }

                            g.DrawRectangle(_borderPen, cell);

                            if (stateDisplay == 0)
                                stateDisplay++;
                        }
                        else if (stateDisplay == 1)
                            stateDisplay++;

                        x += Controller.DisplayOptions.CellHeight;
                        treeNode = iterator.Skip();
                    }
                }
            }
        }

        void DrawWeight(Graphics g, Element provider, Element consumer, Rectangle cell)
        {
            Relation relation = Controller.GetRelation(consumer, provider);

            int weight = 0;
            if (relation != null) weight = relation.Weight;

            if (weight > 0)
            {
                g.DrawString(weight.ToString(), Controller.DisplayOptions.WeightFont,
                    Brushes.Black, cell.Left + 1, cell.Top + 1);
            }
        }

        private void MatrixPanelMouseMove(object sender, MouseEventArgs e)
        {
            try

            {
                // TODO review null values etc

                if (Controller.Enabled)
                {
                    NodePanel hCurrent = _hLayout.LocatePanel(e.Location);
                    NodePanel vCurrent = _vLayout.LocatePanel(e.Location);

                    //if ( HasMouseMovedCell( e.Location, hCurrent, vCurrent ) )

                    //if ( Controller.RowNode != hCurrent.TreeNode || Controller.ColNode != vCurrent.TreeNode )
                    if ((hCurrent?.TreeNode != null && Controller.ProviderTreeNode != hCurrent.TreeNode) ||
                        (vCurrent?.TreeNode != null && Controller.ConsumerTreeNode != vCurrent.TreeNode))
                    {
                        //_vPanel = vCurrent;
                        //_hPanel = hCurrent;

                        // change in position
                        //Controller.ProviderModule = (hCurrent == null ) ? null : hCurrent.TreeNode.NodeValue;
                        //Controller.ConsumerModule = (vCurrent == null ) ? null : vCurrent.TreeNode.NodeValue;

                        Controller.SetCurrentModules(
                            hCurrent?.TreeNode,
                            vCurrent?.TreeNode);

                        DoTooltipAfterMouseMove(hCurrent, vCurrent);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }

        static string TooltipString(Element mod)
        {
            string tooltipText = "";
            if (mod != null)
            {
                tooltipText = $"[{mod.Id}] {mod.FullName}";

                if (mod.Type != null && mod.Type.Length > 0)
                {
                    tooltipText += $" ({mod.Type})";
                }
            }
            return tooltipText;
        }

        void DoTooltipAfterMouseMove(NodePanel hCurrent, NodePanel vCurrent)
        {
            try
            {
                _ttTimer.Stop();

                if (hCurrent == null && vCurrent == null)
                    return;

                if (hCurrent != null && vCurrent != null)
                {
                    if (hCurrent.TreeNode == null)
                    {
                        // header
                        _tooltip.SetToolTip(this, TooltipString(vCurrent.TreeNode.NodeValue));
                    }
                    else // _matrix
                    {

                        if (vCurrent.TreeNode == hCurrent.TreeNode)
                        {
                            _tooltip.SetToolTip(this, TooltipString(vCurrent.TreeNode.NodeValue));
                        }
                        else
                        {
                            Relation rel = Controller.GetRelation(vCurrent.TreeNode.NodeValue, hCurrent.TreeNode.NodeValue);

                            int weight = 0;

                            if (rel != null) weight = rel.Weight;

                            _tooltip.SetToolTip(this,
                                "Consumer: " + TooltipString(vCurrent.TreeNode.NodeValue) +
                                Environment.NewLine +
                                "Provider: " + TooltipString(hCurrent.TreeNode.NodeValue) +
                                Environment.NewLine +
                                "Weight: " + weight);
                        }
                    }
                }
                else
                {
                    _tooltip.SetToolTip(this, String.Empty);
                }

                Invalidate();
                _tooltip.Active = true;
                _ttTimer.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
        }

        private void MatrixPanelMouseLeave(object sender, EventArgs e)
        {
            //_hPanel = null;
            //_vPanel = null;
            //Controller.ProviderModule = null;
            //Controller.ConsumerModule = null;

            //this.Invalidate();

            try
            {

                Controller.SetCurrentModules(null, null);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }

        private void MatrixPanelDoubleClick(object sender, EventArgs e)
        {
            if (Controller.Enabled)
            {
                Controller.ExpandCurrentNode();
            }
            //Point pos = this.PointToClient(Control.MousePosition);

            //if (pos.Y > Controller.DisplayOptions.RootHeight)
            //{
            //    Controller.ExpandSelectedNode();
            //}
            //}
        }

        private void MatrixPanelMouseClick(object sender, MouseEventArgs e)
        {
            if (Controller.Enabled)
            {
                NodePanel providerNodePanel = _hLayout.LocatePanel(e.Location);
                NodePanel consumerNodePanel = _vLayout.LocatePanel(e.Location);

                if (providerNodePanel != null && consumerNodePanel != null)
                {
                    Controller.SelectProviderNode(providerNodePanel.TreeNode);
                    Controller.SelectConsumerNode(consumerNodePanel.TreeNode);

                    if (e.Button == MouseButtons.Right)
                    {
                        if (Controller.ContextMenuIsVisible)
                        {
                            Controller.HideContextMenu();
                            Invalidate();
                        }
                        else
                        {
                            Controller.ShowContextMenu(PointToScreen(e.Location));
                        }
                    }
                }
            }
        }
    }
}
