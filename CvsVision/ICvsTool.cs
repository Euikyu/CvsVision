using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CvsVision
{
    public interface ICvsTool : IDisposable
    {
        #region Properties
        Bitmap InputImage { get; set; }
        DrawingGroup Overlay { get; }
        Exception Exception { get; }
        #endregion

        #region Methods
        void Load(string filePath, Type toolType);
        void Save(string path);
        void Run();
        #endregion
    }
}
