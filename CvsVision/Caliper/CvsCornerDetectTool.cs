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
    /// 이미지를 통해 두 점의 교점을 찾을 수 있는 도구 클래스입니다.
    /// </summary>
    public class CvsCornerDetectTool : ICvsTool
    {
        #region Fields
        private CvsLineDetectTool m_LineATool;
        private CvsLineDetectTool m_LineBTool;
        private Bitmap m_InputImage;
        private CvsCornerSetting m_Setting;

        private CvsCornerDetect m_CornerDetect;
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
        /// 교점 검색을 위한 설정 값을 가져오거나 설정합니다.
        /// </summary>
        public CvsCornerSetting Setting
        {
            get { return m_Setting; }
            set
            {
                m_Setting = value;
                if (value != null)
                {
                    m_LineATool.Setting = m_Setting.LineASetting;
                    m_LineBTool.Setting = m_Setting.LineBSetting;

                    m_CornerDetect = m_Setting.GetToolParams();
                }
            }
        }
        /// <summary>
        /// 결과 교점을 가져옵니다.
        /// </summary>
        public CvsCorner Corner { get; private set; }
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
        /// 두 직선의 교점을 찾는 도구를 생성합니다.
        /// </summary>
        public CvsCornerDetectTool()
        {
            m_LineATool = new CvsLineDetectTool();
            m_LineBTool = new CvsLineDetectTool();

            m_Setting = new CvsCornerSetting
            {
                LineASetting = m_LineATool.Setting,
                LineBSetting = m_LineBTool.Setting
            };

            Setting.LineASetting.OriginX = 40;
            Setting.LineASetting.OriginY = 20;

            Setting.LineBSetting.OriginX = 20;
            Setting.LineBSetting.OriginY = 140;
            Setting.LineBSetting.Radian = -Math.PI / 2;

            m_CornerDetect = m_Setting.GetToolParams();
        }


#pragma warning disable CS1591 // 공개된 형식 또는 멤버에 대한 XML 주석이 없습니다.
        public void Dispose()
#pragma warning restore CS1591 // 공개된 형식 또는 멤버에 대한 XML 주석이 없습니다.
        {
            if (m_InputImage != null) InputImage.Dispose();
            if (m_LineATool != null) m_LineATool.Dispose();
            if (m_LineBTool != null) m_LineBTool.Dispose();
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
                XmlSerializer xml = new XmlSerializer(typeof(CvsCornerSetting));

                using (var sr = new StreamReader(path))
                {
                    try
                    {
                        var newSetting = xml.Deserialize(sr) as CvsCornerSetting;
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
                    XmlSerializer xml = new XmlSerializer(typeof(CvsCornerSetting));
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
        /// 교점 검색을 시작합니다.
        /// </summary>
        public void Run()
        {
            try
            {
                if (InputImage == null) throw new Exception("Input image first.");

                m_LineATool.InputImage = InputImage;
                m_LineBTool.InputImage = InputImage;

                m_LineATool.Run();
                m_LineBTool.Run();

                m_CornerDetect.LineA = m_LineATool.Line;
                m_CornerDetect.LineB = m_LineBTool.Line;
                m_CornerDetect.Detect();

                Corner = m_CornerDetect.Corner;

                Overlay = this.CreateGeometry();

                Exception = null;
            }
            catch(Exception err)
            {
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
            dg.Children.Add(m_LineATool.Overlay);
            dg.Children.Add(m_LineBTool.Overlay);

            //이미지 영역 안에서 한 점에서 만나는 경우만 점 그래픽 추가
            if (m_CornerDetect.Corner != null && m_CornerDetect.Corner.IntersectionAngle != 0 && 
                m_CornerDetect.Corner.Corner.X >= 0 && m_CornerDetect.Corner.Corner.X <= m_InputImage.Width &&
                m_CornerDetect.Corner.Corner.Y >= 0 && m_CornerDetect.Corner.Corner.Y <= m_InputImage.Height)
            {
                GeometryDrawing graphic = new GeometryDrawing();
                GeometryGroup group = new GeometryGroup();

                group.Children.Add(new EllipseGeometry(m_CornerDetect.Corner.Corner, 1, 1));
                group.Children.Add(new EllipseGeometry(m_CornerDetect.Corner.Corner, 5, 5));

                graphic.Geometry = group;
                graphic.Brush = Brushes.Transparent;
                graphic.Pen = new Pen(Brushes.Cyan, 1);
                graphic.Freeze();

                dg.Children.Add(graphic);
            }
            dg.Freeze();

            return dg;
        }
        #endregion
    }
}
