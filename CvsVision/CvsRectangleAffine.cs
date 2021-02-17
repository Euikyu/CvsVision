using OpenCvSharp;
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
    public class CvsRectangleAffine : ICvsRegion
    {
        #region Fields
        private int m_OriginX;
        private int m_OriginY;
        private int m_Width;
        private int m_Height;
        private float m_Radian;

        private CvsPose m_Pose;
        #endregion

        #region Properties
        // 사각형 정보
        public double OriginX
        {
            get { return m_OriginX; }
            set
            {
                m_OriginX = (int)value;
                if(m_Pose != null)
                {
                    m_Pose.TranslateX = m_OriginX + m_Width / 2;
                }
            }
        }
        public double OriginY
        {
            get { return m_OriginY; }
            set
            {
                m_OriginY = (int)value;
                if (m_Pose != null)
                {
                    m_Pose.TranslateY = m_OriginY + m_Height / 2;
                }
            }
        }
        public double Width
        {
            get { return m_Width; }
            set
            {
                m_Width = (int)value;
                if (m_Pose != null)
                {
                    m_Pose.TranslateX = m_OriginX + m_Width / 2;
                }
            }
        }
        public double Height
        {
            get { return m_Height; }
            set
            {
                m_Height = (int)value;
                if (m_Pose != null)
                {
                    m_Pose.TranslateY = m_OriginY + m_Height / 2;
                }
            }
        }
        public double Radian
        {
            get { return m_Radian; }
            set
            {
                m_Radian = (float)value;
                if (m_Pose != null)
                {
                    m_Pose.Radian = m_Radian;
                }
            }
        }
        public Point Center
        {
            get { return new Point(m_OriginX + m_Width / 2, m_OriginY + m_Height / 2); }
            set
            {
                OriginX = value.X - m_Width / 2;
                OriginY = value.Y - m_Height / 2;
            }
        }
        public CvsPose Pose
        {
            get { return m_Pose; }
            set { m_Pose = value; }
        }
        #endregion

        public CvsRectangleAffine()
        {
            Pose = new CvsPose();
            OriginX = 20;
            OriginY = 20;
            Width = 100;
            Height = 100;
        }

        #region Methods
        // Bitmap Crop
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

        //
        public Bitmap CropImageMono8(System.Drawing.Bitmap BitmapSrc)
        {
            try
            {
                // Create BitmapDst 
                System.Drawing.Bitmap BitmapDst = new System.Drawing.Bitmap(m_Width, m_Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

                // Convert to BitmapData
                BitmapData DataSrc = BitmapSrc.LockBits(new System.Drawing.Rectangle(0, 0, BitmapSrc.Width, BitmapSrc.Height), ImageLockMode.ReadWrite, BitmapSrc.PixelFormat);
                BitmapData DataDst = BitmapDst.LockBits(new System.Drawing.Rectangle(0, 0, BitmapDst.Width, BitmapDst.Height), ImageLockMode.ReadWrite, BitmapDst.PixelFormat);

                // 필요 요소 정의
                byte bitsPerPixel = GetBitsPerPixel(BitmapSrc.PixelFormat);
                int SizeSrc = DataSrc.Stride * DataSrc.Height;
                int SizeDst = DataDst.Stride * DataDst.Height;
                byte[] ArraySrc = new byte[SizeSrc];
                byte[] ArrayDst = new byte[SizeDst];

                // Data copy from DataSrc to ArraySrc[]
                System.Runtime.InteropServices.Marshal.Copy(DataSrc.Scan0, ArraySrc, 0, SizeSrc); //  Marshal.Copy로 memcopy마냥 쓸 수 있구먼

                // ArrayDst[] 채우기
                for (int j = 0; j < Height; j++)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        // Pose Matrix (회전 후 이동)
                        Point point = PoseMatrix(i, j, m_OriginX, m_OriginY, m_Radian, m_Width, m_Height);
                        int srcx = (int)point.X;
                        int srcy = (int)point.Y;

                        // srcx와 srcy가 소스 밖의 점이면 0으로 넣자
                        if (srcx >= 0 && srcx <= BitmapSrc.Width && srcy >= 0 && srcy <= BitmapSrc.Height)
                            ArrayDst[i + j * DataDst.Stride] = ArraySrc[srcx + srcy * DataSrc.Stride];
                        else
                            ArrayDst[i + j * DataDst.Stride] = 0;
                    }
                }

                // Array에서 BitmapData로 Copy
                System.Runtime.InteropServices.Marshal.Copy(ArrayDst, 0, DataDst.Scan0, ArrayDst.Length);

                BitmapSrc.UnlockBits(DataSrc);
                BitmapDst.UnlockBits(DataDst);

                // 팔레트 설정해주기 ( 모노 bmp일 경우 팔레트 사용함 )
                UpdatePaletteForMono8(BitmapDst);

                return BitmapDst;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdatePaletteForMono8(System.Drawing.Bitmap BitmapDst)
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


        public Bitmap CropImageRGB24(System.Drawing.Bitmap BitmapSrc)
        {
            try
            {
                // Create BitmapDst 
                System.Drawing.Bitmap BitmapDst = new System.Drawing.Bitmap(m_Width, m_Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                // Convert to BitmapData
                BitmapData DataSrc = BitmapSrc.LockBits(new System.Drawing.Rectangle(0, 0, BitmapSrc.Width, BitmapSrc.Height), ImageLockMode.ReadWrite, BitmapSrc.PixelFormat);
                BitmapData DataDst = BitmapDst.LockBits(new System.Drawing.Rectangle(0, 0, BitmapDst.Width, BitmapDst.Height), ImageLockMode.ReadWrite, BitmapDst.PixelFormat);

                // 필요 요소 정의
                byte bitsPerPixel = GetBitsPerPixel(BitmapSrc.PixelFormat);
                int SizeSrc = DataSrc.Stride * DataSrc.Height;
                int SizeDst = DataDst.Stride * DataDst.Height;
                byte[] ArraySrc = new byte[SizeSrc];
                byte[] ArrayDst = new byte[SizeDst];

                // Data copy from DataSrc to ArraySrc[]
                System.Runtime.InteropServices.Marshal.Copy(DataSrc.Scan0, ArraySrc, 0, SizeSrc); //  Marshal.Copy로 memcopy마냥 쓸 수 있구먼

                // ArrayDst[] 채우기
                for (int j = 0; j < Height; j++)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        // Pose Matrix (회전 후 이동)
                        Point point = PoseMatrix(i, j, m_OriginX, m_OriginY, m_Radian, m_Width, m_Height);
                        int srcx = (int)point.X;
                        int srcy = (int)point.Y;

                        // srcx와 srcy가 소스 밖의 점이면 0으로 넣자
                        if (srcx >= 0 && srcx <= BitmapSrc.Width && srcy >= 0 && srcy <= BitmapSrc.Height)
                        {
                            ArrayDst[i * 3 + 0 + j * DataDst.Stride] = ArraySrc[srcx * 3 + 0 + srcy * DataSrc.Stride];
                            ArrayDst[i * 3 + 1 + j * DataDst.Stride] = ArraySrc[srcx * 3 + 1 + srcy * DataSrc.Stride];
                            ArrayDst[i * 3 + 2 + j * DataDst.Stride] = ArraySrc[srcx * 3 + 2 + srcy * DataSrc.Stride];
                        }
                        else
                        {
                            ArrayDst[i * 3 + 0 + j * DataDst.Stride] = 0;
                            ArrayDst[i * 3 + 1 + j * DataDst.Stride] = 0;
                            ArrayDst[i * 3 + 2 + j * DataDst.Stride] = 0;
                        }
                    }
                }

                // Array에서 BitmapData로 Copy
                System.Runtime.InteropServices.Marshal.Copy(ArrayDst, 0, DataDst.Scan0, ArrayDst.Length);

                BitmapSrc.UnlockBits(DataSrc);
                BitmapDst.UnlockBits(DataDst);

                return BitmapDst;
            }
            catch (Exception)
            {
                throw;
            }
        }


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

        // 회전 And 이동 매트릭스 테스트
        static Point PoseMatrix(float Model_x, float Model_y, float LeftTopX, float LeftTopY, float radian, int width, int height)
        {
            try
            {
                // 각도를 라디안으로 변환
                float Cos = (float)Math.Cos(radian);
                float Sin = (float)Math.Sin(radian);

                // 1. 배열 만들기
                float[] PoseArray = new float[] {
                    Cos, -Sin, LeftTopX + width/2,
                    Sin,  Cos, LeftTopY + height/2,
                      0,    0, 1
                };

                float[] PointArray = new float[] {
                    Model_x - width/2,
                    Model_y - height/2,
                    1
                };

                // 2. 배열로 Mat 만들기
                Mat PoseMat = new Mat(3, 3, MatType.CV_32FC1, PoseArray);
                Mat PointMat = new Mat(3, 1, MatType.CV_32FC1, PointArray);

                // 3. 계산하기
                Mat PosePoint = PoseMat * PointMat;
                float[] Result = new float[3 * 1];
                PosePoint.GetArray(0, 0, Result);

                Point resPoint = new Point(Result[0], Result[1]);

                return resPoint;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

    }


}
