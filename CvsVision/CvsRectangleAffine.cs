using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Point = System.Windows.Point;

namespace CvsVision
{
    /// <summary>
    /// 이미지 영역 중 하나인 Affine 사각형 영역 클래스입니다.
    /// </summary>
    public class CvsRectangleAffine : ICvsRegion
    {
        #region Fields
        private double m_OriginX;
        private double m_OriginY;
        private double m_Width;
        private double m_Height;

        private CvsPose m_Pose;

        private bool m_IsCenterOriented;
        #endregion

        #region Properties
        /// <summary>
        /// 영역의 원점 X 좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginX
        {
            get { return m_OriginX; }
            set
            {
                m_IsCenterOriented = false;
                m_OriginX = value;
                if(m_Pose != null)
                {
                    m_Pose.TranslateX = m_OriginX + m_Width / 2;
                }
            }
        }
        /// <summary>
        /// 영역의 원점 Y좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginY
        {
            get { return m_OriginY; }
            set
            {
                m_IsCenterOriented = false;
                m_OriginY = value;
                if (m_Pose != null)
                {
                    m_Pose.TranslateY = m_OriginY + m_Height / 2;
                }
            }
        }
        /// <summary>
        /// 영역의 너비를 가져오거나 설정합니다.
        /// </summary>
        public double Width
        {
            get { return m_Width; }
            set
            {
                m_Width = value;
                if (m_Pose != null)
                {
                    if (m_IsCenterOriented) m_OriginX = m_Pose.TranslateX - m_Width / 2;
                    else m_Pose.TranslateX = m_OriginX + m_Width / 2;
                }
            }
        }
        /// <summary>
        /// 영역의 높이를 가져오거나 설정합니다.
        /// </summary>
        public double Height
        {
            get { return m_Height; }
            set
            {
                m_Height = value;
                if (m_Pose != null)
                {
                    if (m_IsCenterOriented) m_OriginY = m_Pose.TranslateY - m_Height / 2;
                    else m_Pose.TranslateY = m_OriginY + m_Height / 2;
                }
            }
        }
        /// <summary>
        /// 영역의 회전 라디안 값을 가져오거나 설정합니다.
        /// </summary>
        public double Radian
        {
            get
            {
                if (m_Pose != null) return m_Pose.Radian;
                else return 0;
            }
            set
            {
                if (m_Pose != null)
                {
                    m_Pose.Radian = value;
                }
            }
        }
        /// <summary>
        /// 영역의 중심 좌표 값을 가져오거나 설정합니다.
        /// </summary>
        public Point Center
        {
            get
            {
                if (m_Pose != null) return new Point(m_Pose.TranslateX, m_Pose.TranslateY);
                else return new Point(m_OriginX + m_Width / 2, m_OriginY + m_Height / 2);
            }
            set
            {
                if (m_Pose != null)
                {
                    m_IsCenterOriented = true;
                    m_Pose.TranslateX = value.X;
                    m_Pose.TranslateY = value.Y;
                }
            }
        }
        /// <summary>
        /// 영역의 Pose 값을 가져오거나 설정합니다.
        /// </summary>
        public CvsPose Pose
        {
            get { return m_Pose; }
            set
            {
                m_IsCenterOriented = true;
                m_Pose = value;
                m_OriginX = m_Pose.TranslateX - m_Width / 2;
                m_OriginY = m_Pose.TranslateY - m_Height / 2;
            }
        }
        #endregion

        #region Constuctor
        /// <summary>
        /// Affine 사각형 클래스를 생성합니다.
        /// </summary>
        public CvsRectangleAffine()
        {
            Pose = new CvsPose();
        }
        /// <summary>
        /// 지정된 너비와 높이를 가지는 Affine 사각형 클래스를 생성합니다.
        /// </summary>
        /// <param name="width">사각형의 너비.</param>
        /// <param name="height">사각형의 높이.</param>
        public CvsRectangleAffine(double width, double height)
        {
            Pose = new CvsPose();
            this.Width = width;
            this.Height = height;
        }
        /// <summary>
        /// 지정된 값들을 이용하여 Affine 사각형 클래스를 생성합니다.
        /// </summary>
        /// <param name="originX">사각형의 원점 X 좌표.</param>
        /// <param name="originY">사각형의 원점 Y 좌표.</param>
        /// <param name="width">사각형의 너비.</param>
        /// <param name="height">사각형의 높이.</param>
        /// <param name="radian">사각형의 회전 라디안 값.</param>
        public CvsRectangleAffine(double originX, double originY, double width, double height, double radian)
        {
            Pose = new CvsPose();
            this.OriginX = originX;
            this.OriginY = originY;
            this.Width = width;
            this.Height = height;
            this.Radian = radian;
        }
        /// <summary>
        /// 지정된 좌표 축을 가지는 Affine 사각형 클래스를 생성합니다.
        /// </summary>
        /// <param name="pose">사각형의 좌표 축 값.</param>
        public CvsRectangleAffine(CvsPose pose)
        {
            this.Pose = pose;
        }
        /// <summary>
        /// 지정된 좌표 축과 회전 라디안 값을 가지는 Affine 사각형 클래스를 생성합니다.
        /// </summary>
        /// <param name="pose">사각형의 좌표 축 값.</param>
        /// <param name="radian">사각형의 회전 라디안 값.</param>
        public CvsRectangleAffine(CvsPose pose, double radian)
        {
            this.Pose = pose;
            this.Radian = radian;
        }
        #endregion

