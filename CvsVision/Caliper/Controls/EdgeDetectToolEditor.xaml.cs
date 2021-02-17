using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZoomPanCon;

namespace CvsVision.Caliper.Controls
{
    /// <summary>
    /// EdgeDetectToolEditor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EdgeDetectToolEditor : UserControl, INotifyPropertyChanged
    {
        #region Fields
        private bool m_IsEditing;
        private System.Drawing.Bitmap m_CurrentBitmap;
        private BitmapSource m_OriginSource;
        private DrawingImage m_OverlaySource;
        private CvsEdgeDetectTool m_Tool;

        #endregion

        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #region Common Properties
        public bool IsEditing
        {
            get { return m_IsEditing; }
            set
            {
                if (m_Tool != null)
                {
                    m_IsEditing = value;
                    this.RaisePropertyChanged(nameof(IsEditing));
                }
                else
                {
                    m_IsEditing = false;
                    this.RaisePropertyChanged(nameof(IsEditing));
                }
            }
        }
        public double ProjectionLength
        {
            get
            {
                if (m_Tool != null && m_Tool.Region != null) return (m_Tool.Region as CvsRectangleAffine).Width;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Region != null)
                {
                    (m_Tool.Region as CvsRectangleAffine).Width = value;
                    this.RaisePropertyChanged(nameof(ProjectionLength));
                }
            }
        }
        public double SearchLength
        {
            get
            {
                if (m_Tool != null && m_Tool.Region != null) return (m_Tool.Region as CvsRectangleAffine).Height;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Region != null)
                {
                    (m_Tool.Region as CvsRectangleAffine).Height = value;
                    this.RaisePropertyChanged(nameof(SearchLength));
                }
            }
        }
        public double OriginX
        {
            get
            {
                if (m_Tool != null && m_Tool.Region != null) return (m_Tool.Region as CvsRectangleAffine).OriginX;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Region != null)
                {
                    (m_Tool.Region as CvsRectangleAffine).OriginX = value;
                    this.RaisePropertyChanged(nameof(OriginX));
                }
            }
        }
        public double OriginY
        {
            get
            {
                if (m_Tool != null && m_Tool.Region != null) return (m_Tool.Region as CvsRectangleAffine).OriginY;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Region != null)
                {
                    (m_Tool.Region as CvsRectangleAffine).OriginY = value;
                    this.RaisePropertyChanged(nameof(OriginY));
                }
            }
        }

        public uint ContrastThreshold
        {
            get
            {
                if (m_Tool != null) return m_Tool.ContrastThreshold;
                else return 0;
            }
            set
            {
                if(m_Tool != null)
                {
                    m_Tool.ContrastThreshold = value;
                    this.RaisePropertyChanged(nameof(ContrastThreshold));
                }
            }
        }
        public uint HalfPixelCount
        {
            get
            {
                if (m_Tool != null) return m_Tool.HalfPixelCount;
                else return 0;
            }
            set
            {
                if(m_Tool != null)
                {
                    m_Tool.HalfPixelCount = value;
                    this.RaisePropertyChanged(nameof(HalfPixelCount));
                }
            }
        }
        public int SelectedEdgeDirection
        {
            get
            {
                if (m_Tool != null) return (int)m_Tool.EdgeDirection;
                else return -1;
            }
            set
            {
                if(m_Tool != null)
                {
                    m_Tool.EdgeDirection = (EDirection)value;
                    this.RaisePropertyChanged(nameof(SelectedEdgeDirection));
                }
            }
        }
        public BitmapSource OriginSource
        {
            get { return m_OriginSource; }
            set
            {
                m_OriginSource = value;
                this.RaisePropertyChanged(nameof(OriginSource));
                OverlaySource = null;

                ImageWidth = m_OriginSource.Width;
                ImageHeight = m_OriginSource.Height;
                this.RaisePropertyChanged(nameof(ImageWidth));
                this.RaisePropertyChanged(nameof(ImageHeight));
            }
        }
        public DrawingImage OverlaySource
        {
            get { return m_OverlaySource; }
            private set
            {
                m_OverlaySource = value;
                this.RaisePropertyChanged(nameof(OverlaySource));
            }
        }
        public double ImageWidth { get; private set; }
        public double ImageHeight { get; private set; }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty SubjectToolProperty =
            DependencyProperty.Register(nameof(SubjectTool), typeof(CvsEdgeDetectTool), typeof(EdgeDetectToolEditor),
                new PropertyMetadata(new CvsEdgeDetectTool()));

        public CvsEdgeDetectTool SubjectTool
        {
            get { return GetValue(SubjectToolProperty) as CvsEdgeDetectTool; }
            set
            {
                SetValue(SubjectToolProperty, value);
                m_Tool = value;
            }
        }
        #endregion

        #endregion

        public EdgeDetectToolEditor()
        {
            InitializeComponent();
            DataContext = this;
            m_Tool = new CvsEdgeDetectTool();
        }

        #region Methods


        #region Events
        private void LoadImageBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Bitmap Image Files (*.bmp)|*.bmp"
            };
            if ((bool)d.ShowDialog())
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(d.FileName);
                if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                {
                    MessageBox.Show("It support only Format8bppIndexed.");
                }
                else
                {
                    m_CurrentBitmap = bmp;
                    var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                    OriginSource = BitmapSource.Create(data.Width, data.Height, bmp.HorizontalResolution, bmp.VerticalResolution, PixelFormats.Gray8, null, data.Scan0, data.Stride * data.Height, data.Stride);
                    OriginSource.Freeze();
                    bmp.UnlockBits(data);
                    if (SubjectTool.InputImage != null) SubjectTool.InputImage.Dispose();
                    SubjectTool.InputImage = m_CurrentBitmap;
                }
            }
        }
        private void LoadToolBtn_Click(object sender, RoutedEventArgs e)
        {
            //Tool 불러오는 과정 실행해야함
            SubjectTool.Load("<Input the loading file path>", typeof(CvsEdgeDetectTool));
        }
        private void SaveToolBtn_Click(object sender, RoutedEventArgs e)
        {
            //Tool 저장
            SubjectTool.Save("<Input the saving file path>");
        }
        private void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            SubjectTool.Run();
            var overlayImg = new DrawingImage(SubjectTool.Overlay);
            overlayImg.Freeze();
            OverlaySource = overlayImg;

            IsEditing = false;
        }
        #endregion

        #endregion

        #region - Zoompan Control
        /*
         * * memberVar
         */
        /// <summary>
        /// The point that was clicked relative to the ZoomAndPanControl.
        /// </summary>
        private Point origZoomAndPanControlMouseDownPoint;

        /// <summary>
        /// Records which mouse button clicked during mouse dragging.
        /// </summary>
        private MouseButton mouseButtonDown;

        /// <summary>
        /// Specifies the current state of the mouse handling logic.
        /// </summary>
        private MouseHandlingMode mouseHandlingMode = MouseHandlingMode.None;

        /// <summary>
        /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
        /// </summary>
        private Point origContentMouseDownPoint;
        /*
         * * property
         */

        /*
         * * method
         */
        /// <summary>
        /// Zoom the viewport in, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomIn(Point contentZoomCenter)
        {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale * 1.2, contentZoomCenter);
        }

        /// <summary>
        /// Zoom the viewport out, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomOut(Point contentZoomCenter)
        {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale / 1.2, contentZoomCenter);
        }
        /*
         * Callback
         */
        /// <summary>
        /// Event raised by rotating the mouse wheel
        /// </summary>

        /// <summary>
        /// The 'ZoomIn' command (bound to the plus key) was executed.
        /// </summary>
        private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomIn(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
        }

        /// <summary>
        /// The 'ZoomOut' command (bound to the minus key) was executed.
        /// </summary>
        private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomOut(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
        }

        private void ZoomAndPanControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (e.Delta > 0)
            {
                Point curContentMousePoint = e.GetPosition(ImgCanvas);
                ZoomIn(curContentMousePoint);
            }
            else if (e.Delta < 0)
            {
                Point curContentMousePoint = e.GetPosition(ImgCanvas);
                ZoomOut(curContentMousePoint);
            }

        }
        /// <summary>
        /// Event raised on mouse down in the ZoomAndPanControl.
        /// </summary>
        private void ZoomAndPanControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ImgCanvas.Focus();

            mouseButtonDown = e.ChangedButton;
            origZoomAndPanControlMouseDownPoint = e.GetPosition(zoomAndPanControl);
            origContentMouseDownPoint = e.GetPosition(ImgCanvas);

            if (mouseButtonDown == MouseButton.Left)
            {
                // Just a plain old left-down initiates panning mode.
                mouseHandlingMode = MouseHandlingMode.Panning;
            }

            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                // Capture the mouse so that we eventually receive the mouse up event.
                zoomAndPanControl.CaptureMouse();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised on mouse up in the ZoomAndPanControl.
        /// </summary>
        private void ZoomAndPanControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                if (mouseHandlingMode == MouseHandlingMode.Zooming)
                {
                    if (mouseButtonDown == MouseButton.Left)
                    {
                        // Shift + left-click zooms in on the ImgCanvas.
                        ZoomIn(origContentMouseDownPoint);
                    }
                    else if (mouseButtonDown == MouseButton.Right)
                    {
                        // Shift + left-click zooms out from the ImgCanvas.
                        ZoomOut(origContentMouseDownPoint);
                    }
                }
                zoomAndPanControl.ReleaseMouseCapture();
                mouseHandlingMode = MouseHandlingMode.None;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised on mouse move in the ZoomAndPanControl.
        /// </summary>
        private void ZoomAndPanControl_MouseMove(object sender, MouseEventArgs e)
        {
            //shseol85: overlay랑 충돌
            //Color c = GetPixelColor(e.GetPosition(ImgCanvas));
            //PixelInfo = string.Format("{0}", c.B);

            if (mouseHandlingMode == MouseHandlingMode.Panning)
            {
                //
                // The user is left-dragging the mouse.
                // Pan the viewport by the appropriate amount.
                //
                Point curContentMousePoint = e.GetPosition(ImgCanvas);
                Vector dragOffset = curContentMousePoint - origContentMouseDownPoint;

                zoomAndPanControl.ContentOffsetX -= dragOffset.X;
                zoomAndPanControl.ContentOffsetY -= dragOffset.Y;

                e.Handled = true;
            }
            else if (mouseHandlingMode == MouseHandlingMode.Zooming)
            {
                Point curZoomAndPanControlMousePoint = e.GetPosition(zoomAndPanControl);
                Vector dragOffset = curZoomAndPanControlMousePoint - origZoomAndPanControlMouseDownPoint;
                double dragThreshold = 10;
                if (mouseButtonDown == MouseButton.Left && (Math.Abs(dragOffset.X) > dragThreshold ||
                                                            Math.Abs(dragOffset.Y) > dragThreshold))
                {
                    //
                    // When Shift + left-down zooming mode and the user drags beyond the drag threshold,
                    // initiate drag zooming mode where the user can drag out a rectangle to select the area
                    // to zoom in on.
                    //
                    mouseHandlingMode = MouseHandlingMode.DragZooming;
                    Point curContentMousePoint = e.GetPosition(ImgCanvas);
                    //InitDragZoomRect(origContentMouseDownPoint, curContentMousePoint); //LDH9999 추후
                }

                e.Handled = true;
            }
            /*LDH9999 추후
            else if (mouseHandlingMode == MouseHandlingMode.DragZooming)
            {
                //
                // When in drag zooming mode continously update the position of the rectangle
                // that the user is dragging out.
                //
                Point curContentMousePoint = e.GetPosition(content);
                SetDragZoomRect(origContentMouseDownPoint, curContentMousePoint);

                e.Handled = true;
            }*/
        }
        #endregion
    }
}
