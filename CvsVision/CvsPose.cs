using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Point = System.Windows.Point;

namespace CvsVision
{
    /// <summary>
    /// 좌표계 축 클래스입니다.
    /// </summary>
    public class CvsPose : ICloneable
    {
        #region Fields
        
        #endregion

        #region Properties
        /// <summary>
        /// 본 Pose의 부모 Pose를 가져오거나 설정합니다.
        /// </summary>
        public CvsPose Parent { get; set; }
        /// <summary>
        /// 본 Pose의 회전 라디안 값을 가져오거나 설정합니다.
        /// </summary>
        public double Radian { get; set; }
        /// <summary>
        /// 본 Pose의 회전축 X 좌표를 가져옵니다.
        /// </summary>
        public double TranslateX { get; set; }
        /// <summary>
        /// 본 Pose의 회전축 Y 좌표를 가져옵니다.
        /// </summary>
        public double TranslateY { get; set; }
        #endregion
        /// <summary>
        /// 좌표계 축 클래스를 생성합니다.
        /// </summary>
        public CvsPose()
        {
            Radian = 0;
            TranslateX = 0;
            TranslateY = 0;

            Parent = null;
        }
        /// <summary>
        /// 지정된 회전 값과 회전축을 가지는 좌표계 축 클래스를 생성합니다.
        /// </summary>
        /// <param name="rad">회전 라디안 값.</param>
        /// <param name="centerX">회전 중심 축 X 좌표.</param>
        /// <param name="centerY">회전 중심 축 Y 좌표.</param>
        public CvsPose(double rad, double centerX, double centerY)
        {
            Radian = rad;
            TranslateX = centerX;
            TranslateY = centerY;

            Parent = null;
        }
        /// <summary>
        /// 지정된 부모 Pose를 가지는 좌표계 축 클래스를 생성합니다.
        /// </summary>
        /// <param name="parent">부모 Pose 값.</param>
        public CvsPose(CvsPose parent)
        {
            Radian = 0;
            TranslateX = 0;
            TranslateY = 0;

            Parent = parent;
        }
        /// <summary>
        /// 지정된 회전 값과 회전축, 지정된 부모 Pose를 가지는 좌표계 축 클래스를 생성합니다.
        /// </summary>
        /// <param name="parent">부모 Pose 값.</param>
        /// <param name="rad">회전 라디안 값.</param>
        /// <param name="centerX">회전 중심축 X 좌표.</param>
        /// <param name="centerY">회전 중심축 Y 좌표.</param>
        public CvsPose(CvsPose parent, double rad, double centerX, double centerY)
        {
            Radian = rad;
            TranslateX = centerX;
            TranslateY = centerY;

            Parent = parent;
        }

        #region Methods
        /// <summary>
        /// 해당 Pose의 복사본을 생성합니다.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            if (Parent != null) return new CvsPose(Parent.Clone() as CvsPose, Radian, TranslateX, TranslateY);
            else return new CvsPose(null, Radian, TranslateX, TranslateY);
        }
        /// <summary>
        /// 현재 PoseMatrix를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public double[] GetPose()
        {
            var cos = Math.Cos(Radian);
            var sin = Math.Sin(Radian);
            return new double[]
            {
                cos, -sin, TranslateX,
                sin,  cos, TranslateY,
                  0,    0,          1
            };
        }
        /// <summary>
        /// 이전 좌표계에서의 점 위치를 반환합니다.
        /// </summary>
        /// <param name="point">Pose 연산할 점.</param>
        /// <returns></returns>
        public Point GetPointByPose(Point point)
        {
            var poseArr = this.GetPose();
            var resArr = new double[3];
            for (int i = 0; i < resArr.Length; i++)
            {
                resArr[i] = poseArr[3 * i] * point.X + poseArr[3 * i + 1] * point.Y + poseArr[3 * i + 2];
            }

            return new Point(resArr[0], resArr[1]);
        }
        /// <summary>
        /// 이전 좌표계에서의 점 위치를 반환합니다.
        /// </summary>
        /// <param name="x">Pose 연산할 점의 X 좌표.</param>
        /// <param name="y">Pose 연산할 점의 Y 좌표.</param>
        /// <returns></returns>
        public Point GetPointByPose(double x, double y)
        {
            var poseArr = this.GetPose();
            var resArr = new double[3];
            for (int i = 0; i < resArr.Length; i++)
            {
                resArr[i] = poseArr[3 * i] * x + poseArr[3 * i + 1] * y + poseArr[3 * i + 2];
            }

            return new Point(resArr[0], resArr[1]);
        }

        /// <summary>
        /// 원점 좌표계에서의 점 위치를 반환합니다.
        /// </summary>
        /// <param name="point">Pose 연산할 점.</param>
        /// <returns></returns>
        public Point GetPointByOrigin(Point point)
        {
            if (Parent == null) return this.GetPointByPose(point);
            return Parent.GetPointByOrigin(this.GetPointByPose(point));
        }

        /// <summary>
        /// 원점 좌표계에서의 점 위치를 반환합니다.
        /// </summary>
        /// <param name="x">Pose 연산할 점의 X 좌표.</param>
        /// <param name="y">Pose 연산할 점의 Y 좌표.</param>
        /// <returns></returns>
        public Point GetPointByOrigin(double x, double y)
        {
            if (Parent == null) return this.GetPointByPose(x,y);
            return Parent.GetPointByOrigin(this.GetPointByPose(x,y));
        }
        /// <summary>
        /// 이전 좌표계에서의 회전 라디안 값을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public double GetRadianByParent()
        {
            if (Parent == null) return Radian;
            return Parent.Radian + this.Radian;
        }
        /// <summary>
        /// 원점 좌표계에서의 회전 라디안 값을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public double GetRadianByOrigin()
        {
            if (Parent == null) return this.Radian;
            return Parent.GetRadianByOrigin() + this.Radian;
        }

        #endregion
    }
}
