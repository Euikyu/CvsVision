using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;

namespace CvsVision.Caliper
{
    public class CvsEdgeDetectTool : ICvsTool
    {
        #region Fields
        private CvsEdgeSetting m_Setting;
        private CvsEdgeDetect m_EdgeDetect;
        private Bitmap m_InputImage;
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
        public CvsEdgeSetting Setting
        {
            get { return m_Setting; }
            set
            {
                m_Setting = value;
                m_EdgeDetect = m_Setting.GetToolParams();
            }
        }
        public CvsEdge Edge { get { return m_EdgeDetect.Edge; } }
        public DrawingGroup Overlay { get; private set; }
        

        public Exception Exception { get; private set; }
        #endregion

        public CvsEdgeDetectTool()
        {
            Setting = new CvsEdgeSetting();
        }

        public void Dispose()
        {
            if (InputImage != null) InputImage.Dispose();
            if (m_EdgeDetect != null) m_EdgeDetect.Dispose(); 
        }

        #region Methods
        public void Load(string filePath, Type toolType)
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


        public void Run()
        {
            try
            {
                var cropImage = Setting.Region.Crop(InputImage);
                m_EdgeDetect.DetectImage = cropImage;
                m_EdgeDetect.Detect();
                if (m_EdgeDetect.Edge == null) throw new Exception("Edge not found.");

                Overlay = this.CreateGeometry();

                Exception = null;
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
                Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, InputImage.Width, InputImage.Height)),
                Brush = Brushes.Transparent,
                Pen = new Pen(Brushes.Transparent, 0)
            };
            dg.Children.Add(overlay);

            if (m_EdgeDetect != null && m_EdgeDetect.Edge != null)
            {
                GeometryDrawing graphic = new GeometryDrawing();
                GeometryGroup group = new GeometryGroup();

                group.Children.Add(new LineGeometry(Setting.Region.Pose.GetPointByOrigin(-Setting.ProjectionLength / 2, m_EdgeDetect.Edge.Y), Setting.Region.Pose.GetPointByOrigin(Setting.ProjectionLength / 2, m_EdgeDetect.Edge.Y)));

                graphic.Geometry = group;
                graphic.Brush = Brushes.Transparent;
                graphic.Pen = new Pen(Brushes.LawnGreen, 1);
                graphic.Freeze();

                dg.Children.Add(graphic);
            }
            return dg;
        }

        #endregion
    }
}
