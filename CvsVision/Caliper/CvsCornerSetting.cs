using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CvsVision.Caliper
{
    /// <summary>
    /// 두 직선의 교점을 찾기 위한 설정 값 클래스입니다.
    /// </summary>
    public class CvsCornerSetting
    {
        #region Fields
        private CvsCornerDetect m_Detect;
        private CvsLineSetting m_LineASetting;
        private CvsLineSetting m_LineBSetting;
        #endregion

        #region Properties
        /// <summary>
        /// Line A에 대한 설정 값을 가져오거나 설정합니다.
        /// </summary>
        public CvsLineSetting LineASetting
        {
            get { return m_LineASetting; }
            set
            {
                m_LineASetting = value;
                if(value != null) m_Detect.LineA = m_LineASetting.GetToolParams().Line;
            }
        }
        /// <summary>
        /// Line B에 대한 설정 값을 가져오거나 설정합니다.
        /// </summary>
        public CvsLineSetting LineBSetting
        {
            get { return m_LineBSetting; }
            set
            {
                m_LineBSetting = value;
                if (value != null) m_Detect.LineB = m_LineBSetting.GetToolParams().Line;
            }
        }
        #endregion
        /// <summary>
        /// 두 직선의 교점을 찾기 위한 설정 값 클래스를 생성합니다.
        /// </summary>
        public CvsCornerSetting()
        {
            m_Detect = new CvsCornerDetect();
            LineASetting = new CvsLineSetting();
            LineBSetting = new CvsLineSetting();
        }
        #region Methods
        /// <summary>
        /// 현재 설정을 가진 교점 검색 클래스를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public CvsCornerDetect GetToolParams()
        {
            return m_Detect;
        }
        #endregion
    }
}
