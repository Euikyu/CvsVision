using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CvsVision.Caliper
{
    /// <summary>
    /// 주어진 이미지 내에서 에지 쌍을 찾는 클래스입니다.
    /// </summary>
    public class CvsBreadthDetect
    {
        #region Fields
        private float[] m_SubPixelArray;
        private float[] m_ProjectionArray;

        private Bitmap m_DetectImage;
        private byte[] m_DetectRawImage;
        private int m_Width;
        private int m_Height;

        private List<CvsEdge> m_EdgeList;
        private List<CvsBreadth> m_BreadthList;
        private uint m_HalfPixelCount;
        #endregion

        #region Properties
        /// <summary>
        /// 투사한 배열의 변화량을 측정한 배열을 가져옵니다.
        /// </summary>
        public float[] SubPixelArray
        {
            get
            {
                if (m_SubPixelArray == null) this.CalculateSubPixelArray();
                return m_SubPixelArray;
            }
        }
        /// <summary>
        /// 2차원의 이미지를 1차원 배열로 투사한 배열을 가져옵니다.
        /// </summary>
        public float[] ProjectionArray
        {
            get
            {
                if (m_ProjectionArray == null) this.CalculateProjectionArray();
                return m_ProjectionArray;
            }
        }

        /// <summary>
        /// 검사할 이미지를 가져오거나 설정합니다.
        /// </summary>
        public Bitmap DetectImage
        {
            get
            {
                return m_DetectImage;
            }

            set
            {
                if (value.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                {
                    m_Width = value.Width;
                    m_Height = value.Height;
                    var bdata = value.LockBits(new Rectangle(0, 0, m_Width, m_Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    m_ProjectionArray = null;
                    m_SubPixelArray = null;
                    m_DetectRawImage = new byte[m_Width * m_Height];
                    Marshal.Copy(bdata.Scan0, m_DetectRawImage, 0, m_Width * m_Height);
                    value.UnlockBits(bdata);
                    m_DetectImage = value;
                }
            }
        }

        /// <summary>
        /// 에지로 인식할 절반 픽셀 개수를 가져오거나 설정합니다.
        /// </summary>
        public uint HalfPixelCount
        {
            get { return m_HalfPixelCount; }
            set
            {
                if (value == 0) m_HalfPixelCount = 1;
                else m_HalfPixelCount = value;
            }
        }
        /// <summary>
        /// 특정 이상의 변화량을 에지로 판단하기 위한 임계값을 가져오거나 설정합니다. 
        /// </summary>
        public uint ContrastThreshold { get; set; }
        /// <summary>
        /// 에지를 감지할 방향을 가져오거나 설정합니다.
        /// </summary>
        public EDirection Edge0Direction { get; set; }
        /// <summary>
        /// 에지를 감지할 방향을 가져오거나 설정합니다.
        /// </summary>
        public EDirection Edge1Direction { get; set; }
        /// <summary>
        /// 에지 쌍의 목표 너비를 가져오거나 설정합니다.
        /// </summary>
        public double TargetBreadth { get; set; }
        /// <summary>
        /// 검색한 에지 쌍을 가져옵니다.
        /// </summary>
        public CvsBreadth Breadth
        {
            get
            {
                if (m_BreadthList != null && m_BreadthList.Count > 0) return m_BreadthList.First();
                else return null;
            }
        }
        #endregion
        /// <summary>
        /// 에지를 찾는 클래스를 생성합니다.
        /// </summary>
        public CvsBreadthDetect()
        {
            //초기 설정값
            Edge0Direction = EDirection.Any;
            Edge1Direction = EDirection.Any;
            ContrastThreshold = 5;
            HalfPixelCount = 2;
            m_EdgeList = new List<CvsEdge>();
        }

#pragma warning disable CS1591 // 공개된 형식 또는 멤버에 대한 XML 주석이 없습니다.
        public void Dispose()
#pragma warning restore CS1591 // 공개된 형식 또는 멤버에 대한 XML 주석이 없습니다.
        {
            if (m_DetectImage != null)
            {
                m_DetectImage.Dispose();
                m_DetectRawImage = null;
            }
            if (m_EdgeList != null) m_EdgeList.Clear();
            m_ProjectionArray = null;
            m_SubPixelArray = null;
        }

        
        #region Methods
        /// <summary>
        /// 이미지를 1차원으로 투사합니다.
        /// </summary>
        public void CalculateProjectionArray()
        {
            //이미지 없음
            if (m_DetectRawImage == null) throw new Exception("Image select first.");

            //width 방향으로 투사
            //그래서 배열의 크기는 높이가 됨.
            m_ProjectionArray = new float[m_Height];

            //이미지 투사
            for (int row = 0; row < m_Height; row++)
            {
                int sum = 0;
                for (int col = 0; col < m_Width; col++)
                {
                    //width 픽셀 더하기
                    sum += m_DetectRawImage[row * m_Width + col];
                }
                // 투사값은 width 픽셀 합의 평균
                m_ProjectionArray[row] = (float)sum / m_Width;
            }
        }
        /// <summary>
        /// 투사된 배열의 각 변화량을 계산합니다.
        /// </summary>
        public void CalculateSubPixelArray()
        {
            //이미지 투사가 진행되지 않았다면 선행
            if (m_ProjectionArray == null) this.CalculateProjectionArray();

            // 서브픽셀 배열도 프로젝션 배열과 같은 크기
            m_SubPixelArray = new float[m_Height];

            for (int i = 0; i < m_Height; i++)
            {
                //처음 값과 마지막 값은 항상 0
                if (i < HalfPixelCount || i >= m_Height - HalfPixelCount)
                {
                    m_SubPixelArray[i] = 0;
                    continue;
                }
                // 내 값을 기준으로 앞 뒤 값의 차를 구하여 변화량 측정
                float preValue = 0;
                float postValue = 0;

                for (int range = 1; range <= HalfPixelCount; range++)
                {
                    preValue += m_ProjectionArray[i - range];
                    postValue += m_ProjectionArray[i + range];
                }

                m_SubPixelArray[i] = (preValue - postValue) / HalfPixelCount;
            }
        }
        /// <summary>
        /// 이미지의 에지를 검색합니다.
        /// </summary>
        public void Detect()
        {
            //현재 에지 탐색 로직은 불완전함, 추후에 보완할 예정
            //에지 넓이 = 최고점 높이 * 같은 부호 길이 / 2

            //이전 서브픽셀 값
            float lastValue = 0;
            //에지의 최대 대비값
            float lastMaxValue = 0;

            //현재 서브픽셀 변화량의 합
            float currentSum_dx = 0;
            //현재 (서브픽셀 변화량 * 현재 픽셀 위치) 의 합
            float currentSum_y_mul_dx = 0;

            //에지후보 list 초기화
            m_EdgeList.Clear();

            //서브픽셀 배열 개수
            int count = SubPixelArray.Length;

            //서브픽셀 한 바퀴 도는 동안
            for (int i = 0; i < count; i++)
            {
                // 현재 서브픽셀 값이 양수라면?
                if (m_SubPixelArray[i] > 0)
                {
                    //기존값과 부호가 반대라면
                    if (lastValue < 0)
                    {
                        //현재합으로 에지 계산 -> null일 경우 에지 추가 안함
                        var res = this.CalculateEdgeByEdgeDirection(currentSum_dx, currentSum_y_mul_dx);
                        //에지 추가할 때, X, Y, 대비값 추가
                        if (res != null) m_EdgeList.Add(new CvsEdge(0, (float)res - m_Height / 2, lastMaxValue, currentSum_dx));
                        currentSum_dx = 0;
                        currentSum_y_mul_dx = 0;
                        lastMaxValue = 0;
                    }
                    //기존값과 부호가 같다면
                    else
                    {
                        // 대비 임계값을 넘어선 수치라면 현재 합에 더해줌
                        if (Math.Abs(m_SubPixelArray[i]) > ContrastThreshold)
                        {
                            currentSum_dx += m_SubPixelArray[i];
                            currentSum_y_mul_dx += i * m_SubPixelArray[i];
                        }
                    }
                }
                // 현재 서브픽셀 값이 음수라면? 양수와 반대로 동작함. -> 동작원리는 같음
                else if (m_SubPixelArray[i] < 0)
                {
                    if (lastValue > 0)
                    {
                        //현재합으로 에지 계산 -> 0일 경우 에지 추가 안함
                        var res = this.CalculateEdgeByEdgeDirection(currentSum_dx, currentSum_y_mul_dx);
                        //에지 추가할 때, X, Y, 대비값 추가
                        if (res != null) m_EdgeList.Add(new CvsEdge(0, (float)res - m_Height / 2, lastMaxValue, currentSum_dx));
                        currentSum_dx = 0;
                        currentSum_y_mul_dx = 0;
                        lastMaxValue = 0;
                    }
                    else
                    {
                        if (Math.Abs(m_SubPixelArray[i]) > ContrastThreshold)
                        {
                            currentSum_dx += m_SubPixelArray[i];
                            currentSum_y_mul_dx += i * m_SubPixelArray[i];
                        }
                    }
                }
                //변화량이 0이라면?
                else
                {
                    if (lastValue < 0)
                    {
                        //현재합으로 에지 계산 -> 0일 경우 에지 추가 안함
                        var res = this.CalculateEdgeByEdgeDirection(currentSum_dx, currentSum_y_mul_dx);
                        //에지 추가할 때, X, Y, 대비값 추가
                        if (res != null) m_EdgeList.Add(new CvsEdge(0, (float)res - m_Height / 2, lastMaxValue, currentSum_dx));
                        currentSum_dx = 0;
                        currentSum_y_mul_dx = 0;
                        lastMaxValue = 0;
                    }
                    else if (lastValue > 0)
                    {
                        //현재합으로 에지 계산 -> 0일 경우 에지 추가 안함
                        var res = this.CalculateEdgeByEdgeDirection(currentSum_dx, currentSum_y_mul_dx);
                        //에지 추가할 때, X, Y, 대비값 추가
                        if (res != null) m_EdgeList.Add(new CvsEdge(0, (float)res - m_Height / 2, lastMaxValue, currentSum_dx));
                        currentSum_dx = 0;
                        currentSum_y_mul_dx = 0;
                        lastMaxValue = 0;
                    }
                }
                //다음 제품의 임계값
                lastValue = m_SubPixelArray[i];
                if (Math.Abs(m_SubPixelArray[i]) > Math.Abs(lastMaxValue)) lastMaxValue = m_SubPixelArray[i];
            }

            this.SelectEdge();
            this.ScoringAndSortBreadthList();
        }
        

        private void SelectEdge()
        {
            m_BreadthList = new List<CvsBreadth>();

            for(int i = 0; i < m_EdgeList.Count - 1; i++)
            {
                if (Edge0Direction == EDirection.DarkToLight && m_EdgeList[i].Sum > 0) continue;
                if (Edge0Direction == EDirection.LightToDark && m_EdgeList[i].Sum < 0) continue;
                for (int j = i + 1; j < m_EdgeList.Count; j++)
                {
                    if (Edge1Direction == EDirection.DarkToLight && m_EdgeList[j].Sum > 0) continue;
                    if (Edge1Direction == EDirection.LightToDark && m_EdgeList[j].Sum < 0) continue;
                    var dist = Math.Abs(m_EdgeList[i].Y - m_EdgeList[j].Y);
                    m_BreadthList.Add(new CvsBreadth(m_EdgeList[i], m_EdgeList[j], dist));
                }
            }
        }

        /// <summary>
        /// 폭 점수 비교하여 정렬하는 함수.
        /// </summary>
        private void ScoringAndSortBreadthList()
        {
            if (TargetBreadth < 0) m_BreadthList.Sort((i1, i2) => (Math.Abs(i2.Edge0.Sum) + Math.Abs(i2.Edge1.Sum)).CompareTo(Math.Abs(i1.Edge0.Sum) + Math.Abs(i1.Edge1.Sum)));
            else m_BreadthList.Sort((i1, i2) => Math.Abs(i1.Distance - TargetBreadth).CompareTo(Math.Abs(i2.Distance - TargetBreadth)));
        }
        /// <summary>
        /// 에지 방향 결정하는 함수.
        /// </summary>
        /// <param name="sum_dx">변화량의 합.</param>
        /// <param name="sum_y_mul_dx">(변화량 * 픽셀 위치) 의 합.</param>
        /// <returns></returns>
        private float? CalculateEdgeByEdgeDirection(float sum_dx, float sum_y_mul_dx)
        {
            //반환값이 null일 경우 에지 추가 안함
            if (sum_dx == 0) return null;
            return sum_y_mul_dx / sum_dx - 0.5F;
           
        }
        #endregion
    }
    /// <summary>
    /// 에지 쌍 클래스입니다.
    /// </summary>
    public class CvsBreadth
    {
        #region Fields

        #endregion

        #region Properties
        /// <summary>
        /// 에지 0에 대한 데이터를 가져옵니다.
        /// </summary>
        public CvsEdge Edge0 { get; }
        /// <summary>
        /// 에지 1에 대한 데이터를 가져옵니다.
        /// </summary>
        public CvsEdge Edge1 { get; }
        /// <summary>
        /// 두 에지의 거리를 가져옵니다.
        /// </summary>
        public double Distance { get; }
        #endregion

        /// <summary>
        /// 에지 쌍 클래스를 생성합니다.
        /// </summary>
        /// <param name="edge0">에지 0 데이터.</param>
        /// <param name="edge1">에지 1 데이터.</param>
        /// <param name="dist">두 에지의 거리.</param>
        public CvsBreadth(CvsEdge edge0, CvsEdge edge1, double dist)
        {
            this.Edge0 = edge0;
            this.Edge1 = edge1;
            this.Distance = dist;
        }
    }
}
