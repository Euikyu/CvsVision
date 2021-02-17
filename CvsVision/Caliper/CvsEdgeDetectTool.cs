﻿using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;

namespace CvsVision.Caliper
{
    public class CvsEdgeDetectTool : ICvsTool
    {
        #region Fields
        private CvsRectangleAffine m_Region;
        private CvsEdgeDetect m_EdgeDetect;
        #endregion

        #region Properties
        
        public uint ContrastThreshold
        {
            get
            {
                if (m_EdgeDetect != null) return m_EdgeDetect.ContrastThreshold;
                else return 0;
            }
            set
            {
                if (m_EdgeDetect != null) m_EdgeDetect.ContrastThreshold = value;
            }
        }
        public uint HalfPixelCount
        {
            get
            {
                if (m_EdgeDetect != null) return m_EdgeDetect.HalfPixelCount;
                else return 0;
            }
            set
            {
                if(m_EdgeDetect != null)
                {
                    if (value < 1) m_EdgeDetect.HalfPixelCount = 1;
                    else m_EdgeDetect.HalfPixelCount = value;
                }
            }
        }
        /// <summary>
        /// 에지를 감지할 방향.
        /// </summary>
        public EDirection EdgeDirection
        {
            get
            {
                if (m_EdgeDetect != null) return m_EdgeDetect.EdgeDirection;
                else return EDirection.Any;
            }
            set
            {
                if (m_EdgeDetect != null) m_EdgeDetect.EdgeDirection = value;
            }
        }


        public ICvsRegion Region
        {
            get { return m_Region; }
            set
            {
                if (value is CvsRectangleAffine affine)
                {
                    m_Region = affine;
                }
            }
        }

        public CvsEdge Edge { get { return m_EdgeDetect.Edge; } }
        public DrawingGroup Overlay { get; private set; }
        public Bitmap InputImage { get; set; }

        #endregion

        public CvsEdgeDetectTool()
        {
            m_EdgeDetect = new CvsEdgeDetect();
            m_Region = new CvsRectangleAffine();
        }

        public void Dispose()
        {
            if (InputImage != null) InputImage.Dispose();
        }

        #region Methods
        public void Load(string filePath, Type toolType)
        {

        }

        public void Save(string path)
        {

        }


        public void Run()
        {
            m_EdgeDetect.DetectImage = Region.Crop(InputImage);
            m_EdgeDetect.Detect();

            Overlay = this.CreateGeometry();
        }
        
        private DrawingGroup CreateGeometry()
        {

            DrawingGroup dg = new DrawingGroup();
            GeometryDrawing overlay = new GeometryDrawing
            {
                Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, InputImage.Width, InputImage.Height)),
                Brush = Brushes.Transparent,
                Pen = new Pen(Brushes.Transparent, 1)
            };
            dg.Children.Add(overlay);

            if (m_EdgeDetect != null && m_EdgeDetect.Edge != null)
            {
                GeometryDrawing graphic = new GeometryDrawing();
                GeometryGroup group = new GeometryGroup();

                group.Children.Add(new LineGeometry(m_Region.Pose.GetPointByOrigin(new System.Windows.Point(-m_Region.Width / 2, m_EdgeDetect.Edge.Y)), m_Region.Pose.GetPointByOrigin(new System.Windows.Point(m_Region.Width / 2, m_EdgeDetect.Edge.Y))));

                graphic.Geometry = group;
                graphic.Brush = Brushes.Transparent;
                graphic.Pen = new Pen(Brushes.LawnGreen, 1);
                graphic.Freeze();

                dg.Children.Add(graphic);
            }
            return dg;
        }

        #endregion
    }
}
