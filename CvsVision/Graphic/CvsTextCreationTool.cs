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
    /// <summary>
    /// 이미지의 특정 위치에 텍스트 오버레이 생성을 하는 도구 클래스입니다.
    /// </summary>
    public class CvsTextCreationTool : ICvsTool
    {
        #region Fields

        #endregion

        #region Properties
        /// <summary>
        /// 입력 이미지를 가져오거나 설정합니다.
        /// </summary>
        public Bitmap InputImage { get; set; }
        /// <summary>
        /// 오버레이로 출력할 텍스트를 가져오거나 설정합니다.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 텍스트 오버레이 생성을 위한 설정 값을 가져오거나 설정합니다.
        /// </summary>
        public CvsTextCreationSetting Setting { get; set; }
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
        /// 이미지의 특정 위치에 텍스트 오버레이 생성을 하는 도구 클래스를 생성합니다.
        /// </summary>
        public CvsTextCreationTool()
        {
            Setting = new CvsTextCreationSetting();
            Text = string.Empty;
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

        /// <summary>
        /// 텍스트 오버레이를 생성합니다.
        /// </summary>
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
