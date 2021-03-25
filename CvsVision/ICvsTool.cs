using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CvsVision
{
    /// <summary>
    /// 이미지를 받아 처리하는 검사 도구들의 메커니즘을 제공합니다.
    /// </summary>
    public interface ICvsTool : IDisposable
    {
        #region Properties
        /// <summary>
        /// 입력이미지를 가져오거나 설정합니다.
        /// </summary>
        Bitmap InputImage { get; set; }
        /// <summary>
        /// 결과 오버레이를 가져옵니다.
        /// </summary>
        DrawingGroup Overlay { get; }
        /// <summary>
        /// 해당 도구 사용 시 발생하는 예외를 가져옵니다. 
        /// (Null 값 일 경우, 정상적으로 동작한 것입니다.)
        /// </summary>
        Exception Exception { get; }
        #endregion

        #region Methods
        /// <summary>
        /// 파일 형태로 저장된 설정 값들을 불러옵니다.
        /// </summary>
        /// <param name="path">저장된 설정 파일 경로.</param>
        void Load(string path);
        /// <summary>
        /// 현재 설정 값들을 파일 형태로 저장합니다.
        /// </summary>
        /// <param name="path">저장할 파일 경로.</param>
        void Save(string path);
        /// <summary>
        /// 이미지 처리를 시작합니다.
        /// </summary>
        void Run();
        #endregion
    }
}
