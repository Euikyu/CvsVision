using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CvsVision.Graphic
{
    public class CvsTextCreationSetting
    {
        #region Fields
        private CvsPose m_Pose;
        #endregion

        #region Properties
        public double OriginX { get { return m_Pose.TranslateX; } set { m_Pose.TranslateX = value; } }
        public double OriginY { get { return m_Pose.TranslateY; } set { m_Pose.TranslateY = value; } }
        public double Radian { get; set; }
        public string FontName { get; set; }
        public Brush FontBrush { get; set; }
        public double FontSize { get; set; }
        #endregion

        public CvsTextCreationSetting()
        {
            m_Pose = new CvsPose();
            FontName = System.Drawing.SystemFonts.DefaultFont.FontFamily.Name;
            FontSize = System.Drawing.SystemFonts.DefaultFont.Size;
            FontBrush = Brushes.DarkRed;
        }

        #region Methods
        public void SetParentPose(CvsPose parent)
        {
            m_Pose.Parent = parent;
        }
        public Point GetPointByOrigin()
        {
            return m_Pose.GetPointByOrigin(0,0);
        }
        public double GetRadianByOrigin()
        {
            return m_Pose.GetRadianByOrigin() + Radian;
        }
        #endregion
    }
}
