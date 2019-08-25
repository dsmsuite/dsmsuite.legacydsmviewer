using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DsmSuite.DsmViewer.View.Properties;
using DsmSuite.DsmViewer.Model.Data;
using DsmSuite.DsmViewer.Model.Collections;

namespace DsmSuite.DsmViewer.View.UserControls
{
    /// <summary>
    /// This is the panel which displays the types/namespaces and racts to expand/collapse requests
    /// </summary>
    public class TypeUserPanel : UserControl
    {
        private IContainer components;

        readonly Pen _borderPen;         // panel _borderPen pen
        //Pen          _fcPen;             // main font color Pen
        //Brush        _fcBrush;           // main font color Brush
        readonly StringFormat _vStringFormat;     // vertical string format

        readonly Image _imgExpanded;
        readonly Image _imgCollapsed;

        // TODO Correct public fields
        public MatrixControl Controller;
        public Rectangle ViewRectangle;

        readonly LayoutHelper _layout;
        readonly ToolTip _tooltip;
        readonly Timer _ttTimer;
        NodePanel _nodePanel;

        /// <summary>
        /// Constructor
        /// </summary>
		public TypeUserPanel()
        {
            InitializeComponent();

            Font sysFont = SystemFonts.MessageBoxFont;
            Font = new Font(sysFont.Name, sysFont.SizeInPoints, sysFont.Style);

            _borderPen = new Pen(Brushes.DarkGray, 1);

            _vStringFormat = new StringFormat(StringFormatFlags.DirectionVertical);

            _imgExpanded = Resources.Expanded;
            _imgCollapsed = Resources.Collapsed;

            _tooltip = new ToolTip();
            _ttTimer = new Timer {Interval = 4000};
            _ttTimer.Tick += TtTimerTick;

            _layout = new LayoutHelper();
        }

        void TtTimerTick(object sender, EventArgs e)
        {
            if (_tooltip.Active)
            {
                _tooltip.Active = false;
                _ttTimer.Stop();
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

            SuspendLayout();
            // 
            // TypeUserControl
            // 
            BackColor = SystemColors.Control;
            ForeColor = SystemColors.ControlText;
            Size = new Size(224, 3200);
            DoubleClick += TypePanelDoubleClick;
            MouseMove += TypePanelMouseMove;
            MouseClick += TypePanelMouseClick;
            ResumeLayout(false);

        }
        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Controller?.MatrixModel != null)
            {
                using (Graphics g = e.Graphics)
                {
                    _layout.Clear();

                    using (Bitmap image = new Bitmap(ViewRectangle.Width, ViewRectangle.Height))
                    {
                        using (Graphics g1 = Graphics.FromImage(image))
                        {
                            g1.Clip = new Region(ViewRectangle);

                            Draw(g1);

                            g.DrawImageUnscaled(image, 0, 0); //already clipped 
                        }
                    }
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Controller?.MatrixModel == null)
            {
                base.OnPaintBackground(e);
                string str = "No matrix is currently loaded";
                SizeF strSize = e.Graphics.MeasureString(str, Font);
                e.Graphics.DrawString(str, Font, SystemBrushes.GrayText,
                    (Width / 2) - ((int)strSize.Width / 2), 16);
            }
        }

        void Draw(Graphics g)
        {
            int y = -Controller.OffsetY + Controller.DisplayOptions.RootHeight;

            TreeIterator<Element> iterator = new TreeIterator<Element>(Controller.MatrixModel.Hierarchy);
            TreeNode<Element> treeNode = iterator.Next();

            while (treeNode != null)
            {
                if (treeNode.IsHidden == false)
                {
                    if (treeNode.IsCollapsed == false)
                    {
                        Rectangle bounds = new Rectangle(
                            treeNode.Depth * Controller.DisplayOptions.CellHeight,
                            y,
                            Controller.DisplayOptions.CellHeight,
                            Controller.CountNbDisplayableNested(treeNode) * Controller.DisplayOptions.CellHeight);

                        if (g.Clip.IsVisible(bounds))
                        {
                            DrawPanel(g, bounds, treeNode);
                            _layout.Add(new NodePanel(treeNode, bounds));
                        }

                        // y position does not change for next treeNode

                        treeNode = iterator.Next();

                    }
                    else
                    {
                        // treeNode is collapsed - draw the module at position
                        // if the treeNode has childrenskip them
                        int x = treeNode.Depth * Controller.DisplayOptions.CellHeight;

                        Rectangle bounds =
                            new Rectangle(x, y, Size.Width - x, Controller.DisplayOptions.CellHeight);

                        if (g.Clip.IsVisible(bounds))
                        {
                            DrawPanel(g, bounds, treeNode);
                            _layout.Add(new NodePanel(treeNode, bounds));
                        }

                        // position for next panel
                        y += Controller.DisplayOptions.CellHeight;

                        treeNode = iterator.Skip();
                    }
                }
                else
                {
                    treeNode = iterator.Next();
                }
            }

            Size = new Size(Size.Width, y + Controller.OffsetY);
            g.DrawRectangle(_borderPen, new Rectangle(0, 0, Width - 1, Height - 1));

            Rectangle rootBounds = new Rectangle(0, 0, Size.Width - 1, Controller.DisplayOptions.RootHeight - 2);
            DrawRootPanel(g, rootBounds);

        }

