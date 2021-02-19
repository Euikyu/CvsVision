using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Point = System.Windows.Point;

namespace CvsVision
{
    public class CvsPose
    {
        #region Fields
        
        #endregion

        #region Properties
        public CvsPose Parent { get; set; }

        public double Radian { get; set; }
        public double TranslateX { get; set; }
        public double TranslateY { get; set; }
        #endregion

        public CvsPose()
        {
            Radian = 0;
            TranslateX = 0;
            TranslateY = 0;

            Parent = null;
        }

        public CvsPose(double rad, double centerX, double centerY)
        {
            Radian = rad;
            TranslateX = centerX;
            TranslateY = centerY;

            Parent = new CvsPose();
        }

        public CvsPose(CvsPose parent)
        {
            Radian = 0;
            TranslateX = 0;
            TranslateY = 0;

            Parent = parent;
        }

        public CvsPose(CvsPose parent, double rad, double centerX, double centerY)
        {
            Radian = rad;
            TranslateX = centerX;
            TranslateY = centerY;

            Parent = parent;
        }

        #region Methods
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

        public Point GetPointByOrigin(Point point)
        {
            if (Parent == null) return this.GetPointByPose(point);
            return Parent.GetPointByOrigin(this.GetPointByPose(point));
        }

        public Point GetPointByOrigin(double x, double y)
        {
            if (Parent == null) return this.GetPointByPose(x,y);
            return Parent.GetPointByOrigin(this.GetPointByPose(x,y));
        }

        public double GetRadianByOrigin()
        {
            if (Parent == null) return 0;
            return Parent.GetRadianByOrigin() + this.Radian;
        }
        #endregion
    }
}
