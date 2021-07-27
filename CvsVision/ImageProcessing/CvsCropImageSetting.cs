using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CvsVision.ImageProcessing
{
    public class CvsCropImageSetting
    {
        #region Fields
        private CvsRectangleAffine m_Region;
        #endregion

        #region Properties
        public double OriginX
        {
            get
            {
                if (m_Region != null) return m_Region.OriginX;
                else return 0;
            }
            set
            {
                if (m_Region != null) m_Region.OriginX = value;
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
                if (m_Region != null) m_Region.OriginY = value;
            }
        }

        public Point Center
        {
            get
            {
                if (m_Region != null) return m_Region.Center;
                else return new Point();
            }
            set
            {
                if (m_Region != null) m_Region.Center = value;
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
                if (m_Region != null) m_Region.Radian = value;
            }
        }
        public CvsRectangleAffine Region
        {
            get { return m_Region; }
            set
            {
                m_Region = value;
            }
        }
        public double CropWidth
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
        public double CropHeight
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
        #endregion
        
        public CvsCropImageSetting()
        {
            m_Region = new CvsRectangleAffine();
        }

        #region Methods

        #endregion
    }
}
