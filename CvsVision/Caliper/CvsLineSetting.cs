﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvsVision.Caliper
{
    /// <summary>
    /// 선을 찾기 위한 설정 값 클래스입니다.
    /// </summary>
    public class CvsLineSetting
    {
        #region Fields
        private CvsLineDetect m_LineDetect;
        private CvsEdgeSetting m_EdgeSetting;
        #endregion

        #region Properties

        #region Line Settings
        /// <summary>
        /// 선 상으로 인정되는 범위 값을 가져오거나 설정합니다.
        /// </summary>
        public double ConsensusThreshold
        {
            get
            {
                if (m_LineDetect != null) return m_LineDetect.ConsensusThreshold;
                else return 0;
            }
            set { if (m_LineDetect != null) m_LineDetect.ConsensusThreshold = value; }
        }
        /// <summary>
        /// 선에 대한 Pose 값을 가져오거나 설정합니다.
        /// </summary>
        public CvsPose LinePose { get; set; }
        /// <summary>
        /// 선 모델의 원점 X 좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginX
        {
            get
            {
                if (LinePose != null) return LinePose.TranslateX;
                else return 0;
            }
            set { if (LinePose != null) LinePose.TranslateX = value; }
        }
        /// <summary>
        /// 선 모델의 원점 Y 좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginY
        {
            get
            {
                if (LinePose != null) return LinePose.TranslateY;
                else return 0;
            }
            set { if (LinePose != null) LinePose.TranslateY = value; }
        }
        /// <summary>
        /// 선 모델의 회전 라디안 값을 가져오거나 설정합니다.
        /// </summary>
        public double Radian
        {
            get
            {
                if (LinePose != null) return LinePose.Radian;
                else return 0;
            }
            set { if (LinePose != null) LinePose.Radian = value; }
        }
        /// <summary>
        /// 선 모델의 캘리퍼 개수를 가져옵니다.
        /// </summary>
        public int CaliperCount { get; set; }
        /// <summary>
        /// 선 모델의 선분 길이를 가져오거나 설정합니다.
        /// </summary>
        public double SegmentLength { get; set; }
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
        /// 선을 찾기 위한 설정 값 클래스를 생성합니다.
        /// </summary>
        public CvsLineSetting()
        {
            LinePose = new CvsPose();
            m_LineDetect = new CvsLineDetect();
            m_EdgeSetting = new CvsEdgeSetting();
        }
        #region Methods
        /// <summary>
        /// 현재 설정을 가진 선 검색 클래스를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public CvsLineDetect GetToolParams()
        {
            return m_LineDetect;
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

    /// <summary>
    /// 에지 검색 설정 집합 클래스입니다.
    /// </summary>
    public class CvsEdgeSettingCollection : IEnumerable<CvsEdgeSetting>
    {
        #region Fields
        List<CvsEdgeSetting> m_List;
        private CvsEdgeSetting m_EdgeSetting;
        private CvsPose m_Pose;
        #endregion

        #region Properties
        /// <summary>
        /// 지정한 인덱스에 있는 요소를 가져오거나 설정합니다.
        /// </summary>
        /// <param name="index">가져오거나 설정할 요소의 인덱스(0부터 시작).</param>
        /// <returns></returns>
        public CvsEdgeSetting this[int index] { get { return m_List[index]; } set { m_List[index] = value; } }
        /// <summary>
        /// 현재 집합의 요소 수를 가져옵니다.
        /// </summary>
        public int Count { get { return m_List.Count; } }
        /// <summary>
        /// 각 에지의 투사 길이를 가져오거나 설정합니다.
        /// (영역의 너비입니다.)
        /// </summary>
        public double ProjectionLength
        {
            get { return m_EdgeSetting.ProjectionLength; }
            set
            {
                m_EdgeSetting.ProjectionLength = value;
                foreach (var item in m_List)
                {
                    item.ProjectionLength = m_EdgeSetting.ProjectionLength;
                }
            }
        }
        /// <summary>
        /// 각 에지의 검색 길이를 가져오거나 설정합니다.
        /// (영역의 높이입니다.)
        /// </summary>
        public double SearchLength
        {
            get { return m_EdgeSetting.SearchLength; }
            set
            {
                m_EdgeSetting.SearchLength = value;
                foreach (var item in m_List)
                {
                    item.SearchLength = m_EdgeSetting.SearchLength;
                }
            }
        }
        /// <summary>
        /// 각 에지의 절반 픽셀 크기를 가져오거나 설정합니다.
        /// </summary>
        public uint HalfPixelCount
        {
            get { return m_EdgeSetting.HalfPixelCount; }
            set
            {
                m_EdgeSetting.HalfPixelCount = value;
                foreach (var item in m_List)
                {
                    item.HalfPixelCount = m_EdgeSetting.HalfPixelCount;
                }
            }
        }
        /// <summary>
        /// 각 에지의 대비 임계값을 가져오거나 설정합니다.
        /// </summary>
        public uint ContrastThreshold
        {
            get { return m_EdgeSetting.ContrastThreshold; }
            set
            {
                m_EdgeSetting.ContrastThreshold = value;
                foreach (var item in m_List)
                {
                    item.ContrastThreshold = m_EdgeSetting.ContrastThreshold;
                }
            }
        }
        /// <summary>
        /// 각 에지를 감지할 방향을 가져오거나 설정합니다.
        /// </summary>
        public EDirection EdgeDirection
        {
            get { return m_EdgeSetting.EdgeDirection; }
            set
            {
                m_EdgeSetting.EdgeDirection = value;
                foreach (var item in m_List)
                {
                    item.EdgeDirection = m_EdgeSetting.EdgeDirection;
                }
            }
        }
        #endregion
        /// <summary>
        /// 에지 설정 집합 클래스를 생성합니다.
        /// </summary>
        public CvsEdgeSettingCollection()
        {
            m_List = new List<CvsEdgeSetting>();
            m_EdgeSetting = new CvsEdgeSetting();
            m_Pose = new CvsPose();
        }

        #region Methods
        /// <summary>
        /// 에지 집합들의 상위 Pose를 설정합니다.
        /// </summary>
        /// <param name="parent">부모가 될 Pose.</param>
        public void SetParentPose(CvsPose parent)
        {
            m_Pose = parent;
            foreach (var item in m_List)
            {
                item.Region.Pose.Parent = m_Pose;
            }
        }
        /// <summary>
        /// 에지 집합의 값들을 일체 설정합니다.
        /// </summary>
        /// <param name="setting">일체 설정할 설정 값.</param>
        public void SetWholeEdgeSetting(CvsEdgeSetting setting)
        {
            m_EdgeSetting = setting;
            foreach (var item in m_List)
            {
                item.EdgeDirection = m_EdgeSetting.EdgeDirection;
                item.ContrastThreshold = m_EdgeSetting.ContrastThreshold;
                item.HalfPixelCount = m_EdgeSetting.HalfPixelCount;
                item.SearchLength = m_EdgeSetting.SearchLength;
                item.ProjectionLength = m_EdgeSetting.ProjectionLength;
            }
        }
        /// <summary>
        /// 개체를 집합 끝 부분에 추가합니다.
        /// </summary>
        /// <param name="item">추가할 요소.</param>
        public void Add(CvsEdgeSetting item)
        {
            item.Region.Pose.Parent = m_Pose;
            item.EdgeDirection = m_EdgeSetting.EdgeDirection;
            item.ContrastThreshold = m_EdgeSetting.ContrastThreshold;
            item.HalfPixelCount = m_EdgeSetting.HalfPixelCount;
            item.SearchLength = m_EdgeSetting.SearchLength;
            item.ProjectionLength = m_EdgeSetting.ProjectionLength;
            m_List.Add(item);
        }
        /// <summary>
        /// 현재 집합에서 모든 요소를 제거합니다.
        /// </summary>
        public void Clear()
        {
            m_List.Clear();
        }
        /// <summary>
        /// 현재 집합에 해당 요소가 있는 지 확인합니다.
        /// </summary>
        /// <param name="item">확인할 요소.</param>
        /// <returns></returns>
        public bool Contains(CvsEdgeSetting item)
        {
            return m_List.Contains(item);
        }
        /// <summary>
        /// 호환되는 대상 1차원 배열에 현재 집합의 처음부터 끝까지 복사합니다.
        /// </summary>
        /// <param name="array">복사한 요소의 배열.</param>
        public void CopyTo(CvsEdgeSetting[] array)
        {
            m_List.CopyTo(array);
        }
        /// <summary>
        /// 호환되는 대상 1차원 배열의 지정한 배열 인덱스부터 시작하여 현재 집합의 처음부터 끝까지 복사합니다.
        /// </summary>
        /// <param name="array">복사한 요소의 배열.</param>
        /// <param name="arrayIndex">배열에 복사가 시작되는 인덱스.</param>
        public void CopyTo(CvsEdgeSetting[] array, int arrayIndex)
        {
            m_List.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// 호환되는 대상 1차원 배열의 지정한 배열 인덱스부터 시작하여
        /// 현재 집합의 지정한 배열부터 지정한 개수만큼 복사합니다.
        /// </summary>
        /// <param name="startIndex">복사를 시작할 인덱스.</param>
        /// <param name="array">복사한 요소의 배열.</param>
        /// <param name="arrayIndex">배열에 복사가 시작되는 인덱스.</param>
        /// <param name="count">복사할 요소 수.</param>
        public void CopyTo(int startIndex, CvsEdgeSetting[] array, int arrayIndex, int count)
        {
            m_List.CopyTo(startIndex, array, arrayIndex,count);
        }
        /// <summary>
        /// 집합을 반복하는 열거자를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<CvsEdgeSetting> GetEnumerator()
        {
            return m_List.GetEnumerator();
        }
        /// <summary>
        /// 지정된 개체를 검색하고 전체 집합에서 처음 검색한 개체의 인덱스를 반환합니다.
        /// </summary>
        /// <param name="item">검색할 요소.</param>
        /// <returns></returns>
        public int IndexOf(CvsEdgeSetting item)
        {
            return m_List.IndexOf(item);
        }
        /// <summary>
        /// 집합의 지정된 인덱스에 요소를 삽입합니다.
        /// </summary>
        /// <param name="index">지정할 인덱스.</param>
        /// <param name="item">삽입할 요소.</param>
        public void Insert(int index, CvsEdgeSetting item)
        {
            item.Region.Pose.Parent = m_Pose;
            item.EdgeDirection = m_EdgeSetting.EdgeDirection;
            item.ContrastThreshold = m_EdgeSetting.ContrastThreshold;
            item.HalfPixelCount = m_EdgeSetting.HalfPixelCount;
            item.SearchLength = m_EdgeSetting.SearchLength;
            item.ProjectionLength = m_EdgeSetting.ProjectionLength;
            m_List.Insert(index, item);
        }
        /// <summary>
        /// 집합에서 맨 처음 발견되는 특정 개체를 제거합니다.
        /// </summary>
        /// <param name="item">제거할 요소.</param>
        /// <returns></returns>
        public bool Remove(CvsEdgeSetting item)
        {
            return m_List.Remove(item);
        }
        /// <summary>
        /// 집합에서 지정한 인덱스의 요소를 제거합니다.
        /// </summary>
        /// <param name="index">제거할 인덱스.</param>
        public void RemoveAt(int index)
        {
            m_List.RemoveAt(index);
        }
        /// <summary>
        /// 컬렉션을 반복하는 열거자를 반환합니다.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (m_List is IEnumerable e) return e.GetEnumerator();
            else return null;
        }
        #endregion


    }
}
