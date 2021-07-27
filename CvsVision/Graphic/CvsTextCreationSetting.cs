using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CvsVision.Graphic
{
    /// <summary>
    /// 텍스트 오버레이 생성을 위한 설정 값 클래스입니다.
    /// </summary>
    public class CvsTextCreationSetting
    {
        #region Fields
        private CvsPose m_Pose;
        #endregion

        #region Properties
        /// <summary>
        /// 텍스트의 원점 X 좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginX { get { return m_Pose.TranslateX; } set { m_Pose.TranslateX = value; } }
        /// <summary>
        /// 텍스트의 원점 Y 좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginY { get { return m_Pose.TranslateY; } set { m_Pose.TranslateY = value; } }
        /// <summary>
        /// 텍스트의 회전 라디안 값을 가져오거나 설정합니다.
        /// </summary>
        public double Radian { get; set; }
        /// <summary>
        /// 텍스트의 폰트를 가져오거나 설정합니다.
        /// </summary>
        public string FontName { get; set; }
        /// <summary>
        /// 텍스트의 폰트 색상을 가져오거나 설정합니다.
        /// </summary>
        public Brush FontBrush { get; set; }
        /// <summary>
        /// 텍스트의 폰트 크기를 가져오거나 설정합니다.
        /// </summary>
        public double FontSize { get; set; }
        #endregion

        /// <summary>
        /// 텍스트 오버레이를 위한 설정 값 클래스를 생성합니다.
        /// </summary>
        public CvsTextCreationSetting()
        {
            m_Pose = new CvsPose();
            FontName = System.Drawing.SystemFonts.DefaultFont.FontFamily.Name;
            FontSize = System.Drawing.SystemFonts.DefaultFont.Size;
            FontBrush = Brushes.DarkRed;
        }

        #region Methods
        /// <summary>
        /// 부모 Pose를 변경합니다.
        /// </summary>
        /// <param name="parent">부모가 될 Pose.</param>
        public void SetParentPose(CvsPose parent)
        {
            m_Pose.Parent = parent;
        }
        /// <summary>
        /// 원점 좌표계에서의 점 위치를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public Point GetPointByOrigin()
        {
            return m_Pose.GetPointByOrigin(0,0);
        }
        /// <summary>
        /// 원점 좌표계에서의 회전 라디안 값을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public double GetRadianByOrigin()
        {
            return m_Pose.GetRadianByOrigin() + Radian;
        }
        #endregion
    }
}
