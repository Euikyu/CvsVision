using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvsVision.Caliper
{
    /// <summary>
    /// 원을 찾기 위한 설정 값 클래스입니다.
    /// </summary>
    public class CvsCircleSetting
    {
        #region Fields
        private CvsCircleDetect m_CircleDetect;
        private CvsEdgeSetting m_EdgeSetting;
        #endregion

        #region Properties

        #region Circle Settings
        /// <summary>
        /// 원으로 인정되는 범위 값을 가져오거나 설정합니다.
        /// </summary>
        public double ConsensusThreshold
        {
            get
            {
                if (m_CircleDetect != null) return m_CircleDetect.ConsensusThreshold;
                else return 0;
            }
            set { if (m_CircleDetect != null) m_CircleDetect.ConsensusThreshold = value; }
        }
        public CvsPose CirclePose { get; set; }
        /// <summary>
        /// 원 모델의 원점 X 좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginX
        {
            get
            {
                if (CirclePose != null) return CirclePose.TranslateX;
                else return 0;
            }
            set { if (CirclePose != null) CirclePose.TranslateX = value; }
        }
        /// <summary>
        /// 원 모델의 원점 Y 좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginY
        {
            get
            {
                if (CirclePose != null) return CirclePose.TranslateY;
                else return 0;
            }
            set { if (CirclePose != null) CirclePose.TranslateY = value; }
        }
        /// <summary>
        /// 원 모델의 캘리퍼 개수를 가져오거나 설정합니다.
        /// </summary>
        public int CaliperCount { get; set; }
        /// <summary>
        /// 원 그래픽의 시작 각도를 가져오거나 설정합니다.
        /// </summary>
        public double StartAngle { get; set; }
        /// <summary>
        /// 원 그래픽의 끝 각도를 가져오거나 설정합니다.
        /// </summary>
        public double EndAngle { get; set; }
        /// <summary>
        /// 원 그래픽의 반지름 값을 가져오거나 설정합니다.
        /// </summary>
        public double Radius { get; set; }
        /// <summary>
        /// 원 모델의 검색 방향을 바깥 방향으로 검사할 것인지의 여부를 가져오거나 설정합니다.
        /// </summary>
        public bool IsOutwardDirection { get; set; }
        #endregion

        #region Caliper Settings
        /// <summary>
        /// 각 에지의 투사 길이를 가져오거나 설정합니다.
        /// (영역의 너비입니다.)
        /// </summary>
        public double ProjectionLength
        {
            get
            {
                if (m_EdgeSetting != null) return m_EdgeSetting.ProjectionLength;
                else return 0;
            }
            set
            {
                if (m_EdgeSetting != null)
                {
                    m_EdgeSetting.ProjectionLength = value;
                }
            }
        }
        /// <summary>
        /// 각 에지의 검색 길이를 가져오거나 설정합니다.
        /// (영역의 높이입니다.)
        /// </summary>
        public double SearchLength
        {
            get
            {
                if (m_EdgeSetting != null) return m_EdgeSetting.SearchLength;
                else return 0;
            }
            set
            {
                if (m_EdgeSetting != null)
                {
                    m_EdgeSetting.SearchLength = value;
                }
            }
        }
        /// <summary>
        /// 각 에지의 절반 픽셀 크기를 가져오거나 설정합니다.
        /// </summary>
        public uint HalfPixelCount
        {
            get
            {
                if (m_EdgeSetting != null) return m_EdgeSetting.HalfPixelCount;
                else return 0;
            }
            set
            {
                if (m_EdgeSetting != null)
                {
                    m_EdgeSetting.HalfPixelCount = value;
                }
            }
        }
        /// <summary>
        /// 각 에지의 대비 임계값을 가져오거나 설정합니다.
        /// </summary>
        public uint ContrastThreshold
        {
            get
            {
                if (m_EdgeSetting != null) return m_EdgeSetting.ContrastThreshold;
                else return 0;
            }
            set
            {
                if (m_EdgeSetting != null)
                {
                    m_EdgeSetting.ContrastThreshold = value;
                }
            }
        }
        /// <summary>
        /// 각 에지를 감지할 방향을 가져오거나 설정합니다.
        /// </summary>
        public EDirection EdgeDirection
        {
            get
            {
                if (m_EdgeSetting != null) return m_EdgeSetting.EdgeDirection;
                else return 0;
            }
            set
            {
                if (m_EdgeSetting != null)
                {
                    m_EdgeSetting.EdgeDirection = value;
                }
            }
        }
        #endregion

        #endregion
        
        /// <summary>
        /// 원을 찾기 위한 설정 값 클래스를 생성합니다.
        /// </summary>
        public CvsCircleSetting()
        {
            CirclePose = new CvsPose();
            m_CircleDetect = new CvsCircleDetect();
            m_EdgeSetting = new CvsEdgeSetting();
        }
        /// <summary>
        /// 현재 설정을 가진 원 검색 클래스를 반환합니다.
        /// </summary>
        /// <returns></returns>
        #region Methods
        public CvsCircleDetect GetToolParams()
        {
            return m_CircleDetect;
        }
        /// <summary>
        /// 현재 에지에 해당되는 설정 값을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public CvsEdgeSetting GetCaliperSettings()
        {
            return m_EdgeSetting;
        }
        #endregion
    }
}
