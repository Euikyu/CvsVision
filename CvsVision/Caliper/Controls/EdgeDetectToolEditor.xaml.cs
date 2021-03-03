using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZoomPanCon;

namespace CvsVision.Caliper.Controls
{
    /// <summary>
    /// EdgeDetectToolEditor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EdgeDetectToolEditor : UserControl, INotifyPropertyChanged
    {
        #region Fields
        private bool m_IsEditing;
        private System.Drawing.Bitmap m_CurrentBitmap;
        private BitmapSource m_OriginSource;
        private CvsEdgeDetectTool m_Tool;

        #endregion

        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Common Properties
        /// <summary>
        /// 현재 도구가 수정 중인지 여부를 가져오거나 설정합니다.
        /// </summary>
        public bool IsEditing
        {
            get { return m_IsEditing; }
            set
            {
                if (m_Tool != null && m_OriginSource != null)
                {
                    m_IsEditing = value;
                    this.RaisePropertyChanged(nameof(IsEditing));
                }
                else
                {
                    m_IsEditing = false;
                    this.RaisePropertyChanged(nameof(IsEditing));
                }
            }
        }
        /// <summary>
        /// 에지 그래픽의 투사 길이를 가져오거나 설정합니다. 
        /// (영역의 너비입니다.)
        /// </summary>
        public double ProjectionLength
        {
            get
            {
                if (m_Tool != null && m_Tool.Setting != null) return m_Tool.Setting.ProjectionLength;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Setting != null)
                {
                    m_Tool.Setting.ProjectionLength = value;
                    this.RaisePropertyChanged(nameof(ProjectionLength));
                }
            }
        }
        /// <summary>
        /// 에지 그래픽의 검색 길이를 가져오거나 설정합니다.
        /// (영역의 높이입니다.)
        /// </summary>
        public double SearchLength
        {
            get
            {
                if (m_Tool != null && m_Tool.Setting != null) return m_Tool.Setting.SearchLength;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Setting != null)
                {
                    m_Tool.Setting.SearchLength = value;
                    this.RaisePropertyChanged(nameof(SearchLength));
                }
            }
        }
        /// <summary>
        /// 에지 그래픽의 원점 X 좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginX
        {
            get
            {
                if (m_Tool != null && m_Tool.Setting != null) return m_Tool.Setting.OriginX;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Setting != null)
                {
                    m_Tool.Setting.OriginX = value;
                    this.RaisePropertyChanged(nameof(OriginX));
                }
            }
        }
        /// <summary>
        /// 에지 그래픽의 원점 Y 좌표를 가져오거나 설정합니다.
        /// </summary>
        public double OriginY
        {
            get
            {
                if (m_Tool != null && m_Tool.Setting != null) return m_Tool.Setting.OriginY;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Setting != null)
                {
                    m_Tool.Setting.OriginY = value;
                    this.RaisePropertyChanged(nameof(OriginY));
                }
            }
        }
        /// <summary>
        /// 에지 그래픽의 회전 라디안 값을 가져오거나 설정합니다.
        /// </summary>
        public double Radian
        {
            get
            {
                if (m_Tool != null && m_Tool.Setting != null) return m_Tool.Setting.Radian;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Setting != null)
                {
                    m_Tool.Setting.Radian = value;
                    this.RaisePropertyChanged(nameof(Radian));
                    this.RaisePropertyChanged(nameof(Rotation));
                }
            }
        }
        /// <summary>
        /// 에지 그래픽의 회전 Degree 값을 가져오거나 설정합니다.
        /// </summary>
        public double Rotation
        {
            get
            {
                return Radian * 180 / Math.PI;
            }
            set
            {
                Radian = value * Math.PI / 180;

            }
        }

        /// <summary>
        /// 특정 이상의 변화량을 에지로 판단하기 위한 임계값을 가져오거나 설정합니다. 
        /// </summary>
        public uint ContrastThreshold
        {
            get
            {
                if (m_Tool != null && m_Tool.Setting != null) return m_Tool.Setting.ContrastThreshold;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Setting != null)
                {
                    m_Tool.Setting.ContrastThreshold = value;
                    this.RaisePropertyChanged(nameof(ContrastThreshold));
                }
            }
        }
        /// <summary>
        /// 에지로 인식할 절반 픽셀 개수를 가져오거나 설정합니다.
        /// </summary>
        public uint HalfPixelCount
        {
            get
            {
                if (m_Tool != null && m_Tool.Setting != null) return m_Tool.Setting.HalfPixelCount;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Setting != null)
                {
                    m_Tool.Setting.HalfPixelCount = value;
                    this.RaisePropertyChanged(nameof(HalfPixelCount));
                }
            }
        }
        /// <summary>
        /// 에지를 감지할 방향을 가져오거나 설정합니다.
        /// </summary>
        public int SelectedEdgeDirection
        {
            get
            {
                if (m_Tool != null && m_Tool.Setting != null) return (int)m_Tool.Setting.EdgeDirection;
                else return -1;
            }
            set
            {
                if (m_Tool != null && m_Tool.Setting != null)
                {
                    m_Tool.Setting.EdgeDirection = (EDirection)value;
                    this.RaisePropertyChanged(nameof(SelectedEdgeDirection));
                }
            }
        }
        /// <summary>
        /// 화면에 출력할 원본 이미지를 가져오거나 설정합니다.
        /// </summary>
        public BitmapSource OriginSource
        {
            get { return m_OriginSource; }
            set
            {
                m_OriginSource = value;
                this.RaisePropertyChanged(nameof(OriginSource));
                this.RaisePropertyChanged(nameof(Overlay));

                ImageWidth = m_OriginSource.Width;
                ImageHeight = m_OriginSource.Height;
                this.RaisePropertyChanged(nameof(ImageWidth));
                this.RaisePropertyChanged(nameof(ImageHeight));
            }
        }
        /// <summary>
        /// 화면에 출력할 결과 오버레이를 가져옵니다.
        /// </summary>
        public DrawingGroup Overlay
        {
            get
            {
                if (m_Tool != null) return m_Tool.Overlay;
                else return null;
            }
        }
        /// <summary>
        /// 결과를 반영하는 메세지를 가져옵니다.
        /// </summary>
        public string Message
        {
            get
            {
                if (m_CurrentBitmap == null) return "Image not supplied.";
                else if (m_Tool == null) return "Please load tool.";
                else if (m_Tool.Exception != null) return "Error - " + m_Tool.Exception.Message + ".";
                else return "Success.";
            }
        }
        /// <summary>
        /// 원본 이미지의 너비를 가져옵니다.
        /// </summary>
        public double ImageWidth { get; private set; }
        /// <summary>
        /// 원본 이미지의 높이를 가져옵니다.
        /// </summary>
        public double ImageHeight { get; private set; }
        #endregion

        #region Dependency Properties
        /// <summary>
        /// EdgeDetectToolEditor.SubjectTool의 종속성 속성을 식별합니다.
        /// </summary>
        public static readonly DependencyProperty SubjectToolProperty =
            DependencyProperty.Register(nameof(SubjectTool), typeof(CvsEdgeDetectTool), typeof(EdgeDetectToolEditor),
                new PropertyMetadata(new CvsEdgeDetectTool()));

        /// <summary>
        /// 현재 에디터의 주체가 되는 검사 도구를 가져옵니다.
        /// </summary>
        public CvsEdgeDetectTool SubjectTool
        {
            get { return GetValue(SubjectToolProperty) as CvsEdgeDetectTool; }
            set
            {
                SetValue(SubjectToolProperty, value);
                m_Tool = value;
                this.UpdateToolData();
            }
        }
        #endregion

        #endregion

        public EdgeDetectToolEditor()
        {
            InitializeComponent();
            DataContext = this;
            SubjectTool = new CvsEdgeDetectTool();
        }

        #region Methods
        /// <summary>
        /// 모든 속성 업데이트하기.
        /// </summary>
        private void UpdateToolData()
        {
            this.RaisePropertyChanged(nameof(ProjectionLength));
            this.RaisePropertyChanged(nameof(SearchLength));
            this.RaisePropertyChanged(nameof(OriginX));
            this.RaisePropertyChanged(nameof(OriginY));
            this.RaisePropertyChanged(nameof(Radian));
            this.RaisePropertyChanged(nameof(Rotation));
            this.RaisePropertyChanged(nameof(ContrastThreshold));
            this.RaisePropertyChanged(nameof(HalfPixelCount));
            this.RaisePropertyChanged(nameof(SelectedEdgeDirection));
            this.RaisePropertyChanged(nameof(Message));
        }
        
        #region Events
        // 이미지 불러오는 콜백
        private void LoadImageBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Bitmap Image Files (*.bmp)|*.bmp"
            };
            if ((bool)d.ShowDialog())
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(d.FileName);
                if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                {
                    MessageBox.Show("It support only Format8bppIndexed.");
                }
                else
                {
                    //현재 이미지에 넣고,
                    m_CurrentBitmap = bmp;
                    var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                    //화면에 출력
                    OriginSource = BitmapSource.Create(data.Width, data.Height, bmp.HorizontalResolution, bmp.VerticalResolution, PixelFormats.Gray8, null, data.Scan0, data.Stride * data.Height, data.Stride);
                    OriginSource.Freeze();
                    bmp.UnlockBits(data);
                    //기존 입력이미지는 비우고
                    if (m_Tool.InputImage != null) m_Tool.InputImage.Dispose();
                    //현재 이미지를 입력이미지로
                    m_Tool.InputImage = m_CurrentBitmap;
                }
            }
            //메세지 업데이트
            this.RaisePropertyChanged(nameof(Message));
        }

        // 도구 불러오기 콜백
        private void LoadToolBtn_Click(object sender, RoutedEventArgs e)
        {
            //Tool 불러오는 과정 실행해야함
            m_Tool.Load("<Input the loading file path>", typeof(CvsEdgeDetectTool));
            this.UpdateToolData();
        }

        // 도구 저장하기 콜백
        private void SaveToolBtn_Click(object sender, RoutedEventArgs e)
        {
            //Tool 저장
            SubjectTool = m_Tool;
            SubjectTool.Save("<Input the saving file path>");
        }

        // 검사 실행하기 콜백
        private void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            m_Tool.Run();
            this.RaisePropertyChanged(nameof(Overlay));

            IsEditing = false;
            this.RaisePropertyChanged(nameof(Message));
        }
        #endregion

        #endregion

    }
}
