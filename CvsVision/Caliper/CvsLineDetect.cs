using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CvsVision.Caliper
{
    public class CvsLineDetect
    {
        #region Fields
        private Random m_Rand = new Random();
        private CvsLine m_SelectedRANSAC;
        private List<CvsLine> m_RANSAC_Models;
        #endregion

        #region Properties
        /// <summary>
        /// 선을 구할 점 집합을 가져오거나 설정합니다.
        /// </summary>
        public List<Point> InputPointList { get; set; }
        /// <summary>
        /// 선상으로 인정되는 범위 값을 가져오거나 설정합니다.
        /// </summary>
        public double ConsensusThreshold { get; set; }
        /// <summary>
        /// 구한 선 결과를 가져옵니다.
        /// </summary>
        public CvsLine Line { get { return m_SelectedRANSAC; } }
        #endregion

        // 생성자
        public CvsLineDetect()
        {
            this.InputPointList = new List<Point>();
            this.ConsensusThreshold = 6;
        }

        public CvsLineDetect(List<Point> InputPoints)
        {
            this.InputPointList = InputPoints.ToList();
            this.ConsensusThreshold = 6;
        }

        public void Detect()
        {
            // 모델들 구하기
            this.CalcModels();
            //모델 정확도에 따라 순서 나누기
            this.ScoringRANSACModel();

            //가장 정확도가 높은 모델을 선택
            m_SelectedRANSAC = m_RANSAC_Models.First();
        }

        // 모델 구하기
        private void CalcModels()
        {
            try
            {
                m_RANSAC_Models = new List<CvsLine>();
                for (int i = 0; i < 12; i++)
                {
                    SelectLinePoint(InputPointList, m_Rand, out Point sPoint, out Point ePoint);                    
                    this.m_RANSAC_Models.Add(CalcConsensusPoints(this.InputPointList, sPoint, ePoint, this.ConsensusThreshold));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        private void SelectLinePoint(List<Point> points, Random random, out Point startPoint, out Point endPoint)
        {
            try
            {
                if (points.Count <= 1)
                    throw new Exception("The point must have at least 2.");
                
                int start, end, size, rand_num1, rand_num2;
                size = points.Count;
                start = 0;
                end = size - 1;
                
                do
                {
                    if(points.Count == 2)
                    {
                        rand_num1 = 0;
                        rand_num2 = 1;
                        break;
                    }
                    rand_num1 = random.Next(start, end);
                    rand_num2 = random.Next(start, end);
                } while (rand_num1 == rand_num2);

                // 난수를 인덱스로 사용해서 p1, p2 구해
                startPoint = points[rand_num1];
                endPoint = points[rand_num2];                
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        private double CalcGradient(Point p1, Point p2)
        {
            return p2.X - p1.X == 0 ? double.NaN : (p2.Y - p1.Y) / (p2.X - p1.X);
        }

        private double CalcIntercept(Point p1, Point p2)
        {
            return p2.X - p1.X == 0 ? double.NaN : (p2.X * p1.Y - p1.X * p2.Y) / (p2.X - p1.X);
        }
        
        private CvsLine CalcConsensusPoints(List<Point> points, Point startPoint, Point endPoint, double ConsensusThreshold)
        {
            try
            {
                double distance = 0;
                double gradient = CalcGradient(startPoint, endPoint);
                double yIntercept = CalcIntercept(startPoint, endPoint);

                List<Point> consensusPointList = new List<Point>();

                for (int i = 0; i < points.Count; i++)
                {
                    double x1 = points[i].X;
                    double y1 = points[i].Y;
                    
                    if (endPoint.X == startPoint.X)
                    {
                        distance = Math.Abs(x1 - startPoint.X);
                    }
                    else if (endPoint.Y == startPoint.Y)
                    {
                        distance = Math.Abs(y1 - startPoint.Y);
                    }
                    else
                    {
                        // 점(ptOne.X,ptOne.Y)를 지나는 직선의 방정식 : mx-y-m*ptOne.X+ptOne.Y = 0

                        // =====점(x1,y1)과 직선(ax+by+c=0)의 거리 구하는 공식====
                        // |ax1+by1+c|
                        // ------------
                        // 루트(a제곱+b제곱)

                        if (double.IsNaN(gradient)) distance = Math.Abs(x1 - startPoint.X);
                        else distance = Math.Abs(x1 * gradient + (-1 * y1) + yIntercept) / Math.Sqrt(Math.Pow(gradient, 2) + 1);
                    }

                    if (distance < ConsensusThreshold)
                    {
                        consensusPointList.Add(points[i]);
                    }
                }

                return new CvsLine(startPoint, endPoint, gradient, yIntercept, consensusPointList.ToArray());
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        private void ScoringRANSACModel()
        {
            if (m_RANSAC_Models == null || m_RANSAC_Models.Count == 0) throw new Exception("The segment could not be found.");
            m_RANSAC_Models.Sort((i1, i2) => i2.ConsensusPoints.Length.CompareTo(i1.ConsensusPoints.Length));
        }
    }
    public class CvsLine
    {
        #region Properties
        /// <summary>
        /// 선분의 시작점.
        /// </summary>
        public Point StartPoint { get; }
        /// <summary>
        /// 선분의 끝점.
        /// </summary>
        public Point EndPoint { get; }
        /// <summary>
        /// 선의 기울기.
        /// </summary>
        public double Gradient { get; }
        /// <summary>
        /// 선의 Y 절편.
        /// </summary>
        public double Y_Intercept { get; }
        /// <summary>
        /// 해당 선분의 선상에 있는 점들.
        /// </summary>
        public Point[] ConsensusPoints { get; }
        #endregion

        public CvsLine(Point startP, Point endP, double gradient, double intercept, Point[] consensusPoints)
        {
            this.StartPoint = startP;
            this.EndPoint = endP;
            this.Gradient = gradient;
            this.Y_Intercept = intercept;
            this.ConsensusPoints = consensusPoints;
        }
    }
}
