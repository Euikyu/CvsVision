using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CvsVision.Caliper.Controls
{
    /// <summary>
    /// LineSettingGraphic.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LineSettingGraphic : UserControl, INotifyPropertyChanged
    {

        #region Fields
        private readonly object m_MoveLock = new object();

        private bool m_IsCaptured;
        private Point m_LastMovePoint;

        private double m_LineOriginX;
        private double m_LineOriginY;
        private double m_LineWidth;
        private double m_LineThickness;

        private RotateTransform m_LineRotateTransform;
        private double m_Radian;

        #endregion

        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #region Common Properties
        /// <summary>
        /// 현재 선 도형의 회전 변환 정보를 가져옵니다.
        /// </summary>
        public RotateTransform LineRotateTransform
        {
            get { return m_LineRotateTransform; }
            private set
            {
                m_LineRotateTransform = value;
                RaisePropertyChanged(nameof(LineRotateTransform));
            }
        }

        /// <summary>
        /// 현재 라인에 대한 캘리퍼의 위치정보 리스트를 가져옵니다.
        /// </summary>
        public ObservableCollection<CvsPose> PoseCollection { get; }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty OriginXProperty =
                    DependencyProperty.Register(nameof(OriginX), typeof(double), typeof(LineSettingGraphic));

        public static readonly DependencyProperty OriginYProperty =
                    DependencyProperty.Register(nameof(OriginY), typeof(double), typeof(LineSettingGraphic));
        
        public static readonly DependencyProperty SearchLengthProperty =
                    DependencyProperty.Register(nameof(SearchLength), typeof(double), typeof(LineSettingGraphic));

        public static readonly DependencyProperty ProjectionLengthProperty =
                    DependencyProperty.Register(nameof(ProjectionLength), typeof(double), typeof(LineSettingGraphic));

        public static readonly DependencyProperty RotationProperty =
                    DependencyProperty.Register(nameof(Rotation), typeof(double), typeof(LineSettingGraphic),
                        new PropertyMetadata(Rotation_PropertyChanged));

        private static void Rotation_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            LineSettingGraphic control = (LineSettingGraphic)o;
            control.LineRotateTransform.Angle = (double)e.NewValue;
        }
        
        public static readonly DependencyProperty RadianProperty =
                    DependencyProperty.Register(nameof(Radian), typeof(double), typeof(LineSettingGraphic),
                        new PropertyMetadata(Radian_PropertyChanged));
        private static void Radian_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            LineSettingGraphic control = (LineSettingGraphic)o;
            control.LineRotateTransform.Angle = (double)e.NewValue * 180 / Math.PI;
        }

        public static readonly DependencyProperty CaliperCountProperty =
                    DependencyProperty.Register(nameof(CaliperCount), typeof(int), typeof(LineSettingGraphic),
                        new PropertyMetadata(2, CaliperCount_PropertyChanged, CaliperCount_CoerceValue));
        private static void CaliperCount_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            LineSettingGraphic control = (LineSettingGraphic)o;

            var newCount = (int)e.NewValue > 200 ? 200 : ((int)e.NewValue < 3 ? 3 : (int)e.NewValue);
            if (control.PoseCollection.Count != newCount)
            {
                if (control.Width == double.NaN || control.Width == 0) return;

                control.PoseCollection.Clear();
                var interval = control.Width / newCount;
                for (int i = 0; i < newCount; i++) control.PoseCollection.Add(new CvsPose { TranslateX = (i + 0.5) * interval });
            }
        }
        private static object CaliperCount_CoerceValue(DependencyObject o, object baseValue)
        {
            LineSettingGraphic control = (LineSettingGraphic)o;
            if (baseValue is int count)
            {
                if (count < 3) return 3;
                else if (count > 200) return 200;
                else return count;
            }
            else return 3;
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
            set { if(value > 0) SetValue(SearchLengthProperty, value); }
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
        /// 현재 선의 원점 X좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginX
        {
            get { return (double)GetValue(OriginXProperty); }
            set { SetValue(OriginXProperty, value); }
        }

        /// <summary>
        /// 현재 선의 원점 Y좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginY
        {
            get { return (double)GetValue(OriginYProperty); }
            set { SetValue(OriginYProperty, value); }
        }

        /// <summary>
        /// 현재 선의 각도를 Degree 값으로 가져오거나 설정합니다.
        /// </summary>
        public double Rotation
        {
            get { return (double)GetValue(RotationProperty); }
            set
            {
                m_Radian = value * (Math.PI / 180);
                SetValue(RadianProperty, m_Radian);
                SetValue(RotationProperty, value);
            }
        }

        /// <summary>
        /// 현재 선의 각도를 Radian 값으로 가져오거나 설정합니다.
        /// </summary>
        public double Radian
        {
            get { return (double)GetValue(RadianProperty); }
            set
            {
                SetValue(RadianProperty, value);
                m_Radian = value;
                var deg = value * (180 / Math.PI);
                SetValue(RotationProperty, deg);
            }
        }
        #endregion

        #endregion

        /// <summary>
        /// 이미지의 라인을 찾도록 돕는 사용자 컨트롤을 생성합니다.
        /// </summary>
        public LineSettingGraphic()
        {
            InitializeComponent();
            DataContext = this;
            PoseCollection = new ObservableCollection<CvsPose>();
            LineRotateTransform = new RotateTransform(m_Radian, 0, m_LineThickness / 2);
        }

        #region Methods
        /// <summary>
        /// 현재 데이터를 바탕으로 캘리퍼를 설정합니다.
        /// </summary>
        private void UpdateCaliper()
        {
            if (double.IsNaN(this.Width)) this.Width = 100;
            m_LineWidth = this.Width;
            m_LineOriginX = this.OriginX;
            m_LineOriginY = this.OriginY;
            Radian = m_Radian;

            RaisePropertyChanged(nameof(LineRotateTransform));
        }

        /// <summary>
        /// 현재 선의 회전 중심 축을 변경합니다.
        /// </summary>
        /// <param name="controlName">현재 클릭한 컨트롤의 이름.</param>
        private void MoveRotateOrigin(string controlName)
        {
            //선의 시작점이 중심축이었고 이제 선의 시작점을 이동시켜야 할 경우
            if (LineRotateTransform.CenterX == 0 && controlName.Contains("Start"))
            {
                //새로 중심축이 될 좌표를 현재 중심축에서 로테이션한 좌표로 변환
                var newCenter = this.GetPointByRotation(new Point(this.Width, 0), m_Radian, new Point()) - new Point(-(this.OriginX), -(this.OriginY + m_LineThickness / 2));

                //중심 축 이동
                LineRotateTransform = new RotateTransform(Rotation, this.Width, m_LineThickness / 2);
                RaisePropertyChanged(nameof(LineRotateTransform));

                //변환한 중심축의 실좌표에서 도형 안의 중심축 값을 빼면 도형이 생각하는 원점 좌표가 나옴.
                this.OriginX = newCenter.X - LineRotateTransform.CenterX;
                this.OriginY = newCenter.Y - LineRotateTransform.CenterY;

            }
            //선의 끝 점이 중심축이었고 이제 중심축을 되돌릴 경우
            else if (LineRotateTransform.CenterX != 0 && controlName.Contains("Start"))
            {
                //새로 중심축이 될 좌표를 현재 중심축에서 로테이션한 좌표로 변환
                var newCenter = this.GetPointByRotation(new Point(-this.Width, 0), m_Radian, new Point()) - new Point(-(this.OriginX + this.Width), -(this.OriginY + m_LineThickness / 2));

                //중심 축 이동
                LineRotateTransform = new RotateTransform(Rotation, 0, m_LineThickness / 2);
                RaisePropertyChanged(nameof(LineRotateTransform));

                //변환한 중심축의 실좌표에서 도형 안의 중심축 값을 빼면 도형이 생각하는 원점 좌표가 나옴.
                this.OriginX = newCenter.X - LineRotateTransform.CenterX;
                this.OriginY = newCenter.Y - LineRotateTransform.CenterY;
            }
        }

        /// <summary>
        /// 목표점을 중심점 기준으로 회전이동변환한 점을 반환합니다. 
        /// </summary>
        /// <param name="rotatePoint">변환시킬 목표점.</param>
        /// <param name="rad">Radian 값 각도.</param>
        /// <param name="center">회전의 중심점.</param>
        /// <returns></returns>
        private Point GetPointByRotation(Point rotatePoint, double rad, Point center)
        {
            return new Point(Math.Cos(rad) * rotatePoint.X - Math.Sin(rad) * rotatePoint.Y - center.X, Math.Sin(rad) * rotatePoint.X + Math.Cos(rad) * rotatePoint.Y - center.Y);
        }
        #endregion

        #region Events
        private void LineSearcher_Loaded(object sender, RoutedEventArgs e)
        {
            //컨트롤의 기본값 설정
            CaliperCount = 3;
            OriginX = 20;
            OriginY = 20;
            ProjectionLength = 30;
            SearchLength = 100;
            m_LineThickness = this.MinHeight;
            Radian = 0;
        }

        private void Line_MouseLeave(object sender, MouseEventArgs e)
        {
            //마우스 모양 원래대로 돌리기
            if (sender is FrameworkElement control)
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void Line_MouseEnter(object sender, MouseEventArgs e)
        {
            //위치에 맞는 마우스 모양 변경
            if (sender is FrameworkElement control)
            {
                switch (control.Name)
                {
                    case "Point_Start":
                    case "Point_End":
                        this.Cursor = Cursors.Cross;
                        break;
                    case "Segment":
                        this.Cursor = Cursors.SizeAll;
                        break;
                }
            }
        }
        private void Line_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //마우스 캡쳐가 가능한 컨트롤이면서 부모로 캔버스를 지니는 지 확인
            if (sender is IInputElement element && this.Parent is Canvas canvas)
            {
                //마우스 고정 (이후로 마우스이벤트는 이 컨트롤에 관한 것만 발생)
                element.CaptureMouse();
                m_IsCaptured = true;
                //점 위치 설정
                m_LastMovePoint = e.GetPosition(canvas);

                //컨트롤 이름 가져와서 중심축 체크
                if (sender is FrameworkElement control) this.MoveRotateOrigin(control.Name);

                //상위에 연결된 다른 MouseDown 실행시키지 않음
                e.Handled = true;
            }
        }

        private void Line_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lock (m_MoveLock)
            {
                //캡쳐 해제
                Mouse.Capture(null);
                m_IsCaptured = false;
                //위치초기화
                m_LastMovePoint = new Point();

                //컨트롤 이름 가져와서 중심축 체크
                if (sender is FrameworkElement control) this.MoveRotateOrigin(control.Name);

                //캘리퍼 업데이트
                this.UpdateCaliper();
                
                //상위에 연결된 다른 MouseUp 실행시키지 않음
                e.Handled = true;
            }
        }
        private void Line_MouseMove(object sender, MouseEventArgs e)
        {
            //MouseUp 이벤트가 끝나기도 전에 발생하지 않도록 임계구역 설정
            lock (m_MoveLock)
            {
                if (m_IsCaptured)
                {
                    //본인이 컨트롤이고 부모로 캔버스를 가지고 있는지 확인
                    if (sender is FrameworkElement control && this.Parent is Canvas canvas)
                    {
                        //다른 MouseMove 실행시키지 않음
                        e.Handled = true;

                        //이름 확인
                        switch (control.Name)
                        {
                            //시작점일 경우
                            case "Point_Start":
                                //마우스 위치 증감에 따라 반대로 움직여야 함
                                Radian = Math.PI + Math.Atan2(e.GetPosition(canvas).Y - (LineRotateTransform.CenterY + this.OriginY), e.GetPosition(canvas).X - (LineRotateTransform.CenterX + this.OriginX));

                                //실측 길이와 실제 너비의 차이
                                var offset = Math.Sqrt(Math.Pow(e.GetPosition(canvas).X - (LineRotateTransform.CenterX + this.OriginX), 2) + Math.Pow(e.GetPosition(canvas).Y - (LineRotateTransform.CenterY + this.OriginY), 2)) - this.Width;

                                //너비 변화량만큼 원점에선 빼주고 기존 너비에선 더해줌
                                this.OriginX -= offset;
                                this.Width += offset;

                                //회전 및 중심축 재설정                                
                                LineRotateTransform = new RotateTransform(Rotation, this.Width, m_LineThickness / 2);
                                RaisePropertyChanged(nameof(LineRotateTransform));
                                break;
                            //끝점일 경우
                            case "Point_End":
                                Radian = Math.Atan2(e.GetPosition(canvas).Y - (LineRotateTransform.CenterY + this.OriginY), e.GetPosition(canvas).X - (LineRotateTransform.CenterX + this.OriginX));

                                this.Width = Math.Sqrt(Math.Pow(e.GetPosition(canvas).X - (LineRotateTransform.CenterX + this.OriginX), 2) + Math.Pow(e.GetPosition(canvas).Y - (LineRotateTransform.CenterY + this.OriginY), 2));

                                //회전 재설정
                                LineRotateTransform = new RotateTransform(Rotation, 0, m_LineThickness / 2);
                                RaisePropertyChanged(nameof(LineRotateTransform));
                                break;
                            //선분일 경우
                            case "Segment":
                                //위치 변화량 구하기
                                Vector moveOffset = e.GetPosition(canvas) - m_LastMovePoint;
                                //원점에 변화량 더해줌
                                this.OriginX += moveOffset.X;
                                this.OriginY += moveOffset.Y;
                                //다음 위치 정보를 현재 위치로 변경
                                m_LastMovePoint = e.GetPosition(canvas);
                                break;
                        }
                    }
                }
            }
        }

        //외부에서 Width 변경 시 호출하도록 설정
        private void ContentControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!m_IsCaptured)
            {
                this.UpdateCaliper();
                var interval = this.Width / PoseCollection.Count;
                for (int i = 0; i < PoseCollection.Count; i++) PoseCollection[i].TranslateX = (i + 0.5) * interval;
            }
        }

        //외부에서 Search Length, Projection Length 변경 시 호출하도록 설정
        private void Edge_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var xMargin = -ProjectionLength / 2;
            var yMargin = -SearchLength / 2;
            (sender as EdgeSettingGraphic).Margin = new Thickness(xMargin, yMargin, xMargin, yMargin);
            if (this.Width != double.NaN || this.Width != 0) this.UpdateCaliper();
        }
        #endregion

    }
}

