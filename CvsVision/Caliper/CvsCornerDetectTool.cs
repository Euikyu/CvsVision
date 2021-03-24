using System;
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
        public Bitmap InputImage
        {
            get { return m_InputImage; }
            set
            {
                m_InputImage = value;
                Overlay = null;
            }
        }
        public CvsCornerSetting Setting
        {
            get { return m_Setting; }
            set
            {
                m_Setting = value;
                m_LineATool.Setting = m_Setting.LineASetting;
                m_LineBTool.Setting = m_Setting.LineBSetting;

                m_CornerDetect = m_Setting.GetToolParams();
            }
        }
        public CvsCorner Corner { get; private set; }
        public DrawingGroup Overlay { get; private set; }

        public Exception Exception { get; private set; }
        #endregion
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
            Setting.LineBSetting.OriginY = 40;
            Setting.LineBSetting.Radian = Math.PI / 2;

            m_CornerDetect = m_Setting.GetToolParams();
        }


        public void Dispose()
        {
            if (m_InputImage != null) InputImage.Dispose();
            if (m_LineATool != null) m_LineATool.Dispose();
            if (m_LineBTool != null) m_LineBTool.Dispose();
        }

        #region Methods
        public void Load(string filePath, Type toolType)
        {

        }
        public void Save(string path)
        {

        }

        public void Run()
        {
            try
            {
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
            return dg;
        }
        #endregion
    }
}
