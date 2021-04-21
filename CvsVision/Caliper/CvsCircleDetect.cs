using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CvsVision.Caliper
{
    public class CvsCircleDetect : IDisposable
    {
        #region Fields
        private Random m_Rand = new Random();
        private CvsCircle m_SelectedRANSAC;
        private List<CvsCircle> m_RANSAC_Models;

        private CvsCornerDetect m_CornerDetect;
        #endregion

        #region Properties
        /// <summary>
        /// 원을 구할 점 집합을 가져오거나 설정합니다.
        /// </summary>
        public List<Point> InputPointList { get; set; }
        /// <summary>
        /// 원의 선상으로 인정되는 범위 값을 가져오거나 설정합니다.
        /// </summary>
        public double ConsensusThreshold { get; set; }
        /// <summary>
        /// 구한 원 결과를 가져옵니다.
        /// </summary>
        public CvsCircle Circle { get { return m_SelectedRANSAC; } }
        #endregion
        /// <summary>
        /// 원을 찾는 클래스를 생성합니다.
        /// </summary>
        public CvsCircleDetect()
        {
            m_CornerDetect = new CvsCornerDetect();
            this.InputPointList = new List<Point>();
            this.ConsensusThreshold = 6;
        }

        /// <summary>
        /// 입력한 점 집합의 적합원을 찾는 클래스를 생성합니다.
        /// </summary>
        /// <param name="InputPoints">적합원을 찾을 점 집합.</param>
        public CvsCircleDetect(List<Point> InputPoints)
        {
            m_CornerDetect = new CvsCornerDetect();
            this.InputPointList = InputPoints.ToList();
            this.ConsensusThreshold = 6;
        }

        public void Dispose()
        {
            if (InputPointList != null) InputPointList.Clear();
        }

        #region Methods

        public void Detect()
        {
            //원 적합 구하기
            //RANSAC 방법으로 구하려고 함
            //1. 무작위 세 점을 선택하여 원을 구함
            //2. 이 원을 가지고 다른 점들 비교
            //3. 이 과정을 여러번 실행
            //4. 동의하는 점이 가장 많은 모델 선정

            this.CalcModels();

            this.ScoringRANSACModel();

            m_SelectedRANSAC = m_RANSAC_Models.First();
        }

        /// <summary>
        /// 모델을 구하기.
        /// </summary>
        private void CalcModels()
        {
            try
            {
                m_RANSAC_Models = new List<CvsCircle>();
                for (int i = 0; i < 12; i++)
                {
                    this.SelectCirclePoint(InputPointList, m_Rand, out Point p0, out Point p1, out Point p2);
                    var model = this.CalcConsensusPoints(this.InputPointList, p0, p1, p2, this.ConsensusThreshold);
                    if(model != null) this.m_RANSAC_Models.Add(model);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SelectCirclePoint(List<Point> points, Random random, out Point p0, out Point p1, out Point p2)
        {
            if (points.Count < 3)
                throw new Exception("The point must have at least 3.");

            int start, end, size, rand_num1, rand_num2, rand_num3;
            size = points.Count;
            start = 0;
            end = size - 1;


            if (points.Count == 3)
            {
                rand_num1 = 0;
                rand_num2 = 1;
                rand_num3 = 2;
            }
            else
            {
                rand_num1 = random.Next(start, end);
                do
                {
                    rand_num2 = random.Next(start, end);
                } while (rand_num1 == rand_num2);
                do
                {
                    rand_num3 = random.Next(start, end);
                } while (rand_num3 == rand_num1 || rand_num3 == rand_num2);
            }

            // 난수를 인덱스로 사용해서 p0, p1, p2 구해
            p0 = points[rand_num1];
            p1 = points[rand_num2];
            p2 = points[rand_num3];
        }

        private CvsLine CalcBisector(Point p0, Point p1)
        {
            //수직 이등분 선의 방정식 (x1 - x0)(x - (x1 + x0) / 2) + (y1 - y0)(y - (y1 + y0) / 2) = 0;
            //직선의 방정식 y = ax + b;
            //직선의 방정식 형태로 변경하면
            // y = (x0 - x1) / (y1 - y0)x + (x1 - x0) * (x1 + x0) / (2 * (y1 - y0)) + (y1 + y0) / 2;
            // a = (x0 - x1) / (y1 - y0),  b = (- a * (x1 + x0) + (y1 + y0)) / 2;
            var gradient = p1.Y == p0.Y ? double.NaN : (p0.X - p1.X) / (p1.Y - p0.Y);
            var y_intercept = p1.Y == p0.Y ? p0.Y : (-gradient * (p0.X + p1.X) + (p0.Y + p1.Y)) / 2;

            return new CvsLine(new Point(0, y_intercept), new Point(1, gradient + y_intercept), gradient, y_intercept, null);
        }

        private double CalcRadius(Point center, Point p0)
        {
            return Math.Sqrt(Math.Pow(center.X - p0.X, 2) + Math.Pow(center.Y - p0.Y, 2));
        }

        private CvsCircle CalcConsensusPoints(List<Point> points, Point p0, Point p1, Point p2, double ConsensusThreshold)
        {
            try
            {
                m_CornerDetect.LineA = this.CalcBisector(p0, p1);
                m_CornerDetect.LineB = this.CalcBisector(p0, p2);
                m_CornerDetect.Detect();

                if (m_CornerDetect.Corner == null || m_CornerDetect.Corner.IntersectionAngle == 0) return null;

                var rad = this.CalcRadius(m_CornerDetect.Corner.Corner, p0);

                List<Point> consensusPointList = new List<Point>();

                foreach(var p in points)
                {
                    var dist = Math.Abs(rad - this.CalcRadius(m_CornerDetect.Corner.Corner, p));
                    if (dist <= ConsensusThreshold) consensusPointList.Add(p);
                }

                return new CvsCircle(m_CornerDetect.Corner.Corner, rad, consensusPointList.ToArray());
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 선 결과들 비교하여 정렬하기.
        /// </summary>
        private void ScoringRANSACModel()
        {
            if (m_RANSAC_Models == null || m_RANSAC_Models.Count == 0) throw new Exception("The segment could not be found.");
            m_RANSAC_Models.Sort((i1, i2) => i2.ConsensusPoints.Length.CompareTo(i1.ConsensusPoints.Length));
        }
        #endregion
    }

    public class CvsCircle
    {
        #region Properties
        /// <summary>
        /// 원의 중심점을 가져옵니다.
        /// </summary>
        public Point Center { get; }
        /// <summary>
        /// 반지름 값을 가져옵니다.
        /// </summary>
        public double Radius { get; }
        /// <summary>
        /// 해당 적합원의 선상에 있는 점 집합을 가져옵니다.
        /// </summary>
        public Point[] ConsensusPoints { get; }
        #endregion
        
        public CvsCircle(Point center, double radius, Point[] consensusPoints)
        {
            this.Center = center;
            this.Radius = radius;
            this.ConsensusPoints = consensusPoints;
        }
    }
}
