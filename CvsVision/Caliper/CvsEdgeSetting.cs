using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvsVision.Caliper
{

    public class CvsEdgeSetting
    {
        #region Fields
        private CvsRectangleAffine m_Region = new CvsRectangleAffine();
        private CvsEdgeDetect m_EdgeDetect = new CvsEdgeDetect();
        #endregion

        #region Properties
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
        /// 에지를 감지할 방향.
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

        public ICvsRegion Region
        {
            get { return m_Region; }
            set
            {
                if (value is CvsRectangleAffine affine)
                {
                    m_Region = affine;
                }
            }
        }
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
        public CvsEdgeDetect GetToolParams()
        {
            return m_EdgeDetect;
        }
        #endregion
    }
}
