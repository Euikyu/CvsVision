using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Windows.Point;

namespace CvsVision
{
    public interface ICvsRegion
    {
        #region Properties
        CvsPose Pose { get; set; }
        Point Center { get; set; }
        #endregion

        #region Methods
        Bitmap Crop(Bitmap bmp);
        #endregion
    }
}
