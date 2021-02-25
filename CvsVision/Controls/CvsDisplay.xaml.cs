﻿using System;
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

namespace CvsVision.Controls
{
    /// <summary>
    /// CvsDisplay.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CvsDisplay : UserControl, INotifyPropertyChanged
    {
        #region Fields

        #endregion

        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #region Common Properties


        #endregion

        #region Dependency Properties
        //public static readonly DependencyProperty OriginXProperty =
        //    DependencyProperty.Register("OriginX", typeof(double), typeof(SymmetryRectangle));

        //public static readonly DependencyProperty OriginYProperty =
        //    DependencyProperty.Register("OriginY", typeof(double), typeof(SymmetryRectangle));

        //public static readonly DependencyProperty IsGroupedProperty =
        //    DependencyProperty.Register("IsGrouped", typeof(bool), typeof(SymmetryRectangle));

        ///// <summary>
        ///// 이 도형의 원점 X좌표를 가져오거나 설정합니다.
        ///// </summary>
        //public double OriginX
        //{
        //    get { return (double)GetValue(OriginXProperty); }
        //    set
        //    {
        //        SetValue(OriginXProperty, value);
        //    }
        //}

        ///// <summary>
        ///// 이 도형의 원점 Y좌표를 가져오거나 설정합니다.
        ///// </summary>
        //public double OriginY
        //{
        //    get { return (double)GetValue(OriginYProperty); }
        //    set { SetValue(OriginYProperty, value); }
        //}

        ///// <summary>
        ///// 이 도형이 여러개의 그룹으로 묶이는 지 아니면 단독으로 쓰이는 지 여부를 가져오거나 설정합니다.
        ///// </summary>
        //public bool IsGrouped
        //{
        //    get { return (bool)GetValue(IsGroupedProperty); }
        //    set { SetValue(IsGroupedProperty, value); }
        //}
        #endregion

        #endregion

        public CvsDisplay()
        {
            InitializeComponent();
        }

        #region Methods

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