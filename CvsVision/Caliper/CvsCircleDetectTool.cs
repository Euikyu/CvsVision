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
    /// <summary>
    /// 이미지를 통해 원을 찾을 수 있는 도구 클래스입니다.
    /// </summary>
    public class CvsCircleDetectTool : ICvsTool
    {
        #region Fields
        private Bitmap m_InputImage;
        private CvsCircleDetect m_CircleDetect;
        private CvsCircleSetting m_Setting;
        private CvsEdgeSettingCollection m_Collection;
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
        /// 원 검색을 위한 설정 값을 가져오거나 설정합니다.
        /// </summary>
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
        /// <summary>
        /// 결과 원을 가져옵니다.
        /// </summary>
        public CvsCircle Circle
        {
            get
            {
                if (m_CircleDetect != null) return m_CircleDetect.Circle;
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
        /// 원을 찾는 도구 클래스를 생성합니다.
        /// </summary>
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

#pragma warning disable CS1591 // 공개된 형식 또는 멤버에 대한 XML 주석이 없습니다.
        public void Dispose()
#pragma warning restore CS1591 // 공개된 형식 또는 멤버에 대한 XML 주석이 없습니다.
        {
            if (InputImage != null) InputImage.Dispose();
        }


        #region Methods
        /// <summary>
        /// 파일 형태로 저장된 설정 값들을 불러옵니다.
        /// </summary>
        /// <param name="path">저장된 설정 파일 경로.</param>
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

        /// <summary>
        /// 현재 설정 값들을 파일 형태로 저장합니다.
        /// </summary>
        /// <param name="path">저장할 파일 경로.</param>
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

        /// <summary>
        /// 원 검색을 시작합니다.
        /// </summary>
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
            if (this.Circle.Center.X + this.Circle.Radius <= m_InputImage.Width && this.Circle.Center.X - this.Circle.Radius >= 0 &&
                this.Circle.Center.Y + this.Circle.Radius <= m_InputImage.Height && this.Circle.Center.Y - this.Circle.Radius >= 0)
            {
                group.Children.Add(new EllipseGeometry(this.Circle.Center, this.Circle.Radius, this.Circle.Radius));
            }
            else
            {
                var geometry = new PathGeometry();
                geometry.Figures = this.GetFigures();

                group.Children.Add(geometry);
            }
            graphic.Geometry = group;
            graphic.Brush = Brushes.Transparent;
            graphic.Pen = new Pen(Brushes.LawnGreen, 1);
            graphic.Freeze();

            dg.Children.Add(graphic);

            dg.Freeze();

            return dg;
        }

        private PathFigureCollection GetFigures()
        {
            List<PathFigure> figures = new List<PathFigure>();

            // 원 중심 (Xc, Yc)이고 반지름 r을 가지는
            // 원의 방정식 : (x - Xc)^2 + (y - Yc)^2 = r^2
            // y = Yc ± Math.Sqrt(Math.Pow(r,2) - Math.Pow(x - Xc, 2)); 
            // x = Xc ± Math.Sqrt(Math.Pow(r,2) - Math.Pow(y - Yc, 2));
            double centerX = Circle.Center.X;
            double centerY = Circle.Center.Y;
            double r = Circle.Radius;

            double w = m_InputImage.Width;
            double h = m_InputImage.Height;

            double distX = Math.Abs(centerY);
            double distY = Math.Abs(centerX);
            double distW = Math.Abs(centerX - w);
            double distH = Math.Abs(centerY - h);

            var size = new System.Windows.Size(r, r);
            int location = (distX < r ? 1 : 0) + (distY < r ? 2 : 0) + (distH < r ? 4 : 0) + (distW < r ? 8 : 0);

            double tmpXVal = Math.Pow(r, 2) >= Math.Pow(centerY, 2) ? Math.Sqrt(Math.Pow(r, 2) - Math.Pow(centerY, 2)) : 0;
            double tmpYVal = Math.Pow(r, 2) >= Math.Pow(centerX, 2) ? Math.Sqrt(Math.Pow(r, 2) - Math.Pow(centerX, 2)) : 0;
            double tmpWVal = Math.Pow(r, 2) >= Math.Pow(centerX - w, 2) ? Math.Sqrt(Math.Pow(r, 2) - Math.Pow(w - centerX, 2)) : 0;
            double tmpHVal = Math.Pow(r, 2) >= Math.Pow(centerY - h, 2) ? Math.Sqrt(Math.Pow(r, 2) - Math.Pow(h - centerY, 2)) : 0;

            switch (location)
            {
                // 원이 이미지 안에 온전히 있거나 완전히 벗어난 경우,
                case 0:
                    break;

                // 원이 y = 0 인 직선에만 두 점에 걸린 경우,
                case 1:
                    //원이 이미지 밖을 벗어난 경우,
                    if (centerX < -r || centerX > w + r) break;
                    var loc1_p0 = new Point(centerX - tmpXVal, 0);
                    var loc1_p1 = new Point(centerX + tmpXVal, 0);
                    bool loc1_isLargeArc = centerY > 0 ? true : false;
                    
                    figures.Add(new PathFigure(loc1_p1, new ArcSegment[] { new ArcSegment(loc1_p0, size, 0, loc1_isLargeArc, SweepDirection.Clockwise, true) }, false));
                    break;

                // 원이 x = 0 인 직선에만 두 점에 걸린 경우,
                case 2:
                    //원이 이미지 밖을 벗어난 경우,
                    if (centerY < -r || centerY > h + r) break;
                    var loc2_p0 = new Point(0, centerY - tmpYVal);
                    var loc2_p1 = new Point(0, centerY + tmpYVal);
                    var loc2_isLargeArc = centerX > 0 ? true : false;
                    
                    figures.Add(new PathFigure(loc2_p0, new ArcSegment[] { new ArcSegment(loc2_p1, size, 0, loc2_isLargeArc, SweepDirection.Clockwise, true) }, false));
                    break;

                // 원이 y = h 인 직선에만 두 점에 걸린 경우,
                case 4:
                    //원이 이미지 밖을 벗어난 경우,
                    if (centerX < -r || centerX > w + r) break;
                    var loc4_p0 = new Point(centerX - tmpHVal, h);
                    var loc4_p1 = new Point(centerX + tmpHVal, h);
                    bool loc4_isLargeArc = centerY < h ? true : false;

                    figures.Add(new PathFigure(loc4_p0, new ArcSegment[] { new ArcSegment(loc4_p1, size, 0, loc4_isLargeArc, SweepDirection.Clockwise, true) }, false));
                    break;

                // 원이 x = w 인 직선에만 두 점에 걸린 경우,
                case 8:
                    //원이 이미지 밖을 벗어난 경우,
                    if (centerY < -r || centerY > h + r) break;
                    var loc8_p0 = new Point(w, centerY - tmpWVal);
                    var loc8_p1 = new Point(w, centerY + tmpWVal);
                    bool loc8_isLargeArc = centerX < w ? true : false;

                    figures.Add(new PathFigure(loc8_p1, new ArcSegment[] { new ArcSegment(loc8_p0, size, 0, loc8_isLargeArc, SweepDirection.Clockwise, true) }, false));
                    break;

                // 원이 y = 0 인 직선과 x = 0 인 직선에 각각 두 점에 걸린 경우,
                case 3:
                    //원이 이미지 밖을 벗어난 경우,
                    if (centerX < 0 && centerY < 0 && Math.Sqrt(Math.Pow(centerX, 2) + Math.Pow(centerY, 2)) > r) break;
                    var loc3_p0 = new Point(centerX - tmpXVal, 0);
                    var loc3_p1 = new Point(centerX + tmpXVal, 0);
                    var loc3_p2 = new Point(0, centerY - tmpYVal);
                    var loc3_p3 = new Point(0, centerY + tmpYVal);

                    if (loc3_p0.X > 0 && loc3_p2.Y > 0)
                    {
                        figures.Add(new PathFigure(loc3_p2, new ArcSegment[] { new ArcSegment(loc3_p0, size, 0, false, SweepDirection.Clockwise, true) }, false));
                        figures.Add(new PathFigure(loc3_p1, new ArcSegment[] { new ArcSegment(loc3_p3, size, 0, true, SweepDirection.Clockwise, true) }, false));
                    }
                    else if(loc3_p0.X > 0)
                    {
                        figures.Add(new PathFigure(loc3_p1, new ArcSegment[] { new ArcSegment(loc3_p0, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    else if(loc3_p2.Y > 0)
                    {
                        figures.Add(new PathFigure(loc3_p2, new ArcSegment[] { new ArcSegment(loc3_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    else
                    {
                        figures.Add(new PathFigure(loc3_p1, new ArcSegment[] { new ArcSegment(loc3_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    break;

                // 원이 y = 0 인 직선과 y = h 인 직선에 각각 두 점에 걸린 경우,
                case 5:
                    //원이 이미지 밖을 벗어난 경우,
                    if (centerX < -r || centerX > w + r) break;
                    var loc5_p0 = new Point(centerX - tmpXVal, 0);
                    var loc5_p1 = new Point(centerX + tmpXVal, 0);
                    var loc5_p2 = new Point(centerX - tmpHVal, h);
                    var loc5_p3 = new Point(centerX + tmpHVal, h);
                    
                    figures.Add(new PathFigure(loc5_p1, new ArcSegment[] { new ArcSegment(loc5_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    figures.Add(new PathFigure(loc5_p2, new ArcSegment[] { new ArcSegment(loc5_p0, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    break;

                // 원이 y = 0 인 직선과 x = w 인 직선에 각각 두 점에 걸린 경우,
                case 9:
                    //원이 이미지 밖을 벗어난 경우,
                    if (centerX > w && centerY < 0 && Math.Sqrt(Math.Pow(centerX - w, 2) + Math.Pow(centerY, 2)) > r) break;

                    var loc9_p0 = new Point(centerX - tmpXVal, 0);
                    var loc9_p1 = new Point(centerX + tmpXVal, 0);
                    var loc9_p2 = new Point(w, centerY - tmpWVal);
                    var loc9_p3 = new Point(w, centerY + tmpWVal);

                    if (loc9_p1.X < w && loc9_p2.Y > 0)
                    {
                        figures.Add(new PathFigure(loc9_p1, new ArcSegment[] { new ArcSegment(loc9_p2, size, 0, false, SweepDirection.Clockwise, true) }, false));
                        figures.Add(new PathFigure(loc9_p3, new ArcSegment[] { new ArcSegment(loc9_p0, size, 0, true, SweepDirection.Clockwise, true) }, false));
                    }
                    else if (loc9_p1.X < w)
                    {
                        figures.Add(new PathFigure(loc9_p1, new ArcSegment[] { new ArcSegment(loc9_p0, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    else if (loc9_p2.Y > 0)
                    {
                        figures.Add(new PathFigure(loc9_p3, new ArcSegment[] { new ArcSegment(loc9_p2, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    else
                    {
                        figures.Add(new PathFigure(loc9_p3, new ArcSegment[] { new ArcSegment(loc9_p0, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    break;

                // 원이 x = 0 인 직선과 y = h 인 직선에 각각 두 점에 걸린 경우,
                case 6:
                    //원이 이미지 밖을 벗어난 경우,
                    if (centerX < 0  && centerY > h && Math.Sqrt(Math.Pow(centerX, 2) + Math.Pow(centerY - h, 2)) > r) break;

                    var loc6_p0 = new Point(0, centerY - tmpYVal);
                    var loc6_p1 = new Point(0, centerY + tmpYVal);
                    var loc6_p2 = new Point(centerX - tmpHVal, h);
                    var loc6_p3 = new Point(centerX + tmpHVal, h);

                    if (loc6_p0.X > 0 && loc6_p3.Y < h)
                    {
                        figures.Add(new PathFigure(loc6_p2, new ArcSegment[] { new ArcSegment(loc6_p1, size, 0, false, SweepDirection.Clockwise, true) }, false));
                        figures.Add(new PathFigure(loc6_p0, new ArcSegment[] { new ArcSegment(loc6_p3, size, 0, true, SweepDirection.Clockwise, true) }, false));
                    }
                    else if (loc6_p0.X > 0)
                    {
                        figures.Add(new PathFigure(loc6_p1, new ArcSegment[] { new ArcSegment(loc6_p0, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    else if (loc6_p3.Y < h)
                    {
                        figures.Add(new PathFigure(loc6_p2, new ArcSegment[] { new ArcSegment(loc6_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    else
                    {
                        figures.Add(new PathFigure(loc6_p0, new ArcSegment[] { new ArcSegment(loc6_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    break;

                // 원이 x = 0 인 직선과 x = w 인 직선에 각각 두 점에 걸린 경우,
                case 10:
                    //원이 이미지 밖을 벗어난 경우,
                    if (centerY < -r || centerY > h + r) break;
                    var loc10_p0 = new Point(0, centerY - tmpYVal);
                    var loc10_p1 = new Point(0, centerY + tmpYVal);
                    var loc10_p2 = new Point(w, centerY - tmpWVal);
                    var loc10_p3 = new Point(w, centerY + tmpWVal);
                    
                    figures.Add(new PathFigure(loc10_p0, new ArcSegment[] { new ArcSegment(loc10_p2, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    figures.Add(new PathFigure(loc10_p3, new ArcSegment[] { new ArcSegment(loc10_p1, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    break;

                // 원이 y = h 인 직선과 x = w 인 직선에 각각 두 점에 걸린 경우,
                case 12:
                    //원이 이미지 밖을 벗어난 경우,
                    if (centerX > w && centerY > h && Math.Sqrt(Math.Pow(centerX - w, 2) + Math.Pow(centerY - h, 2)) > r) break;

                    var loc12_p0 = new Point(centerX - tmpHVal, h);
                    var loc12_p1 = new Point(centerX + tmpHVal, h);
                    var loc12_p2 = new Point(w, centerY - tmpWVal);
                    var loc12_p3 = new Point(w, centerY + tmpWVal);
                    
                    if (loc12_p1.X < w && loc12_p3.Y < h)
                    {
                        figures.Add(new PathFigure(loc12_p3, new ArcSegment[] { new ArcSegment(loc12_p1, size, 0, false, SweepDirection.Clockwise, true) }, false));
                        figures.Add(new PathFigure(loc12_p0, new ArcSegment[] { new ArcSegment(loc12_p2, size, 0, true, SweepDirection.Clockwise, true) }, false));
                    }
                    else if (loc12_p1.X < w)
                    {
                        figures.Add(new PathFigure(loc12_p0, new ArcSegment[] { new ArcSegment(loc12_p1, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    else if (loc12_p3.Y < h)
                    {
                        figures.Add(new PathFigure(loc12_p3, new ArcSegment[] { new ArcSegment(loc12_p2, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    else
                    {
                        figures.Add(new PathFigure(loc12_p0, new ArcSegment[] { new ArcSegment(loc12_p2, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    break;

                // 원이 y = 0 인 직선과 x = 0 인 직선, y = h 인 직선에 각각 두 점에 걸린 경우,
                case 7:
                    var loc7_p0 = new Point(centerX - tmpXVal, 0);
                    var loc7_p1 = new Point(centerX + tmpXVal, 0);
                    var loc7_p2 = new Point(0, centerY - tmpYVal);
                    var loc7_p3 = new Point(0, centerY + tmpYVal);
                    var loc7_p4 = new Point(centerX - tmpHVal, h);
                    var loc7_p5 = new Point(centerX + tmpHVal, h);

                    if (loc7_p1.X > 0)
                    {
                        if (loc7_p5.X < 0)
                        {
                            figures.Add(new PathFigure(loc7_p1, new ArcSegment[] { new ArcSegment(loc7_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                        }
                        else
                        {
                            figures.Add(new PathFigure(loc7_p1, new ArcSegment[] { new ArcSegment(loc7_p5, size, 0, false, SweepDirection.Clockwise, true) }, false));
                            if (loc7_p0.X > 0)
                            {
                                figures.Add(new PathFigure(loc7_p2, new ArcSegment[] { new ArcSegment(loc7_p0, size, 0, false, SweepDirection.Clockwise, true) }, false));
                            }
                            if (loc7_p4.X > 0)
                            {
                                figures.Add(new PathFigure(loc7_p4, new ArcSegment[] { new ArcSegment(loc7_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                            }
                        }
                    }
                    else if(loc7_p5.X > 0)
                    {
                        figures.Add(new PathFigure(loc7_p2, new ArcSegment[] { new ArcSegment(loc7_p5, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    else
                    {
                        figures.Add(new PathFigure(loc7_p2, new ArcSegment[] { new ArcSegment(loc7_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    
                    break;

                // 원이 y = 0 인 직선과 x = 0 인 직선, x = w 인 직선에 각각 두 점에 걸린 경우,
                case 11:
                    var loc11_p0 = new Point(centerX - tmpXVal, 0);
                    var loc11_p1 = new Point(centerX + tmpXVal, 0);
                    var loc11_p2 = new Point(0, centerY - tmpYVal);
                    var loc11_p3 = new Point(0, centerY + tmpYVal);
                    var loc11_p4 = new Point(w, centerY - tmpWVal);
                    var loc11_p5 = new Point(w, centerY + tmpWVal);
                    
                    if (loc11_p5.Y > 0)
                    {
                        if (loc11_p3.Y < 0)
                        {
                            figures.Add(new PathFigure(loc11_p5, new ArcSegment[] { new ArcSegment(loc11_p0, size, 0, false, SweepDirection.Clockwise, true) }, false));
                        }
                        else
                        {
                            figures.Add(new PathFigure(loc11_p5, new ArcSegment[] { new ArcSegment(loc11_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                            if (loc11_p2.Y > 0)
                            {
                                figures.Add(new PathFigure(loc11_p2, new ArcSegment[] { new ArcSegment(loc11_p0, size, 0, false, SweepDirection.Clockwise, true) }, false));
                            }
                            if (loc11_p4.Y > 0)
                            {
                                figures.Add(new PathFigure(loc11_p1, new ArcSegment[] { new ArcSegment(loc11_p4, size, 0, false, SweepDirection.Clockwise, true) }, false));
                            }
                        }
                    }
                    else if (loc11_p3.Y > 0)
                    {
                        figures.Add(new PathFigure(loc11_p1, new ArcSegment[] { new ArcSegment(loc11_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    else
                    {
                        figures.Add(new PathFigure(loc11_p1, new ArcSegment[] { new ArcSegment(loc11_p0, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }

                    break;

                // 원이 y = 0 인 직선과 y = h 인 직선, x = w 인 직선에 각각 두 점에 걸린 경우,
                case 13:
                    var loc13_p0 = new Point(centerX - tmpXVal, 0);
                    var loc13_p1 = new Point(centerX + tmpXVal, 0);
                    var loc13_p2 = new Point(centerX - tmpHVal, h);
                    var loc13_p3 = new Point(centerX + tmpHVal, h);
                    var loc13_p4 = new Point(w, centerY - tmpWVal);
                    var loc13_p5 = new Point(w, centerY + tmpWVal);

                    if (loc13_p0.X < w)
                    {
                        if (loc13_p2.X > w)
                        {
                            figures.Add(new PathFigure(loc13_p5, new ArcSegment[] { new ArcSegment(loc13_p0, size, 0, false, SweepDirection.Clockwise, true) }, false));
                        }
                        else
                        {
                            figures.Add(new PathFigure(loc13_p2, new ArcSegment[] { new ArcSegment(loc13_p0, size, 0, false, SweepDirection.Clockwise, true) }, false));
                            if (loc13_p3.X < w)
                            {
                                figures.Add(new PathFigure(loc13_p5, new ArcSegment[] { new ArcSegment(loc13_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                            }
                            if (loc13_p1.X < w)
                            {
                                figures.Add(new PathFigure(loc13_p1, new ArcSegment[] { new ArcSegment(loc13_p4, size, 0, false, SweepDirection.Clockwise, true) }, false));
                            }
                        }
                    }
                    else if (loc13_p2.X < w)
                    {
                        figures.Add(new PathFigure(loc13_p2, new ArcSegment[] { new ArcSegment(loc13_p4, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    else
                    {
                        figures.Add(new PathFigure(loc13_p5, new ArcSegment[] { new ArcSegment(loc13_p4, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    
                    break;

                // 원이 x = 0 인 직선과 y = h 인 직선, x = w 인 직선에 각각 두 점에 걸린 경우,
                case 14:
                    var loc14_p0 = new Point(0, centerY - tmpYVal);
                    var loc14_p1 = new Point(0, centerY + tmpYVal);
                    var loc14_p2 = new Point(centerX - tmpHVal, h);
                    var loc14_p3 = new Point(centerX + tmpHVal, h);
                    var loc14_p4 = new Point(w, centerY - tmpWVal);
                    var loc14_p5 = new Point(w, centerY + tmpWVal);

                    if (loc14_p0.Y < h)
                    {
                        if (loc14_p4.Y > h)
                        {
                            figures.Add(new PathFigure(loc14_p0, new ArcSegment[] { new ArcSegment(loc14_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                        }
                        else
                        {
                            figures.Add(new PathFigure(loc14_p0, new ArcSegment[] { new ArcSegment(loc14_p4, size, 0, false, SweepDirection.Clockwise, true) }, false));
                            if (loc14_p5.Y < h)
                            {
                                figures.Add(new PathFigure(loc14_p5, new ArcSegment[] { new ArcSegment(loc14_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                            }
                            if (loc14_p1.Y < h)
                            {
                                figures.Add(new PathFigure(loc14_p2, new ArcSegment[] { new ArcSegment(loc14_p1, size, 0, false, SweepDirection.Clockwise, true) }, false));
                            }
                        }
                    }
                    else if (loc14_p4.Y < h)
                    {
                        figures.Add(new PathFigure(loc14_p2, new ArcSegment[] { new ArcSegment(loc14_p4, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }
                    else
                    {
                        figures.Add(new PathFigure(loc14_p2, new ArcSegment[] { new ArcSegment(loc14_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    }

                    break;

                // 원이 모든 직선에 각각 두 점에 걸린 경우,
                case 15:
                    var loc15_p0 = new Point(centerX - tmpXVal, 0);
                    var loc15_p1 = new Point(centerX + tmpXVal, 0);
                    var loc15_p2 = new Point(0, centerY - tmpYVal);
                    var loc15_p3 = new Point(0, centerY + tmpYVal);
                    var loc15_p4 = new Point(centerX - tmpHVal, h);
                    var loc15_p5 = new Point(centerX + tmpHVal, h);
                    var loc15_p6 = new Point(w, centerY - tmpWVal);
                    var loc15_p7 = new Point(w, centerY + tmpWVal);

                    if (loc15_p0.X >= 0 && loc15_p0.X <= w && loc15_p2.Y >= 0 && loc15_p2.Y <= h) figures.Add(new PathFigure(loc15_p2, new ArcSegment[] { new ArcSegment(loc15_p0, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    if (loc15_p1.X >= 0 && loc15_p1.X <= w && loc15_p6.Y >= 0 && loc15_p6.Y <= h) figures.Add(new PathFigure(loc15_p1, new ArcSegment[] { new ArcSegment(loc15_p6, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    if (loc15_p5.X >= 0 && loc15_p5.X <= w && loc15_p7.Y >= 0 && loc15_p7.Y <= h) figures.Add(new PathFigure(loc15_p7, new ArcSegment[] { new ArcSegment(loc15_p5, size, 0, false, SweepDirection.Clockwise, true) }, false));
                    if (loc15_p4.X >= 0 && loc15_p4.X <= w && loc15_p3.Y >= 0 && loc15_p3.Y <= h) figures.Add(new PathFigure(loc15_p4, new ArcSegment[] { new ArcSegment(loc15_p3, size, 0, false, SweepDirection.Clockwise, true) }, false));

                    break;

                default:
                    throw new Exception("Invalid location number.");
            }

            return new PathFigureCollection(figures);
        }

        #endregion
    }
}
