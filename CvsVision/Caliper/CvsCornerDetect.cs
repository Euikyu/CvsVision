using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CvsVision.Caliper
{
    /// <summary>
    /// 주어진 선 두 개의 교차점을 찾는 클래스입니다.
    /// </summary>
    public class CvsCornerDetect : IDisposable
    {
        #region Fields

        #endregion

        #region Properties
        /// <summary>
        /// 직선 A의 값을 가져오거나 설정합니다.
        /// </summary>
        public CvsLine LineA { get; set; }
        /// <summary>
        /// 직선 B의 값을 가져오거나 설정합니다.
        /// </summary>
        public CvsLine LineB { get; set; }
        /// <summary>
        /// 두 직선의 교점 정보를 가져옵니다.
        /// </summary>
        public CvsCorner Corner { get; private set; }
        #endregion
        /// <summary>
        /// 주어진 선 두 개의 교차점을 찾는 클래스를 생성합니다.
        /// </summary>
        public CvsCornerDetect()
        {

        }

#pragma warning disable CS1591 // 공개된 형식 또는 멤버에 대한 XML 주석이 없습니다.
        public void Dispose()
#pragma warning restore CS1591 // 공개된 형식 또는 멤버에 대한 XML 주석이 없습니다.
        {

        }

        #region Methods
        /// <summary>
        /// 두 직선의 교점을 검색합니다.
        /// </summary>
        public void Detect()
        {
            if (LineA == null)
            {
                Corner = null;
                throw new Exception("Not found line A");
            }
            if (LineB == null)
            {
                Corner = null;
                throw new Exception("Not found line B");
            }
            //기울기가 같을 경우
            if (LineA.Gradient == LineB.Gradient || (double.IsNaN(LineA.Gradient) && double.IsNaN(LineB.Gradient)))
            {
                //무수히 만나는 경우
                if(LineA.Y_Intercept == LineB.Y_Intercept)
                {
                    Corner = new CvsCorner(new Point(double.PositiveInfinity, double.PositiveInfinity), 0);
                }
                //평행한 경우
                else
                {
                    Corner = new CvsCorner(new Point(double.NaN, double.NaN), 0);
                }
            }
            //Line A가 x = c 인 직선인 경우
            else if (double.IsNaN(LineA.Gradient))
            {
                var corner = new Point(LineA.Y_Intercept, LineB.Gradient * LineA.Y_Intercept + LineB.Y_Intercept);
                Corner = new CvsCorner(corner, Math.Abs(0.5 * Math.PI - (corner.X == 0 ? Math.Atan2(LineB.Gradient, 1) : Math.Atan2(LineB.Y_Intercept - corner.Y, -corner.X))));
            }
            //Line B가 x = c 인 직선인 경우
            else if (double.IsNaN(LineB.Gradient))
            {
                var corner = new Point(LineB.Y_Intercept, LineA.Gradient * LineB.Y_Intercept + LineA.Y_Intercept);
                Corner = new CvsCorner(corner, Math.Abs(0.5 * Math.PI - (corner.X == 0 ? Math.Atan2(LineA.Gradient, 1) : Math.Atan2(LineA.Y_Intercept - corner.Y, -corner.X))));
            }
            //나머지 한 점에서 만나는 경우
            else
            {
                var newX = -(LineA.Y_Intercept - LineB.Y_Intercept) / (LineA.Gradient - LineB.Gradient);
                var corner = new Point(newX, LineA.Gradient * newX + LineA.Y_Intercept);

                if (corner.X == 0) Corner = new CvsCorner(corner, Math.Abs(Math.Atan2(LineB.Gradient, 1) - Math.Atan2(LineA.Gradient, 1)));
                else Corner = new CvsCorner(corner, Math.Abs(Math.Atan2(LineB.Y_Intercept - corner.Y, -corner.X) - Math.Atan2(LineA.Y_Intercept - corner.Y, -corner.X)));
            }
        }
        #endregion
    }

    /// <summary>
    /// 두 직선의 교점 클래스입니다.
    /// </summary>
    public class CvsCorner
    {
        #region Fields

        #endregion

        #region Properties
        /// <summary>
        /// 교점을 가져옵니다.
        /// </summary>
        public Point Corner { get; }
        /// <summary>
        /// 두 직선의 사이각을 가져옵니다.
        /// </summary>
        public double IntersectionAngle { get; }
        #endregion
        /// <summary>
        /// 두 직선의 교점 클래스를 생성합니다.
        /// </summary>
        /// <param name="corner">두 직선의 교점.</param>
        /// <param name="angle">두 직선의 사이각.</param>
        public CvsCorner(Point corner, double angle)
        {
            Corner = corner;
            IntersectionAngle = angle;
        }
    }
}