        void DrawPanel(Graphics g, Rectangle bounds, TreeNode<Element> treeNode)
        {
            Element module = treeNode.NodeValue;

            g.FillRectangle(
                Controller.ProviderTreeNode == treeNode ? Brushes.White : Controller.GetBackgroundColour(treeNode, null), bounds);

            if (treeNode.HasChildren)
            {
                if (treeNode.IsCollapsed)
                {
                    g.DrawImage(_imgCollapsed, bounds.Left + 4, bounds.Top + 4);
                    g.DrawString(module.Name + " - " + module.Id,
                        GetNodeFont(treeNode), Brushes.Black, bounds.Left + 18, bounds.Top + 2);
                }
                else
                {
                    g.DrawImage(_imgExpanded, bounds.Left + 4, bounds.Top + 4);
                    g.DrawString(module.Name, GetNodeFont(treeNode), Brushes.Black,
                        bounds.Left, bounds.Top + 18, _vStringFormat);
                }
            }
            else
            {
                g.DrawString(module.Name + " - " + module.Id, GetNodeFont(treeNode), Brushes.Black,
                    bounds.Left + 2, bounds.Top + 2);
            }



            g.DrawRectangle(_borderPen, bounds);
        }

        void DrawRootPanel(Graphics g, Rectangle bounds)
        {
            g.FillRectangle(SystemBrushes.ControlLight, bounds);

            TreeNode<Element> sn = Controller.MatrixModel.SelectedTreeNode;
            string text = sn != null ? sn.NodeValue.FullName : "<No module currently selected>";

            g.DrawString(text, Controller.DisplayOptions.TextFont,
                Brushes.CornflowerBlue, bounds.X + 1, bounds.Y + 3);

            g.DrawRectangle(_borderPen, bounds);

            g.DrawLine(new Pen(Brushes.White, 1),
                bounds.Left, bounds.Bottom + 1, bounds.Right, bounds.Bottom + 1);

        }

        void TypePanelDoubleClick(object sender, EventArgs e)
        {
            if (Controller.Enabled)
            {
                Point pos = PointToClient(MousePosition);

                if (pos.Y > Controller.DisplayOptions.RootHeight)
                {
                    Controller.ExpandSelectedNode();
                }
            }
        }

        void TypePanelMouseClick(object sender, MouseEventArgs e)
        {
            if (Controller.Enabled)
            {
                NodePanel nodePanel = _layout.LocatePanel(e.Location);

                if (nodePanel != null)
                {
                    Controller.SelectNode(nodePanel.TreeNode);

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

        Font GetNodeFont(TreeNode<Element> treeNode)
        {
            if (treeNode == Controller.MatrixModel.SelectedTreeNode)
            {
                return new Font(Controller.DisplayOptions.TextFont, FontStyle.Bold);
            }

            return Controller.DisplayOptions.TextFont;
        }

        void DoTooltipAfterMouseMove(Point p)
        {
            NodePanel current = _layout.LocatePanel(p);

            if (LayoutHelper.MovedTest(_nodePanel, current, p))
            {
                _ttTimer.Stop();

                if (current == null) return;

                Controller.SetCurrentModules(current.TreeNode, Controller.ConsumerTreeNode);

                _nodePanel = current;
                _tooltip.SetToolTip(this, TooltipString(current.TreeNode.NodeValue));
                _tooltip.Active = true;
                _ttTimer.Start();
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

        private void TypePanelMouseMove(object sender, MouseEventArgs e)
        {
            if (Controller.Enabled)
            {
                DoTooltipAfterMouseMove(e.Location);
            }
        }
    }
}
