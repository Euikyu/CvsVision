using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvsVision.Caliper
{
    public class CvsBreadthSetting
    {
        #region Fields
        private CvsRectangleAffine m_Region = new CvsRectangleAffine();
        private CvsBreadthDetect m_BreadthDetect = new CvsBreadthDetect();
        #endregion

        #region Properties
        /// <summary>
        /// 특정 이상의 변화량을 에지로 판단하기 위한 임계값을 가져오거나 설정합니다. 
        /// </summary>
        public uint ContrastThreshold
        {
            get
            {
                if (m_BreadthDetect != null) return m_BreadthDetect.ContrastThreshold;
                else return 0;
            }
            set
            {
                if (m_BreadthDetect != null) m_BreadthDetect.ContrastThreshold = value;
            }
        }
        /// <summary>
        /// 에지로 인식할 절반 픽셀 개수를 가져오거나 설정합니다.
        /// </summary>
        public uint HalfPixelCount
        {
            get
            {
                if (m_BreadthDetect != null) return m_BreadthDetect.HalfPixelCount;
                else return 0;
            }
            set
            {
                if (m_BreadthDetect != null)
                {
                    if (value < 1) m_BreadthDetect.HalfPixelCount = 1;
                    else m_BreadthDetect.HalfPixelCount = value;
                }
            }
        }
        /// <summary>
        /// 에지를 감지할 방향을 가져오거나 설정합니다.
        /// </summary>
        public EDirection Edge0Direction
        {
            get
            {
                if (m_BreadthDetect != null) return m_BreadthDetect.Edge0Direction;
                else return EDirection.Any;
            }
            set
            {
                if (m_BreadthDetect != null) m_BreadthDetect.Edge0Direction = value;
            }
        }
        /// <summary>
        /// 에지를 감지할 방향을 가져오거나 설정합니다.
        /// </summary>
        public EDirection Edge1Direction
        {
            get
            {
                if (m_BreadthDetect != null) return m_BreadthDetect.Edge1Direction;
                else return EDirection.Any;
            }
            set
            {
                if (m_BreadthDetect != null) m_BreadthDetect.Edge1Direction = value;
            }
        }
        public double TargetBreadth
        {
            get
            {
                if (m_BreadthDetect != null) return m_BreadthDetect.TargetBreadth;
                else return -1;
            }
            set
            {
                if (m_BreadthDetect != null) m_BreadthDetect.TargetBreadth = value;
            }
        }
        /// <summary>
        /// 에지 검색 영역을 가져오거나 설정합니다.
        /// </summary>
        public CvsRectangleAffine Region
        {
            get { return m_Region; }
            set
            {
                m_Region = value;
            }
        }
        /// <summary>
        /// 에지의 투사 길이를 가져오거나 설정합니다. 
        /// (영역의 너비입니다.)
        /// </summary>
        public double ProjectionLength
        {
            get
            {
                if (m_Region != null) return m_Region.Width;
                else return 0;
            }
            set
            {
                if (m_Region != null)
                {
                    m_Region.Width = value;
                }
            }
        }
        /// <summary>
        /// 에지의 검색 길이를 가져오거나 설정합니다.
        /// (영역의 높이입니다.)
        /// </summary>
        public double SearchLength
        {
            get
            {
                if (m_Region != null) return m_Region.Height;
                else return 0;
            }
            set
            {
                if (m_Region != null)
                {
                    m_Region.Height = value;
                }
            }
        }
        /// <summary>
        /// 에지 영역의 원점 X 좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginX
        {
            get
            {
                if (m_Region != null) return m_Region.OriginX;
                else return 0;
            }
            set
            {
                if (m_Region != null)
                {
                    m_Region.OriginX = value;
                }
            }
        }
        /// <summary>
        /// 에지 영역의 원점 Y 좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginY
        {
            get
            {
                if (m_Region != null) return m_Region.OriginY;
                else return 0;
            }
            set
            {
                if (m_Region != null)
                {
                    m_Region.OriginY = value;
                }
            }
        }
        /// <summary>
        /// 에지 영역의 회전 라디안 값을 가져오거나 설정합니다.
        /// </summary>
        public double Radian
        {
            get
            {
                if (m_Region != null) return m_Region.Radian;
                else return 0;
            }
            set
            {
                if (m_Region != null)
                {
                    m_Region.Radian = value;
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 현재 설정을 가진 에지 검색 클래스를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public CvsBreadthDetect GetToolParams()
        {
            return m_BreadthDetect;
        }
        #endregion
    }
}
