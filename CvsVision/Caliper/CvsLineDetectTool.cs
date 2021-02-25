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
    public class CvsLineDetectTool : ICvsTool
    {
        #region Fields
        private Bitmap m_InputImage;
        private CvsLineDetect m_LineDetect;
        private CvsLineSetting m_Setting;
        #endregion

        #region Properties
        public Bitmap InputImage
        {
            get { return m_InputImage; }
            set
            {
                m_InputImage = value;
                Overlay = null;
            }
        }
        public CvsLineSetting Setting
        {
            get { return m_Setting; }
            set
            {
                m_Setting = value;
                m_LineDetect = m_Setting.GetToolParams();
            }
        }
        public CvsLine Line
        {
            get
            {
                if (m_LineDetect != null) return m_LineDetect.Line;
                else return null;
            }
        }
        public DrawingGroup Overlay { get; private set; }

        public Exception Exception { get; private set; }
        #endregion

        public CvsLineDetectTool()
        {
            Setting = new CvsLineSetting();
        }



        public void Dispose()
        {
            if (InputImage != null) InputImage.Dispose();
        }


        #region Methods

        public void Load(string filePath, Type toolType)
        {
            try
            {

            }
            catch(Exception err)
            {
                Exception = err;
            }
        }
        public void Save(string path)
        {
            try
            {

            }
            catch (Exception err)
            {
                Exception = err;
            }
        }

        public void Run()
        {
            try
            {
                if (m_LineDetect.InputPointList != null) m_LineDetect.InputPointList.Clear();
                else m_LineDetect.InputPointList = new List<Point>();

                foreach(var edge in Setting.EdgeCollection)
                {
                    var cropImage = edge.Region.Crop(m_InputImage);
                    var edgeDetect = edge.GetToolParams();
                    edgeDetect.DetectImage = cropImage;
                    edgeDetect.Detect();
                    if(edgeDetect.Edge != null) m_LineDetect.InputPointList.Add(edge.Region.Pose.GetPointByOrigin(edgeDetect.Edge.X, edgeDetect.Edge.Y));
                }
                m_LineDetect.Detect();

                if (Line == null) throw new Exception("Line not found.");

                Overlay = this.CreateGeometry();
            }
            catch (Exception err)
            {
                Exception = err;
            }
        }


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
                p0 = new Point(Line.StartPoint.X, 0);
                p1 = new Point(Line.StartPoint.X, m_InputImage.Height);
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
