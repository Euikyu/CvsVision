using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;

namespace CvsVision.Caliper
{
    public class CvsBreadthDetectTool : ICvsTool
    {
        #region Fields
        private CvsBreadthSetting m_Setting;
        private CvsBreadthDetect m_BreadthDetect;
        private Bitmap m_InputImage;
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
        /// 에지 검색을 위한 설정 값을 가져오거나 설정합니다.
        /// </summary>
        public CvsBreadthSetting Setting
        {
            get { return m_Setting; }
            set
            {
                m_Setting = value;
                if (value != null) m_BreadthDetect = m_Setting.GetToolParams();
            }
        }
        /// <summary>
        /// 결과 에지를 가져옵니다.
        /// </summary>
        public CvsBreadth Breadth { get { return m_BreadthDetect.Breadth; } }
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
        /// 에지를 찾는 도구 클래스를 생성합니다.
        /// </summary>
        public CvsBreadthDetectTool()
        {
            Setting = new CvsBreadthSetting
            {
                OriginX = 20,
                OriginY = 20,
                ProjectionLength = 30,
                SearchLength = 100,
                ContrastThreshold = 5,
                HalfPixelCount = 2,
                TargetBreadth = -1
            };
        }

        public void Dispose()
        {
            if (InputImage != null) InputImage.Dispose();
            if (m_BreadthDetect != null) m_BreadthDetect.Dispose();
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
                XmlSerializer xml = new XmlSerializer(typeof(CvsBreadthSetting));

                using (var sr = new StreamReader(path))
                {
                    try
                    {
                        var newSetting = xml.Deserialize(sr) as CvsBreadthSetting;
                        Setting = newSetting ?? throw new Exception("Another type of tool setting file.");
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
                    XmlSerializer xml = new XmlSerializer(typeof(CvsBreadthSetting));
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
        /// 에지 검색을 시작합니다.
        /// </summary>
        public void Run()
        {
            try
            {
                if (InputImage == null) throw new Exception("Input image first.");

                var cropImage = Setting.Region.Crop(InputImage);
                m_BreadthDetect.DetectImage = cropImage;
                m_BreadthDetect.Detect();
                if (m_BreadthDetect.Breadth == null) throw new Exception("Breadth not found.");

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
                Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, InputImage.Width, InputImage.Height)),
                Brush = Brushes.Transparent,
                Pen = new Pen(Brushes.Transparent, 0)
            };
            dg.Children.Add(overlay);

            if (m_BreadthDetect != null && m_BreadthDetect.Breadth != null)
            {
                GeometryDrawing graphic = new GeometryDrawing();
                GeometryGroup group = new GeometryGroup();
                if (m_BreadthDetect.Breadth.Edge0 != null)
                {
                    group.Children.Add(new LineGeometry(Setting.Region.Pose.GetPointByOrigin(-Setting.ProjectionLength / 2, m_BreadthDetect.Breadth.Edge0.Y), Setting.Region.Pose.GetPointByOrigin(Setting.ProjectionLength / 2, m_BreadthDetect.Breadth.Edge0.Y)));
                }
                if (m_BreadthDetect.Breadth.Edge1 != null)
                {
                    group.Children.Add(new LineGeometry(Setting.Region.Pose.GetPointByOrigin(-Setting.ProjectionLength / 2, m_BreadthDetect.Breadth.Edge1.Y), Setting.Region.Pose.GetPointByOrigin(Setting.ProjectionLength / 2, m_BreadthDetect.Breadth.Edge1.Y)));
                }

                graphic.Geometry = group;
                graphic.Brush = Brushes.Transparent;
                graphic.Pen = new Pen(Brushes.LawnGreen, 1);
                graphic.Freeze();

                dg.Children.Add(graphic);
            }
            dg.Freeze();

            return dg;
        }

        #endregion
    }
}
