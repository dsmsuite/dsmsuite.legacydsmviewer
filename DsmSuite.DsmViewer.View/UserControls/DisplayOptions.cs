using System.Drawing;

namespace DsmSuite.DsmViewer.View.UserControls
{
    /// <summary>
    /// Groups view parameters for the matrix
    /// </summary>
    internal class DisplayOptions
    {
        private readonly MatrixControl _matrix;
        private bool _showCyclic;
        private int _zoomLevel = 3;                   // Default index to ZoomLevel array
        private readonly ZoomLevel[] _zoomSettings = new ZoomLevel[6];  // Some standard displays - small to large
        
        /// <summary>
        /// Internal structure regrouping various hard coded settings for a particular zoom level 
        /// </summary>
        private struct ZoomLevel
        {
            public readonly int RootHeight;
            public readonly int CellHeight;
            public readonly Font TextFont;
            public readonly Font CellFont;

            public ZoomLevel(int rootHeight, int cellHeight, Font textFont, Font cellFont)
            {
                RootHeight = rootHeight;
                CellHeight = cellHeight;
                TextFont = textFont;
                CellFont = cellFont;
            }
        }

        /// <summary>
        /// Constructor of display options for the matrix control
        /// </summary>
        /// <param name="ctrl"></param>
        public DisplayOptions(MatrixControl ctrl)
        {
            _matrix = ctrl;

            Font sysFont = SystemFonts.MessageBoxFont;
            //this.Font = new Font(sysFont.Name, sysFont.SizeInPoints, sysFont.Style);

            for (int i = 0; i < 6; i++)
            {
                int fontSize = 5 + i * 2;
                int cellHeight = 3 * fontSize;
                int rootHeight = 6 * fontSize;
                _zoomSettings[i] = new ZoomLevel(rootHeight, cellHeight, new Font(sysFont.Name, fontSize), new Font("Tahoma", fontSize));
            }
        }

        /// <summary>
        /// Fixed size font for dependency weight values
        /// </summary>
        public Font WeightFont => ZoomSetting.CellFont;

        /// <summary>
        /// Turn on or off cyclic relation highlighter
        /// </summary>
        public bool ShowCyclicRelations
        {
            get
            {
                return _showCyclic;
            }
            set
            {
                _showCyclic = value;
                _matrix.NodeListModified(false);
            }
        }


        /// <summary>
        /// Gets the number of pixels for the height of matrix cells at the current zoom level
        /// </summary>
        public int CellHeight => ZoomSetting.CellHeight;


        /// <summary>
        /// Gets the height of the header row in pixels at the current zoom level
        /// </summary>
        public int RootHeight => ZoomSetting.RootHeight;


        /// <summary>
        /// Get the font for type labels at the current zoom level
        /// </summary>
        public Font TextFont => ZoomSetting.TextFont;

        /// <summary>
        /// Set the zoom level
        /// </summary>
        /// <param name="level"></param>
        public void SetZoomLevel(int level)
        {
            if (level != _zoomLevel)
            {
                if (level < 1)
                    _zoomLevel = 1;
                else if (level > 5)
                    _zoomLevel = 5;
                else
                    _zoomLevel = level;

                _matrix.NodeListModified(true);
            }
        }

        ZoomLevel ZoomSetting => _zoomSettings[_zoomLevel];
    }
}
