using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace CvsVision.Caliper
{
    /// <summary>
    /// 이미지를 통해 선을 찾을 수 있는 도구 클래스입니다.
    /// </summary>
    public class CvsLineDetectTool : ICvsTool
    {
        #region Fields
        private Bitmap m_InputImage;
        private CvsLineDetect m_LineDetect;
        private CvsLineSetting m_Setting;
        #endregion

        #region Properties
        /// <summary>
        /// 입력 이미지를 가져오거나 설정합니다.
        /// </summary>
        public Bitmap InputImage
        {
            get { return m_InputImage; }
            set
            {
                m_InputImage = value;
                Overlay = null;
            }
        }
        /// <summary>
        /// 선 검색을 위한 설정 값을 가져오거나 설정합니다.
        /// </summary>
        public CvsLineSetting Setting
        {
            get { return m_Setting; }
            set
            {
                m_Setting = value;
                m_LineDetect = m_Setting.GetToolParams();
            }
        }
        /// <summary>
        /// 결과 선을 가져옵니다.
        /// </summary>
        public CvsLine Line
        {
            get
            {
                if (m_LineDetect != null) return m_LineDetect.Line;
                else return null;
            }
        }
        /// <summary>
        /// 결과 오버레이를 가져옵니다.
        /// </summary>
        public DrawingGroup Overlay { get; private set; }

        /// <summary>
        /// 해당 도구 사용 시 발생하는 예외를 가져옵니다. 
        /// (Null 값 일 경우, 정상적으로 동작한 것입니다.)
        /// </summary>
        public Exception Exception { get; private set; }
        #endregion

        /// <summary>
        /// 선을 찾는 도구 클래스를 생성합니다.
        /// </summary>
        public CvsLineDetectTool()
        {
            Setting = new CvsLineSetting();
        }
        
        public void Dispose()
        {
            if (InputImage != null) InputImage.Dispose();
        }

        #region Methods
        /// <summary>
        /// 파일 형태로 저장된 설정 값들을 불러옵니다.
        /// </summary>
        /// <param name="filePath">저장된 설정 파일 경로.</param>
        /// <param name="toolType">불러오는 도구의 타입.</param>
        public void Load(string filePath, Type toolType)
        {
            try
            {

                Exception = null;
            }
            catch(Exception err)
            {
                Exception = err;
            }
        }

        /// <summary>
        /// 현재 설정 값들을 파일 형태로 저장합니다.
        /// </summary>
        /// <param name="path">저장할 파일 경로.</param>
        public void Save(string path)
        {
            try
            {

                Exception = null;
            }
            catch (Exception err)
            {
                Exception = err;
            }
        }

        /// <summary>
        /// 선 검색을 시작합니다.
        /// </summary>
        public void Run()
        {
            try
            {
                if (m_LineDetect.InputPointList != null) m_LineDetect.InputPointList.Clear();
                else m_LineDetect.InputPointList = new List<Point>();

                foreach(var edge in Setting.EdgeCollection)
                {
                    //주어진 설정 값으로 각 이미지 자르고
                    var cropImage = edge.Region.Crop(m_InputImage);
                    var edgeDetect = edge.GetToolParams();
                    edgeDetect.DetectImage = cropImage;
                    //에지 찾기
                    edgeDetect.Detect();
                    //에지 있으면 선 찾기 위한 점 집합에 추가,
                    if(edgeDetect.Edge != null) m_LineDetect.InputPointList.Add(edge.Region.Pose.GetPointByOrigin(edgeDetect.Edge.X, edgeDetect.Edge.Y));
                }
                //선 찾기
                m_LineDetect.Detect();

                //결과 없으면 예외
                if (Line == null) throw new Exception("Line not found.");

                //결과 오버레이 생성
                Overlay = this.CreateGeometry();

                Exception = null;
            }
            catch (Exception err)
            {
                Overlay = null;
                Exception = err;
            }
        }
        
        /// <summary>
        /// 결과 그래픽을 생성합니다.
        /// </summary>
        /// <returns></returns>
        private DrawingGroup CreateGeometry()
        {
            DrawingGroup dg = new DrawingGroup();
            GeometryDrawing overlay = new GeometryDrawing
            {
                Geometry = new RectangleGeometry(new Rect(0, 0, m_InputImage.Width, m_InputImage.Height)),
                Brush = Brushes.Transparent,
                Pen = new Pen(Brushes.Transparent, 0)
            };
            dg.Children.Add(overlay);

            GeometryDrawing graphic = new GeometryDrawing();
            GeometryGroup group = new GeometryGroup();
            
            foreach (var point in m_LineDetect.InputPointList)
            {
                group.Children.Add(new LineGeometry(new Point(point.X - 5, point.Y + 5), new Point(point.X + 5, point.Y - 5)));
                group.Children.Add(new LineGeometry(new Point(point.X - 5, point.Y - 5), new Point(point.X + 5, point.Y + 5)));
            }
            
            Point p0, p1;

            //x = c 인 직선
            if (Line.Gradient == double.NaN)
            {
                p0 = new Point(Line.Y_Intercept, 0);
                p1 = new Point(Line.Y_Intercept, m_InputImage.Height);
            }
            //y = c 인 직선
            else if (Line.Gradient == 0)
            {
                p0 = new Point(0, Line.Y_Intercept);
                p1 = new Point(m_InputImage.Width, Line.Y_Intercept);
            }
            else
            {
                var p0Location = this.GetLinePointLocation();
                var p1Location = this.GetLinePointLocation(p0Location);

                p0 = this.GetPointFromLocation(p0Location);
                p1 = this.GetPointFromLocation(p1Location);
            }
            group.Children.Add(new LineGeometry(p0, p1));

            graphic.Geometry = group;
            graphic.Brush = Brushes.Transparent;
            graphic.Pen = new Pen(Brushes.LawnGreen, 1);
            graphic.Freeze();

            dg.Children.Add(graphic);

            return dg;
        }
        /// <summary>
        /// 직선 모양에 따른 위치 번호 부여하기.
        /// </summary>
        /// <param name="passLocation">통과할 위치(시작점이 접한 위치).</param>
        /// <returns></returns>
        private int GetLinePointLocation(int passLocation = 0)
        {
            //x = 0, y = 0 일 경우
            if (Line.Y_Intercept == 0 && passLocation != 1)
            {
                return 1;
            }
            //0 < x < w,  y = 0 일 경우
            else if (-Line.Y_Intercept / Line.Gradient > 0 && -Line.Y_Intercept / Line.Gradient < m_InputImage.Width && passLocation != 2)
            {
                return 2;
            }
            //x = w, y = 0 일 경우
            else if (Line.Gradient * m_InputImage.Width + Line.Y_Intercept == 0 && passLocation != 3)
            {
                return 3;
            }
            //x = w, 0 < y < h 일 경우
            else if (Line.Gradient * m_InputImage.Width + Line.Y_Intercept > 0 && Line.Gradient * m_InputImage.Width + Line.Y_Intercept < m_InputImage.Height && passLocation != 4)
            {
                return 4;
            }
            //x = w, y = h 일 경우
            else if (Line.Gradient * m_InputImage.Width + Line.Y_Intercept == m_InputImage.Height && passLocation != 5)
            {
                return 5;
            }
            //0 < x < w, y = h 일 경우
            else if ((m_InputImage.Height - Line.Y_Intercept) / Line.Gradient > 0 && (m_InputImage.Height - Line.Y_Intercept) / Line.Gradient < m_InputImage.Width && passLocation != 6)
            {
                return 6;
            }
            //x = 0, y = h 일 경우
            else if (Line.Y_Intercept == m_InputImage.Height && passLocation != 7)
            {
                return 7;
            }
            //x = 0, 0 < y < h 일 경우
            else if (Line.Y_Intercept > 0 && Line.Y_Intercept < m_InputImage.Height && passLocation != 8)
            {
                return 8;
            }
            else throw new Exception("Wrong edge coordinate.");
        }
        /// <summary>
        /// 위치번호에 따른 선 끝 점 구하기.
        /// </summary>
        /// <param name="location">위치 번호.</param>
        /// <returns></returns>
        private Point GetPointFromLocation(int location)
        {
            switch (location)
            {
                case 1:
                    return new Point();
                case 2:
                    return new Point(-Line.Y_Intercept / Line.Gradient, 0);
                case 3:
                    return new Point(m_InputImage.Width, 0);
                case 4:
                    return new Point(m_InputImage.Width, m_InputImage.Width * Line.Gradient + Line.Y_Intercept);
                case 5:
                    return new Point(m_InputImage.Width, m_InputImage.Height);
                case 6:
                    return new Point((m_InputImage.Height - Line.Y_Intercept) / Line.Gradient, m_InputImage.Height);
                case 7:
                    return new Point(0, m_InputImage.Height);
                case 8:
                    return new Point(0, Line.Y_Intercept);
                default:
                    throw new Exception("Invalid location number.");
            }
        }
        #endregion

    }
}
