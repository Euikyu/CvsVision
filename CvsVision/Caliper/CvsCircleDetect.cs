using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CvsVision.Caliper
{
    /// <summary>
    /// 주어진 점 집합 내에서 원을 찾는 클래스입니다.
    /// </summary>
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
        /// <summary>
        /// 점 집합의 원을 검색합니다.
        /// </summary>
        public void Detect()
        {
            //원 적합 구하기
            //RANSAC 방법으로 구하려고 함
            //1. 무작위 세 점을 선택하여 원을 구함
            //2. 이 원을 가지고 다른 점들 비교
            //3. 이 과정을 여러번 실행
            //4. 동의하는 점이 가장 많은 모델 선정
            //5. 그 모델의 최소자승법을 통한 원 모델 선정 -> 시간 많이 소요됨
            //6. 최소외접원 찾는 알고리즘 응용하여 구현

            this.CalcModels();

            this.ScoringRANSACModel();
            
            //m_SelectedRANSAC = m_RANSAC_Models.First();
            m_SelectedRANSAC = this.CalcMinimumCoveringCircle(m_RANSAC_Models.First().ConsensusPoints);
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
        /// <summary>
        /// 최소자승법으로 원 찾기.
        /// </summary>
        /// <param name="points">점 집합.</param>
        /// <returns></returns>
        private CvsCircle CalcLeastSquare(IEnumerable<Point> points)
        {
            // 계산방법
            // 1. 모든 수직이등분선의 교점을 구한다.
            // 2. 교점의 평균 값을 구하여 원 중점으로 설정한다.
            // 3. 중점에서 각 점 간의 거리를 구한다.
            // 4. 거리의 평균을 반지름으로 설정한다.

            double sum_x = 0, sum_y = 0;
            int tmpCount = 0;
            
            //수직이등분선 구하기.
            for (int i = 0; i < points.Count() - 2; i++)
            {
                for (int j = i + 1; j < points.Count() - 1; j++)
                {
                    m_CornerDetect.LineA = this.CalcBisector(points.ElementAt(i), points.ElementAt(j));
                    for (int k = j + 1; k < points.Count(); k++)
                    {
                        m_CornerDetect.LineB = this.CalcBisector(points.ElementAt(i), points.ElementAt(k));
                        m_CornerDetect.Detect();

                        if(m_CornerDetect.Corner != null && m_CornerDetect.Corner.IntersectionAngle != 0)
                        {
                            sum_x += m_CornerDetect.Corner.Corner.X;
                            sum_y += m_CornerDetect.Corner.Corner.Y;
                            tmpCount++;
                        }
                    }
                }
            }

            //중심점 구하기
            var center = new Point(sum_x / tmpCount, sum_y / tmpCount);
            
            //중심점에서 거리 측정
            double sum_r = 0;
            foreach (var p in points)
            {
                sum_r += this.CalcRadius(center, p);
            }

            //반지름 구하기
            var radius = sum_r / points.Count();

            return new CvsCircle(center, radius, points is Point[] ? (Point[])points : points.ToArray());
        }

        /// <summary>
        /// 최소외접원으로 점 찾기.
        /// </summary>
        /// <param name="points">점 집합.</param>
        /// <returns></returns>
        private CvsCircle CalcMinimumCoveringCircle(IEnumerable<Point> points)
        {
            // 계산방법
            // 1. 각 점들의 수직이등분선을 구한다.
            // 2. 수직이등분선들의 교점을 구한다.
            // 3. 각 교점 간의 중심점을 구한다.
            // 4. 중심점들의 평균값을 원 중점으로 설정한다.
            // 5. 중점에서 각 점 간의 거리를 구한다.
            // 6. 거리의 평균을 반지름으로 설정한다.

            double sum_x = 0, sum_y = 0;
            int tmpCount = points.Count() / 2;

            List<CvsLine> lines = new List<CvsLine>();
            
            for(int i = 0; i < tmpCount; i++)
            {
                if (2 * i + 1 >= points.Count()) break;
                lines.Add(this.CalcBisector(points.ElementAt(2 * i), points.ElementAt(2 * i + 1)));
            }

            lines.Sort((i1, i2) => double.IsNaN(i1.Gradient) ? 1 : (double.IsNaN(i2.Gradient) ? -1 : i1.Gradient.CompareTo(i2.Gradient)));

            List<Point> centers = new List<Point>();

            for (int i = 0; i < lines.Count / 2; i++)
            {
                m_CornerDetect.LineA = lines[i];
                m_CornerDetect.LineB = lines[lines.Count - i - 1];
                m_CornerDetect.Detect();

                centers.Add(m_CornerDetect.Corner.Corner);
            }
            centers.Sort((i1, i2) => Math.Sqrt(Math.Pow(i1.X, 2) + Math.Pow(i1.Y, 2)).CompareTo(Math.Sqrt(Math.Pow(i2.X, 2) + Math.Pow(i2.Y, 2))));



            List<Point> realCenters = new List<Point>();


            for (int i = 0; i < centers.Count / 2; i++)
            {
                var p0 = centers[i];
                var p1 = centers[centers.Count - i - 1];
                realCenters.Add(new Point((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2));
            }

            foreach(var c in realCenters)
            {
                sum_x += c.X;
                sum_y += c.Y;
            }
            
            var center = new Point(sum_x / realCenters.Count, sum_y / realCenters.Count);
            
            double sum_r = 0;
            foreach (var p in points)
            {
                sum_r += this.CalcRadius(center, p);
            }

            var radius = sum_r / points.Count();

            return new CvsCircle(center, radius, points is Point[] ? (Point[])points : points.ToArray());
        }
        

        /// <summary>
        /// 점 집합 내에서 무작위 세 점 구하기
        /// </summary>
        /// <param name="points">점 집합.</param>
        /// <param name="random">난수 변수.</param>
        /// <param name="p0">첫 번째 점.</param>
        /// <param name="p1">두 번째 점.</param>
        /// <param name="p2">세 번째 점.</param>
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

        /// <summary>
        /// 두 점의 수직 이등분 선 구하기.
        /// </summary>
        /// <param name="p0">첫 번째 점.</param>
        /// <param name="p1">두 번째 점.</param>
        /// <returns></returns>
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

        /// <summary>
        /// 반지름 구하기.
        /// </summary>
        /// <param name="center">원의 중심점.</param>
        /// <param name="p0">원 위의 한 점.</param>
        /// <returns></returns>
        private double CalcRadius(Point center, Point p0)
        {
            return Math.Sqrt(Math.Pow(center.X - p0.X, 2) + Math.Pow(center.Y - p0.Y, 2));
        }

        /// <summary>
        /// 해당 원의 호 위에 존재하는 점 구하기.
        /// </summary>
        /// <param name="points">총 점 집합.</param>
        /// <param name="p0">첫 번째 점.</param>
        /// <param name="p1">두 번째 점.</param>
        /// <param name="p2">세 번째 점.</param>
        /// <param name="ConsensusThreshold">호 위에 </param>
        /// <returns></returns>
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