        #region Methods
        /// <summary>
        /// 지정된 영역만큼 이미지를 잘라 반환합니다.
        /// </summary>
        /// <param name="BitmapSrc">자를 이미지.</param>
        /// <returns>잘린 이미지.</returns>
        public Bitmap Crop(System.Drawing.Bitmap BitmapSrc)
        {
            try
            {
                Bitmap dstBitmap = null;

                switch (BitmapSrc.PixelFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                        {
                            dstBitmap = CropImageMono8(BitmapSrc);
                        }
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                        {
                            dstBitmap = CropImageRGB24(BitmapSrc);
                        }
                        break;
                    default:
                        {
                            dstBitmap = null;
                        }
                        break;
                }

                return dstBitmap;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// mono 이미지를 자르는 함수.
        /// </summary>
        /// <param name="bitmapSrc">자를 이미지.</param>
        /// <returns>잘린 이미지.</returns>
        private Bitmap CropImageMono8(System.Drawing.Bitmap bitmapSrc)
        {
            try
            {
                var dstWidth = (int)Math.Round(m_Width);
                dstWidth = (dstWidth + 3) / 4 * 4;
                
                // Create BitmapDst 
                System.Drawing.Bitmap bitmapDst = new System.Drawing.Bitmap(dstWidth, (int)Math.Round(m_Height), System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

                // Convert to BitmapData
                BitmapData DataSrc = bitmapSrc.LockBits(new System.Drawing.Rectangle(0, 0, bitmapSrc.Width, bitmapSrc.Height), ImageLockMode.ReadWrite, bitmapSrc.PixelFormat);
                BitmapData DataDst = bitmapDst.LockBits(new System.Drawing.Rectangle(0, 0, bitmapDst.Width, bitmapDst.Height), ImageLockMode.ReadWrite, bitmapDst.PixelFormat);

                // 필요 요소 정의
                int sizeSrc = DataSrc.Stride * DataSrc.Height;
                int sizeDst = DataDst.Stride * DataDst.Height;
                byte[] arraySrc = new byte[sizeSrc];
                byte[] arrayDst = new byte[sizeDst];

                // Data copy from DataSrc to ArraySrc[]
                System.Runtime.InteropServices.Marshal.Copy(DataSrc.Scan0, arraySrc, 0, sizeSrc); //  Marshal.Copy로 memcopy마냥 쓸 수 있구먼

                // ArrayDst[] 채우기
                for (int j = 0; j < bitmapDst.Height; j++)
                {
                    for (int i = 0; i < bitmapDst.Width; i++)
                    {
                        // Pose Matrix (회전 후 이동)
                        Point point = Pose.GetPointByOrigin(i - bitmapDst.Width / 2, j - bitmapDst.Height / 2);
                        int srcx = (int)point.X;
                        int srcy = (int)point.Y;

                        var srcRange = srcx + srcy * DataSrc.Stride;
                        var dstRange = i + j * DataDst.Stride;
                        // srcx와 srcy가 소스 밖의 점이면 0으로 넣자
                        if (srcRange < sizeSrc && srcRange >= 0 && dstRange < sizeDst)
                            arrayDst[i + j * DataDst.Stride] = arraySrc[srcx + srcy * DataSrc.Stride];
                    }
                }

                // Array에서 BitmapData로 Copy
                System.Runtime.InteropServices.Marshal.Copy(arrayDst, 0, DataDst.Scan0, arrayDst.Length);

                bitmapSrc.UnlockBits(DataSrc);
                bitmapDst.UnlockBits(DataDst);

                // 팔레트 설정해주기 ( 모노 bmp일 경우 팔레트 사용함 )
                UpdatePaletteForMono8(bitmapDst);

                return bitmapDst;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 8비트 이미지의 팔레트를 그레이스케일로 변환하는 함수.
        /// </summary>
        /// <param name="BitmapDst">변환할 이미지.</param>
        private void UpdatePaletteForMono8(System.Drawing.Bitmap BitmapDst)
        {
            try
            {
                ColorPalette _palette = BitmapDst.Palette;
                System.Drawing.Color[] _entries = _palette.Entries;
                for (int i = 0; i < 256; i++)
                {
                    System.Drawing.Color b = new System.Drawing.Color();
                    b = System.Drawing.Color.FromArgb((byte)i, (byte)i, (byte)i);
                    _entries[i] = b;
                }
                BitmapDst.Palette = _palette;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 24bit RGB 이미지를 자르는 함수.
        /// </summary>
        /// <param name="bitmapSrc">자를 이미지.</param>
        /// <returns>잘린 이미지.</returns>
        private Bitmap CropImageRGB24(System.Drawing.Bitmap bitmapSrc)
        {
            try
            {
                // Create BitmapDst 
                System.Drawing.Bitmap bitmapDst = new System.Drawing.Bitmap((int)Math.Round(m_Width), (int)Math.Round(m_Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                // Convert to BitmapData
                BitmapData dataSrc = bitmapSrc.LockBits(new System.Drawing.Rectangle(0, 0, bitmapSrc.Width, bitmapSrc.Height), ImageLockMode.ReadWrite, bitmapSrc.PixelFormat);
                BitmapData dataDst = bitmapDst.LockBits(new System.Drawing.Rectangle(0, 0, bitmapDst.Width, bitmapDst.Height), ImageLockMode.ReadWrite, bitmapDst.PixelFormat);

                // 필요 요소 정의
                int sizeSrc = dataSrc.Stride * dataSrc.Height;
                int sizeDst = dataDst.Stride * dataDst.Height;
                byte[] arraySrc = new byte[sizeSrc];
                byte[] arrayDst = new byte[sizeDst];

                // Data copy from DataSrc to ArraySrc[]
                System.Runtime.InteropServices.Marshal.Copy(dataSrc.Scan0, arraySrc, 0, sizeSrc); //  Marshal.Copy로 memcopy마냥 쓸 수 있구먼

                // ArrayDst[] 채우기
                for (int j = 0; j < Height; j++)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        // Pose Matrix (회전 후 이동)
                        Point point = Pose.GetPointByOrigin(i - bitmapDst.Width / 2, j - bitmapDst.Height / 2);
                        int srcx = (int)point.X;
                        int srcy = (int)point.Y;

                        // srcx와 srcy가 소스 밖의 점이면 0으로 넣자
                        if (srcx >= 0 && srcx <= bitmapSrc.Width && srcy >= 0 && srcy <= bitmapSrc.Height)
                        {
                            arrayDst[i * 3 + 0 + j * dataDst.Stride] = arraySrc[srcx * 3 + 0 + srcy * dataSrc.Stride];
                            arrayDst[i * 3 + 1 + j * dataDst.Stride] = arraySrc[srcx * 3 + 1 + srcy * dataSrc.Stride];
                            arrayDst[i * 3 + 2 + j * dataDst.Stride] = arraySrc[srcx * 3 + 2 + srcy * dataSrc.Stride];
                        }
                        else
                        {
                            arrayDst[i * 3 + 0 + j * dataDst.Stride] = 0;
                            arrayDst[i * 3 + 1 + j * dataDst.Stride] = 0;
                            arrayDst[i * 3 + 2 + j * dataDst.Stride] = 0;
                        }
                    }
                }

                // Array에서 BitmapData로 Copy
                System.Runtime.InteropServices.Marshal.Copy(arrayDst, 0, dataDst.Scan0, arrayDst.Length);

                bitmapSrc.UnlockBits(dataSrc);
                bitmapDst.UnlockBits(dataDst);

                return bitmapDst;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 픽셀 당 비트 수를 반환하는 함수.
        /// </summary>
        /// <param name="Pixelformat">비트맵의 픽셀 형식.</param>
        /// <returns>픽셀 당 비트 수.</returns>
        public byte GetBitsPerPixel(System.Drawing.Imaging.PixelFormat Pixelformat)
        {
            try
            {
                int BitsPerPixel;

                switch (Pixelformat)
                {
                    case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                        BitsPerPixel = 8;
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                        BitsPerPixel = 24;
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                        BitsPerPixel = 32;
                        break;
                    default:
                        BitsPerPixel = 0;
                        break;
                }

                byte bitsPerPixel = (byte)((float)(BitsPerPixel + 7) / 8);
                return bitsPerPixel;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

    }


}
