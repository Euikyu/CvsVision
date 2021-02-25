using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvsVision.Caliper
{
    public class CvsLineSetting
    {
        #region Fields
        private CvsLineDetect m_LineDetect;
        private CvsEdgeSettingCollection m_Collection;
        private CvsPose m_Pose;
        #endregion

        #region Properties
        public CvsEdgeSettingCollection EdgeCollection { get { return m_Collection; } }
        public double ConsensusThreshold
        {
            get
            {
                if (m_LineDetect != null) return m_LineDetect.ConsensusThreshold;
                else return 0;
            }
            set { if (m_LineDetect != null) m_LineDetect.ConsensusThreshold = value; }
        }
        public double OriginX
        {
            get
            {
                if (m_Pose != null) return m_Pose.TranslateX;
                else return 0;
            }
            set { if (m_Pose != null) m_Pose.TranslateX = value; }
        }
        public double OriginY
        {
            get
            {
                if (m_Pose != null) return m_Pose.TranslateY;
                else return 0;
            }
            set { if (m_Pose != null) m_Pose.TranslateY = value; }
        }
        public double Radian
        {
            get
            {
                if (m_Pose != null) return m_Pose.Radian;
                else return 0;
            }
            set { if (m_Pose != null) m_Pose.Radian = value; }
        }
        public double ProjectionLength
        {
            get
            {
                if (m_Collection != null) return m_Collection.ProjectionLength;
                else return 0;
            }
            set { if (m_Collection != null) m_Collection.ProjectionLength = value; }
        }
        public double SearchLength
        {
            get
            {
                if (m_Collection != null) return m_Collection.SearchLength;
                else return 0;
            }
            set { if (m_Collection != null) m_Collection.SearchLength = value; }
        }
        public uint HalfPixelCount
        {
            get
            {
                if (m_Collection != null) return m_Collection.HalfPixelCount;
                else return 0;
            }
            set { if (m_Collection != null) m_Collection.HalfPixelCount = value; }
        }
        public uint ContrastThreshold
        {
            get
            {
                if (m_Collection != null) return m_Collection.ContrastThreshold;
                else return 0;
            }
            set { if (m_Collection != null) m_Collection.ContrastThreshold = value; }
        }
        public EDirection EdgeDirection
        {
            get
            {
                if (m_Collection != null) return m_Collection.EdgeDirection;
                else return 0;
            }
            set { if (m_Collection != null) m_Collection.EdgeDirection = value; }
        }
        #endregion

        public CvsLineSetting()
        {
            m_Pose = new CvsPose();
            m_LineDetect = new CvsLineDetect();
            m_Collection = new CvsEdgeSettingCollection();
            m_Collection.SetParentPose(m_Pose);
        }
        #region Methods
        public CvsLineDetect GetToolParams()
        {
            return m_LineDetect;
        }
        #endregion
    }

    public class CvsEdgeSettingCollection : IEnumerable<CvsEdgeSetting>
    {
        #region Fields
        List<CvsEdgeSetting> m_List;

        private CvsPose m_Pose;
        //private double m_SegmentLength;

        private double m_ProjectionLength;
        private double m_SearchLength;
        private uint m_HalfPixelCount;
        private uint m_ContrastThreshold;
        private EDirection m_EdgeDirection;
        #endregion

        #region Properties
        public CvsEdgeSetting this[int index] { get { return m_List[index]; } set { m_List[index] = value; } }
        public int Count { get { return m_List.Count; } }
        public double ProjectionLength
        {
            get { return m_ProjectionLength; }
            set
            {
                m_ProjectionLength = value;
                foreach (var item in m_List)
                {
                    item.ProjectionLength = m_ProjectionLength;
                }
            }
        }
        public double SearchLength
        {
            get { return m_SearchLength; }
            set
            {
                m_SearchLength = value;
                foreach (var item in m_List)
                {
                    item.SearchLength = m_SearchLength;
                }
            }
        }
        public uint HalfPixelCount
        {
            get { return m_HalfPixelCount; }
            set
            {
                m_HalfPixelCount = value;
                foreach (var item in m_List)
                {
                    item.HalfPixelCount = m_HalfPixelCount;
                }
            }
        }
        public uint ContrastThreshold
        {
            get { return m_ContrastThreshold; }
            set
            {
                m_ContrastThreshold = value;
                foreach (var item in m_List)
                {
                    item.ContrastThreshold = m_ContrastThreshold;
                }
            }
        }
        public EDirection EdgeDirection
        {
            get { return m_EdgeDirection; }
            set
            {
                m_EdgeDirection = value;
                foreach (var item in m_List)
                {
                    item.EdgeDirection = m_EdgeDirection;
                }
            }
        }
        //public double SegmentLength
        //{
        //    get { return m_SegmentLength; }
        //    set
        //    {
        //        m_SegmentLength = value;
        //        for(int i = 0; i < Count; i++)
        //        {
        //            this[i].Region.Pose.TranslateX = (i + 0.5) * m_SegmentLength;
        //        }
        //    }
        //}
        #endregion

        public CvsEdgeSettingCollection()
        {
            m_List = new List<CvsEdgeSetting>();
            m_Pose = new CvsPose();
        }

        #region Methods
        public void SetParentPose(CvsPose parent)
        {
            m_Pose = parent;
            foreach (var item in m_List)
            {
                item.Region.Pose.Parent = m_Pose;
            }
        }
        public void SetWholeEdgeSetting(CvsEdgeSetting setting)
        {
            foreach (var item in m_List)
            {
                item.EdgeDirection = m_EdgeDirection = setting.EdgeDirection;
                item.ContrastThreshold = m_ContrastThreshold = setting.ContrastThreshold;
                item.HalfPixelCount = m_HalfPixelCount = setting.HalfPixelCount;
                item.SearchLength = m_SearchLength = setting.SearchLength;
                item.ProjectionLength = m_ProjectionLength = setting.ProjectionLength;
            }
        }

        public void Add(CvsEdgeSetting item)
        {
            item.Region.Pose.Parent = m_Pose;
            item.EdgeDirection = m_EdgeDirection;
            item.ContrastThreshold = m_ContrastThreshold;
            item.HalfPixelCount = m_HalfPixelCount;
            item.SearchLength = m_SearchLength;
            item.ProjectionLength = m_ProjectionLength;
            m_List.Add(item);
        }

        public void Clear()
        {
            m_List.Clear();
        }

        public bool Contains(CvsEdgeSetting item)
        {
            return m_List.Contains(item);
        }

        public void CopyTo(CvsEdgeSetting[] array, int arrayIndex)
        {
            m_List.CopyTo(array, arrayIndex);
        }

        public IEnumerator<CvsEdgeSetting> GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        public int IndexOf(CvsEdgeSetting item)
        {
            return m_List.IndexOf(item);
        }

        public void Insert(int index, CvsEdgeSetting item)
        {
            item.Region.Pose.Parent = m_Pose;
            item.EdgeDirection = m_EdgeDirection;
            item.ContrastThreshold = m_ContrastThreshold;
            item.HalfPixelCount = m_HalfPixelCount;
            item.SearchLength = m_SearchLength;
            item.ProjectionLength = m_ProjectionLength;
            m_List.Insert(index, item);
        }

        public bool Remove(CvsEdgeSetting item)
        {
            return m_List.Remove(item);
        }

        public void RemoveAt(int index)
        {
            m_List.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (m_List is IEnumerable e) return e.GetEnumerator();
            else return null;
        }
        #endregion


    }
}
