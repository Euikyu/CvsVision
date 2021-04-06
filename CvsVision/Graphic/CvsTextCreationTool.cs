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
using Point = System.Windows.Point;

namespace CvsVision.Graphic
{
    public class CvsTextCreationTool : ICvsTool
    {
        #region Fields

        #endregion

        #region Properties
        public Bitmap InputImage { get; set; }

        public string Text { get; set; }

        public CvsTextCreationSetting Setting { get; set; }

        public DrawingGroup Overlay { get; private set; }

        public Exception Exception { get; private set; }

        #endregion

        public CvsTextCreationTool()
        {
            Setting = new CvsTextCreationSetting();
            Text = string.Empty;
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
                XmlSerializer xml = new XmlSerializer(typeof(CvsTextCreationSetting));

                using (var sr = new StreamReader(path))
                {
                    try
                    {
                        var newSetting = xml.Deserialize(sr) as CvsTextCreationSetting;
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
                    XmlSerializer xml = new XmlSerializer(typeof(CvsTextCreationSetting));
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

                DrawingGroup dg = new DrawingGroup();
                GeometryDrawing overlay = new GeometryDrawing
                {
                    Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, InputImage.Width, InputImage.Height)),
                    Brush = Brushes.Transparent,
                    Pen = new Pen(Brushes.Transparent, 0)
                };
                dg.Children.Add(overlay);

                FormattedText ft = new FormattedText(Text,
                                                     System.Globalization.CultureInfo.CurrentCulture,
                                                     System.Windows.FlowDirection.LeftToRight,
                                                     new Typeface(Setting.FontName),
                                                     Setting.FontSize,
                                                     Setting.FontBrush, 1);

                var originPoint = Setting.GetPointByOrigin();

                GeometryDrawing textOverlay = new GeometryDrawing
                {
                    Geometry = ft.BuildGeometry(originPoint),
                    Brush = Setting.FontBrush,
                    Pen = new Pen(Setting.FontBrush, 0.4)
                };

                //dg.Children.Add(textOverlay);
                DrawingGroup textTransformGroup = new DrawingGroup();
                textTransformGroup.Children.Add(textOverlay);
                textTransformGroup.Transform = new RotateTransform(Setting.GetRadianByOrigin() * 180 / Math.PI, originPoint.X, originPoint.Y);
                dg.Children.Add(textTransformGroup);

                dg.Freeze();

                Overlay = dg;

                Exception = null;
            }
            catch (Exception err)
            {
                Exception = err;
            }
        }
        #endregion
    }
}
