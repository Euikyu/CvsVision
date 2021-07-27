using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace CvsVision.ImageProcessing
{
    public class CvsCropImageTool : ICvsTool
    {
        #region Fields

        #endregion

        #region Properties
        public System.Drawing.Bitmap InputImage { get; set; }

        public System.Drawing.Bitmap OutputImage { get; private set; }

        public CvsCropImageSetting Setting { get; set; }

        public DrawingGroup Overlay { get; private set; }

        public Exception Exception { get; private set; }

        #endregion

        public CvsCropImageTool()
        {
            Setting = new CvsCropImageSetting
            {
                OriginX = 20,
                OriginY = 20,
                CropWidth = 100,
                CropHeight = 100,                
            };
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
                XmlSerializer xml = new XmlSerializer(typeof(CvsCropImageSetting));

                using (var sr = new StreamReader(path))
                {
                    try
                    {
                        var newSetting = xml.Deserialize(sr) as CvsCropImageSetting;
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
                    XmlSerializer xml = new XmlSerializer(typeof(CvsCropImageSetting));
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

                OutputImage = Setting.Region.Crop(InputImage);

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

            var w = Setting.Region.Width / 2;
            var h = Setting.Region.Height / 2;
            var nwPoint = Setting.Region.Pose.GetPointByOrigin(-w, -h);
            var nePoint = Setting.Region.Pose.GetPointByOrigin(w, -h);
            var swPoint = Setting.Region.Pose.GetPointByOrigin(-w, h);
            var sePoint = Setting.Region.Pose.GetPointByOrigin(w, h);

            GeometryDrawing graphic = new GeometryDrawing();
            GeometryGroup group = new GeometryGroup();

            group.Children.Add(new LineGeometry(nwPoint, nePoint));
            group.Children.Add(new LineGeometry(sePoint, nePoint));
            group.Children.Add(new LineGeometry(nwPoint, swPoint));
            group.Children.Add(new LineGeometry(sePoint, swPoint));

            graphic.Geometry = group;
            graphic.Brush = Brushes.Transparent;
            graphic.Pen = new Pen(Brushes.Cyan, 5);
            graphic.Freeze();
            
            dg.Children.Add(graphic);

            dg.Freeze();

            return dg;
        }
        #endregion
    }
}
