using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Windows.Point;

namespace CvsVision
{
    /// <summary>
    /// 이미지를 자르기 위한 다양한 영역을 제공합니다.
    /// </summary>
    public interface ICvsRegion
    {
        #region Properties
        /// <summary>
        /// 해당 영역의 Pose 값을 가져오거나 설정합니다.
        /// </summary>
        CvsPose Pose { get; set; }
        /// <summary>
        /// 해당 영역의 중심 좌표를 가져오거나 설정합니다.
        /// </summary>
        Point Center { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 본 영역에 맞게 이미지를 자릅니다.
        /// </summary>
        /// <param name="bmp">자를 이미지.</param>
        /// <returns>자른 이미지.</returns>
        Bitmap Crop(Bitmap bmp);
        #endregion
    }
}
