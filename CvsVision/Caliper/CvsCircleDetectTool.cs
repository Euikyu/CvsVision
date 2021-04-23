using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace CvsVision.Caliper
{
    public class CvsCircleDetectTool : ICvsTool
    {
        #region Fields
        private Bitmap m_InputImage;
        private CvsCircleDetect m_CircleDetect;
        private CvsCircleSetting m_Setting;
        private CvsEdgeSettingCollection m_Collection;
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

        public CvsCircleSetting Setting
        {
            get { return m_Setting; }

            set
            {
                m_Setting = value;
                if(value != null)
                {
                    m_CircleDetect = m_Setting.GetToolParams();
                    m_Collection.SetParentPose(m_Setting.CirclePose);
                    m_Collection.SetWholeEdgeSetting(m_Setting.GetCaliperSettings());
                }
            }
        }
        public CvsCircle Circle
        {
            get
            {
                if (m_CircleDetect != null) return m_CircleDetect.Circle;
                else return null;
            }
        }
        public DrawingGroup Overlay { get; private set; }

        public Exception Exception { get; private set; }
        #endregion

        public CvsCircleDetectTool()
        {
            m_Collection = new CvsEdgeSettingCollection();
            m_Setting = new CvsCircleSetting
            {
                ConsensusThreshold = 6,
                OriginX = 20,
                OriginY = 20,
                Radius = 100,
                StartAngle = 180,
                EndAngle = 0,
                CaliperCount = 4,
                ProjectionLength = 30,
                SearchLength = 100,
                ContrastThreshold = 5,
                HalfPixelCount = 2
            };

            m_CircleDetect = m_Setting.GetToolParams();
            m_Collection.SetWholeEdgeSetting(m_Setting.GetCaliperSettings());
            m_Collection.SetParentPose(m_Setting.CirclePose);
        }

        public void Dispose()
        {
            if (InputImage != null) InputImage.Dispose();
        }


        #region Methods
        public void Load(string path)
        {
            try
            {
                if (!File.Exists(path)) throw new Exception("Not found file.");
                XmlSerializer xml = new XmlSerializer(typeof(CvsCircleSetting));

                using (var sr = new StreamReader(path))
                {
                    try
                    {
                        var newSetting = xml.Deserialize(sr) as CvsCircleSetting;
                        Setting = newSetting ?? throw new Exception();
                    }
                    catch
                    {
                        throw new Exception("Different tool type.");
                    }
                }

                Exception = null;
            }
            catch (Exception err)
            {
                Exception = err;
            }
        }
        public void Save(string path)
        {
            try
            {
                using (var sw = new StreamWriter(path))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(CvsCircleSetting));
                    xml.Serialize(sw, Setting);
                }

                Exception = null;
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
                if (InputImage == null) throw new Exception("Input image first.");

                if (m_CircleDetect.InputPointList != null) m_CircleDetect.InputPointList.Clear();
                else m_CircleDetect.InputPointList = new List<Point>();

                var interval = ((Setting.EndAngle - Setting.StartAngle + 360) % 360) / (Setting.CaliperCount - 1);
                m_Collection.Clear();

                for (int i = 0; i < Setting.CaliperCount; i++)
                {
                    var edge = new CvsEdgeSetting();
                    var currentRadian = ((Setting.StartAngle + i * interval) % 360) * Math.PI / 180;
                    var v = this.GetPointByRotation(new Point(Setting.Radius, 0), currentRadian, new Point()) - new Point(-m_Setting.Radius, -m_Setting.Radius);
                    edge.Region.Pose = new CvsPose(currentRadian + (m_Setting.IsOutwardDirection ? -Math.PI / 2 : Math.PI / 2), v.X, v.Y);
                    m_Collection.Add(edge);
                }

                foreach (var edge in m_Collection)
                {
                    //주어진 설정 값으로 각 이미지 자르고
                    var cropImage = edge.Region.Crop(m_InputImage);
                    var edgeDetect = edge.GetToolParams();
                    edgeDetect.DetectImage = cropImage;
                    //에지 찾기
                    edgeDetect.Detect();
                    //에지 있으면 선 찾기 위한 점 집합에 추가,
                    if (edgeDetect.Edge != null) m_CircleDetect.InputPointList.Add(edge.Region.Pose.GetPointByOrigin(edgeDetect.Edge.X, edgeDetect.Edge.Y));
                }
                //선 찾기
                m_CircleDetect.Detect();

                //결과 없으면 예외
                if (Circle == null) throw new Exception("Circle not found.");

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

            foreach (var point in m_CircleDetect.InputPointList)
            {
                group.Children.Add(new LineGeometry(new Point(point.X - 5, point.Y + 5), new Point(point.X + 5, point.Y - 5)));
                group.Children.Add(new LineGeometry(new Point(point.X - 5, point.Y - 5), new Point(point.X + 5, point.Y + 5)));
            }

            group.Children.Add(new EllipseGeometry(this.Circle.Center, this.Circle.Radius, this.Circle.Radius));
            
            graphic.Geometry = group;
            graphic.Brush = Brushes.Transparent;
            graphic.Pen = new Pen(Brushes.LawnGreen, 1);
            graphic.Freeze();

            dg.Children.Add(graphic);

            dg.Freeze();

            return dg;
        }
        #endregion
    }
}
