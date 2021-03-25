using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CvsVision.Caliper
{
    /// <summary>
    /// 에지를 찾기 위한 설정 값 클래스입니다.
    /// </summary>
    public class CvsEdgeSetting
    {
        #region Fields
        private CvsRectangleAffine m_Region = new CvsRectangleAffine();
        private CvsEdgeDetect m_EdgeDetect = new CvsEdgeDetect();
        #endregion

        #region Properties
        /// <summary>
        /// 특정 이상의 변화량을 에지로 판단하기 위한 임계값을 가져오거나 설정합니다. 
        /// </summary>
        public uint ContrastThreshold
        {
            get
            {
                if (m_EdgeDetect != null) return m_EdgeDetect.ContrastThreshold;
                else return 0;
            }
            set
            {
                if (m_EdgeDetect != null) m_EdgeDetect.ContrastThreshold = value;
            }
        }
        /// <summary>
        /// 에지로 인식할 절반 픽셀 개수를 가져오거나 설정합니다.
        /// </summary>
        public uint HalfPixelCount
        {
            get
            {
                if (m_EdgeDetect != null) return m_EdgeDetect.HalfPixelCount;
                else return 0;
            }
            set
            {
                if (m_EdgeDetect != null)
                {
                    if (value < 1) m_EdgeDetect.HalfPixelCount = 1;
                    else m_EdgeDetect.HalfPixelCount = value;
                }
            }
        }
        /// <summary>
        /// 에지를 감지할 방향을 가져오거나 설정합니다.
        /// </summary>
        public EDirection EdgeDirection
        {
            get
            {
                if (m_EdgeDetect != null) return m_EdgeDetect.EdgeDirection;
                else return EDirection.Any;
            }
            set
            {
                if (m_EdgeDetect != null) m_EdgeDetect.EdgeDirection = value;
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
        public CvsEdgeDetect GetToolParams()
        {
            return m_EdgeDetect;
        }
        #endregion
    }
}
