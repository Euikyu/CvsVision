using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace CvsVision.Caliper.Controls
{
    /// <summary>
    /// CircleSettingGraphic.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CircleSettingGraphic : UserControl, INotifyPropertyChanged
    {
        #region Fields
        private readonly object m_MoveLock = new object();

        private bool m_IsCaptured;
        private Point m_LastMovePoint;
        private Point m_LastSizePoint;
        private Point m_StartPoint;
        private Point m_EndPoint;
        #endregion

        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #region Common Properties
        
        public Point StartPoint
        {
            get { return m_StartPoint; }
            set
            {
                m_StartPoint = value; this.RaisePropertyChanged(nameof(StartPoint));
            }
        }
        public Point EndPoint
        {
            get { return m_EndPoint; }
            set
            {
                m_EndPoint = value;
                this.RaisePropertyChanged(nameof(EndPoint));
            }
        }        
        public double SpanAngle
        {
            get
            {
                return (EndAngle - StartAngle + 360) % 360; 
            }
        }
        public double IntervalAngle
        {
            get
            {
                return this.SpanAngle / (this.CaliperCount - 1);
            }
        }
        #endregion

        #region Dependency Properties
        //필요한 속성
        //원점 X
        //원점 Y
        //반지름
        //중심점 = [(원점X + 반지름), (원점Y + 반지름)]
        //시작점 각도
        //끝점 각도
        //사이각 = [(시작각 - 끝각) / 캘리퍼 개수]
        //호 방향
        //캘리퍼 방향
        //캘리퍼 개수
        //투사 길이
        //검색 길이




        /// <summary>
        /// CircleSettingGraphic.OriginX 에 대한 종속성 속성을 식별합니다.
        /// </summary>
        public static readonly DependencyProperty OriginXProperty =
            DependencyProperty.Register(nameof(OriginX), typeof(double), typeof(CircleSettingGraphic));
        /// <summary>
        /// CircleSettingGraphic.OriginY 에 대한 종속성 속성을 식별합니다.
        /// </summary>
        public static readonly DependencyProperty OriginYProperty =
            DependencyProperty.Register(nameof(OriginY), typeof(double), typeof(CircleSettingGraphic));
        /// <summary>
        /// CircularPanel.Radius 에 대한 종속성 속성을 식별합니다.
        /// </summary>
        public static readonly DependencyProperty IsOutwardDirectionProperty =
            DependencyProperty.Register(nameof(IsOutwardDirection), typeof(bool), typeof(CircleSettingGraphic));

        public static readonly DependencyProperty SearchLengthProperty =
                    DependencyProperty.Register(nameof(SearchLength), typeof(double), typeof(CircleSettingGraphic));

        public static readonly DependencyProperty ProjectionLengthProperty =
                    DependencyProperty.Register(nameof(ProjectionLength), typeof(double), typeof(CircleSettingGraphic));

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(CircleSettingGraphic),
                new PropertyMetadata(StartAngle_PropertyChanged));

        private static void StartAngle_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CircleSettingGraphic control = (CircleSettingGraphic)o;
            var v = control.GetPointByRotation(new Point(control.Radius, 0), (double)e.NewValue * Math.PI / 180, new Point()) - new Point(-control.Radius, -control.Radius);
            control.StartPoint = new Point(v.X, v.Y);
            control.UpdateCircle();
        }

        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register(nameof(EndAngle), typeof(double), typeof(CircleSettingGraphic),
                new PropertyMetadata(EndAngle_PropertyChanged));

        private static void EndAngle_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CircleSettingGraphic control = (CircleSettingGraphic)o;
            var v = control.GetPointByRotation(new Point(control.Radius, 0), (double)e.NewValue * Math.PI / 180, new Point()) - new Point(-control.Radius, -control.Radius);
            control.EndPoint = new Point(v.X, v.Y);
            control.UpdateCircle();
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register(nameof(Radius), typeof(double), typeof(CircleSettingGraphic),
                new PropertyMetadata(0.0, Radius_PropertyChanged));

        private static void Radius_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CircleSettingGraphic control = (CircleSettingGraphic)o;
            var radius = (double)e.NewValue;
            if (control.Diameter != radius * 2) control.Diameter = radius * 2;

            
            var startV = control.GetPointByRotation(new Point(radius, 0), control.StartAngle * Math.PI / 180, new Point()) - new Point(-radius, -radius);
            var endV = control.GetPointByRotation(new Point(radius, 0), control.EndAngle * Math.PI / 180, new Point()) - new Point(-radius, -radius);

            control.StartPoint = new Point(startV.X, startV.Y);
            control.EndPoint = new Point(endV.X, endV.Y);
            control.UpdateCircle();
        }

        public static readonly DependencyProperty DiameterProperty =
            DependencyProperty.Register(nameof(Diameter), typeof(double), typeof(CircleSettingGraphic),
                new PropertyMetadata(Diameter_PropertyChanged));

        private static void Diameter_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CircleSettingGraphic control = (CircleSettingGraphic)o;
            if (control.Radius != (double)e.NewValue / 2) control.Radius = (double)e.NewValue / 2;
        }

        public static readonly DependencyProperty CaliperCountProperty =
                    DependencyProperty.Register(nameof(CaliperCount), typeof(int), typeof(CircleSettingGraphic),
                        new PropertyMetadata(3, CaliperCount_PropertyChanged, CaliperCount_CoerceValue));
        private static void CaliperCount_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CircleSettingGraphic control = (CircleSettingGraphic)o;
            control.RaisePropertyChanged(nameof(IntervalAngle));
        }
        private static object CaliperCount_CoerceValue(DependencyObject o, object baseValue)
        {
            CircleSettingGraphic control = (CircleSettingGraphic)o;
            if (baseValue is int count)
            {
                if (count < 4) return 4;
                else if (count > 200) return 200;
                else return count;
            }
            else return 4;
        }
        /// <summary>
        /// 캘리퍼의 개수를 가져오거나 설정합니다.
        /// </summary>
        public int CaliperCount
        {
            get { return (int)GetValue(CaliperCountProperty); }
            set { SetValue(CaliperCountProperty, value); }
        }
        public bool IsOutwardDirection
        {
            get { return (bool)GetValue(IsOutwardDirectionProperty); }
            set { SetValue(IsOutwardDirectionProperty, value); }
        }

        /// <summary>
        /// 각 캘리퍼가 탐색할 길이(높이)를 가져오거나 설정합니다.
        /// </summary>
        public double SearchLength
        {
            get { return (double)GetValue(SearchLengthProperty); }
            set { if (value > 0) SetValue(SearchLengthProperty, value); }
        }

        /// <summary>
        /// 각 캘리퍼가 투사할 길이(너비)를 가져오거나 설정합니다.
        /// </summary>
        public double ProjectionLength
        {
            get { return (double)GetValue(ProjectionLengthProperty); }
            set { if (value > 0) SetValue(ProjectionLengthProperty, value); }
        }
        /// <summary>
        /// 그래픽의 원점 X 좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginX
        {
            get { return (double)GetValue(OriginXProperty); }
            set { SetValue(OriginXProperty, value); }
        }
        /// <summary>
        /// 그래픽의 원점 Y 좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginY
        {
            get { return (double)GetValue(OriginYProperty); }
            set { SetValue(OriginYProperty, value); }
        }

        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }
        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public double Diameter
        {
            get { return (double)GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }
        #endregion

        #endregion

        public CircleSettingGraphic()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region Methods
        private void UpdateCircle()
        {
            this.RaisePropertyChanged(nameof(SpanAngle));
            this.RaisePropertyChanged(nameof(IntervalAngle));
            this.RaisePropertyChanged(nameof(ProjectionLength));
            this.RaisePropertyChanged(nameof(SearchLength));
        }
        /// <summary>
        /// 현재 그래픽의 중심 좌표 반환하기.
        /// </summary>
        /// <returns></returns>
        private Point GetCenter()
        {
            return new Point(this.OriginX + this.Width / 2, this.OriginY + this.Height / 2);
        }
        /// <summary>
        /// 지정된 중심축을 기준으로 회전 변환한 점을 반환하기.
        /// </summary>
        /// <param name="rotatePoint">회전 변환할 점.</param>
        /// <param name="rad">회전 라디안 값.</param>
        /// <param name="center">중심축 점.</param>
        /// <returns></returns>
        private Point GetPointByRotation(Point rotatePoint, double rad, Point center)
        {
            return new Point(Math.Cos(rad) * rotatePoint.X - Math.Sin(rad) * rotatePoint.Y - center.X, Math.Sin(rad) * rotatePoint.X + Math.Cos(rad) * rotatePoint.Y - center.Y);
        }
        #endregion

        #region Events

        private void Circle_Loaded(object sender, RoutedEventArgs e)
        {
            //StartAngle = 180;
            //EndAngle = 0;
            //ProjectionLength = 30;
            //SearchLength = 100;
            //CaliperCount = 10;

            this.UpdateCircle();
        }

        //외부에서 Search Length, Projection Length 변경 시 호출하도록 설정
        private void Edge_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var xMargin = -ProjectionLength / 2;
            var yMargin = -SearchLength / 2;
            (sender as EdgeSettingGraphic).Margin = new Thickness(xMargin, yMargin, xMargin, yMargin);
        }

        private void Circle_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement control)
            {
                switch (control.Name)
                {
                    case "StartPoint_Grid":
                        this.Cursor = Cursors.Cross;
                        break;
                    case "EndPoint_Grid":
                        this.Cursor = Cursors.Cross;
                        break;
                    case "Movable_Ellipse":
                        this.Cursor = Cursors.SizeAll;
                        break;
                    case "Radius_Grid":
                        this.Cursor = Cursors.SizeNS;
                        break;
                }
            }
        }

        private void Circle_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement control)
            {
                this.Cursor = Cursors.Arrow;
            }
        }
        private void Circle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is IInputElement element && this.Parent is Canvas canvas)
            {
                m_LastSizePoint = e.GetPosition(canvas);
                element.CaptureMouse();
                m_IsCaptured = true;

                e.Handled = true;
            }
        }

        private void Circle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lock (m_MoveLock)
            {
                Mouse.Capture(null);
                m_IsCaptured = false;
                m_LastMovePoint = new Point();
                m_LastSizePoint = new Point();

                e.Handled = true;

            }
        }


        private void Circle_MouseMove(object sender, MouseEventArgs e)
        {
            lock (m_MoveLock)
            {
                //마우스 캡쳐 상태일 때에만 일련 동작 진행
                if (m_IsCaptured)
                {
                    if (sender is FrameworkElement control && this.Parent is Canvas canvas)
                    {
                        e.Handled = true;

                        Vector sizeOffset = e.GetPosition(canvas) - m_LastSizePoint;
                        switch (control.Name)
                        {
                            case "StartPoint_Grid":
                                var startCenter = this.GetCenter();
                                var startP = e.GetPosition(canvas);
                                this.StartAngle = startP.X - startCenter.X == 0 ? (startCenter.Y > startP.Y ? -90 : 90) : Math.Atan2(startP.Y - startCenter.Y, startP.X - startCenter.X) * 180 / Math.PI;                                
                                break;

                            case "EndPoint_Grid":
                                var endCenter = this.GetCenter();
                                var endP = e.GetPosition(canvas);
                                this.EndAngle = endP.X - endCenter.X == 0 ? (endCenter.Y > endP.Y ? -90 : 90) : Math.Atan2(endP.Y - endCenter.Y, endP.X - endCenter.X) * 180 / Math.PI;                                
                                break;

                            case "Movable_Ellipse":
                                if (m_LastMovePoint.X == 0 && m_LastMovePoint.Y == 0)
                                {
                                    m_LastMovePoint = e.GetPosition(canvas);
                                    break;
                                }
                                Vector moveOffset = e.GetPosition(canvas) - m_LastMovePoint;
                                this.OriginX += moveOffset.X;
                                this.OriginY += moveOffset.Y;
                                m_LastMovePoint = e.GetPosition(canvas);
                                break;

                            case "Radius_Grid":
                                if (Radius - sizeOffset.Y > 20)
                                {
                                    this.OriginX += sizeOffset.Y;
                                    this.OriginY += sizeOffset.Y;
                                    Radius -= sizeOffset.Y;
                                    m_LastSizePoint = e.GetPosition(canvas);

                                }
                                else if (Radius != 20)
                                {
                                    this.OriginX += 20 - (this.Radius - sizeOffset.Y);
                                    this.OriginY += 20 - (this.Radius - sizeOffset.Y);
                                    Radius = 20;
                                }
                                break;
                        }
                    }
                }
            }
        }

        #endregion
    }

    internal class CircularEdgePanel : Panel
    {

        /// <summary>
        /// CircularPanel.StartAngle 에 대한 종속성 속성을 식별합니다.
        /// </summary>
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(CircularEdgePanel),
                new PropertyMetadata(0.0, StartAngle_PropertyChanged, StartAngle_CoerceValue));

        private static object StartAngle_CoerceValue(DependencyObject o, object baseValue)
        {
            CircularEdgePanel panel = (CircularEdgePanel)o;
            if (baseValue is double angle)
            {
                return (angle + 360) % 360;
            }
            else return panel.StartAngle;
        }

        private static void StartAngle_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CircularEdgePanel panel = (CircularEdgePanel)o;
            panel.ArrangeOverride(panel.RenderSize);
        }

        /// <summary>
        /// CircularPanel.IntervalAngle 에 대한 종속성 속성을 식별합니다.
        /// </summary>
        public static readonly DependencyProperty IntervalAngleProperty =
            DependencyProperty.Register(nameof(IntervalAngle), typeof(double), typeof(CircularEdgePanel),
                new PropertyMetadata(IntervalAngle_PropertyChanged));

        private static void IntervalAngle_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CircularEdgePanel panel = (CircularEdgePanel)o;
            panel.ArrangeOverride(panel.RenderSize);
        }

        /// <summary>
        /// CircularPanel.Radius 에 대한 종속성 속성을 식별합니다.
        /// </summary>
        public static readonly DependencyProperty IsOutwardDirectionProperty =
            DependencyProperty.Register(nameof(IsOutwardDirection), typeof(bool), typeof(CircularEdgePanel),
                new PropertyMetadata(false, IsOutwardDirection_PropertyChanged));

        private static void IsOutwardDirection_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CircularEdgePanel panel = (CircularEdgePanel)o;
            panel.ArrangeOverride(panel.RenderSize);
        }

        /// <summary>
        /// CircularPanel.Radius 에 대한 종속성 속성을 식별합니다.
        /// </summary>
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register(nameof(Radius), typeof(double), typeof(CircularEdgePanel),
                new PropertyMetadata(double.NaN, Radius_PropertyChanged));

        private static void Radius_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is double)
            {
                CircularEdgePanel panel = (CircularEdgePanel)o;
                panel.Width = (double)e.NewValue * 2;
                panel.Height = (double)e.NewValue * 2;
            }
        }

        public static readonly DependencyProperty CaliperCountProperty =
                    DependencyProperty.Register(nameof(CaliperCount), typeof(int), typeof(CircularEdgePanel),
                        new PropertyMetadata(3, CaliperCount_PropertyChanged, CaliperCount_CoerceValue));
        private static void CaliperCount_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CircularEdgePanel panel = (CircularEdgePanel)o;

            var newCount = (int)e.NewValue > 200 ? 200 : ((int)e.NewValue < 4 ? 4 : (int)e.NewValue);
            panel.Children.Clear();
            for(int i = 0; i < newCount; i++) panel.Children.Add(new EdgeSettingGraphic { Width = panel.ProjectionLength, Height = panel.SearchLength, IsGrouped = true });
        }
        private static object CaliperCount_CoerceValue(DependencyObject o, object baseValue)
        {
            if (baseValue is int count)
            {
                if (count < 4) return 4;
                else if (count > 200) return 200;
                else return count;
            }
            else return 4;
        }
        public static readonly DependencyProperty SearchLengthProperty =
                    DependencyProperty.Register(nameof(SearchLength), typeof(double), typeof(CircularEdgePanel),
                        new PropertyMetadata(SearchLength_PropertyChanged));

        private static void SearchLength_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CircularEdgePanel panel = (CircularEdgePanel)o;
            foreach(var child in panel.Children)
            {
                if(child is EdgeSettingGraphic g)
                {
                    g.Height = (double)e.NewValue;
                }
            }
        }

        public static readonly DependencyProperty ProjectionLengthProperty =
                    DependencyProperty.Register(nameof(ProjectionLength), typeof(double), typeof(CircularEdgePanel),
                        new PropertyMetadata(ProjectionLength_PropertyChanged));

        private static void ProjectionLength_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CircularEdgePanel panel = (CircularEdgePanel)o;
            foreach (var child in panel.Children)
            {
                if (child is EdgeSettingGraphic g)
                {
                    g.Width = (double)e.NewValue;
                }
            }
        }

        /// <summary>
        /// 캘리퍼의 개수를 가져오거나 설정합니다.
        /// </summary>
        public int CaliperCount
        {
            get { return (int)GetValue(CaliperCountProperty); }
            set { SetValue(CaliperCountProperty, value); }
        }

        /// <summary>
        /// 각 캘리퍼가 탐색할 길이(높이)를 가져오거나 설정합니다.
        /// </summary>
        public double SearchLength
        {
            get { return (double)GetValue(SearchLengthProperty); }
            set { if (value > 0) SetValue(SearchLengthProperty, value); }
        }

        /// <summary>
        /// 각 캘리퍼가 투사할 길이(너비)를 가져오거나 설정합니다.
        /// </summary>
        public double ProjectionLength
        {
            get { return (double)GetValue(ProjectionLengthProperty); }
            set { if (value > 0) SetValue(ProjectionLengthProperty, value); }
        }

        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public double IntervalAngle
        {
            get { return (double)GetValue(IntervalAngleProperty); }
            set { SetValue(IntervalAngleProperty, value); }
        }
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }
        public bool IsOutwardDirection
        {
            get { return (bool)GetValue(IsOutwardDirectionProperty); }
            set { SetValue(IsOutwardDirectionProperty, value); }
        }

        /// <summary>
        /// 지정된 중심축을 기준으로 회전 변환한 점을 반환하기.
        /// </summary>
        /// <param name="rotatePoint">회전 변환할 점.</param>
        /// <param name="rad">회전 라디안 값.</param>
        /// <param name="center">중심축 점.</param>
        /// <returns></returns>
        private Point GetPointByRotation(Point rotatePoint, double rad, Point center)
        {
            return new Point(Math.Cos(rad) * rotatePoint.X - Math.Sin(rad) * rotatePoint.Y - center.X, Math.Sin(rad) * rotatePoint.X + Math.Cos(rad) * rotatePoint.Y - center.Y);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
            }
            return availableSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            double radius = double.IsNaN(this.Radius) ? (this.ActualWidth > this.ActualHeight ? this.ActualHeight / 2 : this.ActualWidth / 2) : this.Radius;
            int count = this.Children.Count;

            for (int i = 0; i < count; i++)
            {
                if (Children[i] is FrameworkElement element)
                {
                    var angle = this.StartAngle + i * this.IntervalAngle;
                    RotateTransform r = new RotateTransform
                    {
                        Angle = angle + (IsOutwardDirection ? -90 : 90),
                        CenterX = element.Width / 2,
                        CenterY = element.Height / 2
                    };

                    element.RenderTransform = r;

                    var v = this.GetPointByRotation(new Point(radius, 0), angle * Math.PI / 180, new Point()) - new Point(-radius, -radius);

                    element.Arrange(new Rect(v.X - r.CenterX, v.Y - r.CenterY, element.Width, element.Height));
                }
            }

            return finalSize;
        }
    }

    #region Converters
    internal class PointToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //value는 점
            //리턴 값은 Margin
            if (value is Point p)
            {
                return new Thickness(p.X - 7, p.Y - 7, -p.X, -p.Y);
            }
            else
            {
                return new Thickness();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class DiameterToSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //value는 지름
            //리턴 값은 Size
            if (value is double diameter)
            {
                return new Size(diameter, diameter);
            }
            else
            {
                return new Size();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class AngleToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //value는 각도
            //리턴 값은 IsLargeArc
            if (value is double angle)
            {
                if (angle > 180 || (angle < 0 && angle > -180)) return true;
                else return false;
            }
            else
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
