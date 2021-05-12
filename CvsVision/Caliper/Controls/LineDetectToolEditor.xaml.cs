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
    /// LineDetectToolEditor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LineDetectToolEditor : UserControl, INotifyPropertyChanged
    {
        #region Fields
        private bool m_IsEditing;
        private System.Drawing.Bitmap m_CurrentBitmap;
        private BitmapSource m_OriginSource;
        private CvsLineDetectTool m_Tool;

        //private double m_SegmentLength;
        //private int m_CaliperCount;
        #endregion

        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Common Properties

        #region Line Settings
        /// <summary>
        /// 선 그래픽의 원점 X 좌표를 가져오거나 설정합니다.
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
        /// 선 그래픽의 원점 Y 좌표를 가져오거나 설정합니다.
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
        /// 선 그래픽의 회전 라디안 값을 가져오거나 설정합니다.
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
        /// 선 그래픽의 회전 Degree 값을 가져오거나 설정합니다.
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
        /// 선 상으로 인정되는 범위 값을 가져오거나 설정합니다.
        /// </summary>
        public double ConsensusThreshold
        {
            get
            {
                if (m_Tool != null && m_Tool.Setting != null) return m_Tool.Setting.ConsensusThreshold;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Setting != null)
                {
                    m_Tool.Setting.ConsensusThreshold = value;
                    this.RaisePropertyChanged(nameof(ConsensusThreshold));
                }
            }
        }
        /// <summary>
        /// 선 모델의 선분 길이를 가져오거나 설정합니다.
        /// </summary>
        public double SegmentLength
        {
            get
            {
                if (m_Tool != null && m_Tool.Setting != null) return m_Tool.Setting.SegmentLength;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Setting != null)
                {
                    m_Tool.Setting.SegmentLength = value;
                    this.RaisePropertyChanged(nameof(SegmentLength));
                }
            }
        }
        /// <summary>
        /// 캘리퍼 모델의 개수를 가져오거나 설정합니다.
        /// </summary>
        public int CaliperCount
        {
            get
            {
                if (m_Tool != null && m_Tool.Setting != null) return m_Tool.Setting.CaliperCount;
                else return 0;
            }
            set
            {
                if (m_Tool != null && m_Tool.Setting != null)
                {

                    m_Tool.Setting.CaliperCount = value > 150 ? 150 : (value < 3 ? 3 : value);
                    this.RaisePropertyChanged(nameof(CaliperCount));
                }
            }
        }
        #endregion

        #region Caliper Settings
        /// <summary>
        /// 각 에지 그래픽의 투사 길이를 가져오거나 설정합니다.
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
        /// 각 에지 그래픽의 검색 길이를 가져오거나 설정합니다.
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
        /// 각 에지의 대비 임계값을 가져오거나 설정합니다.
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
        /// 각 에지의 절반 픽셀 크기를 가져오거나 설정합니다.
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
        /// 각 에지를 감지할 방향을 가져오거나 설정합니다.
        /// </summary>
        public int SelectedEdgeDirection
        {
            get
            {
                if (m_Tool != null && m_Tool.Setting != null) return (int)m_Tool.Setting.EdgeDirection;
                else return 0;
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
        #endregion
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
        /// 화면에 출력할 원본 이미지를 가져오거나 설정합니다.
        /// </summary>
        public BitmapSource OriginSource
        {
            get { return m_OriginSource; }
            set
            {
                m_OriginSource = value;
                this.RaisePropertyChanged(nameof(OriginSource));

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
        /// LineDetectToolEditor.SubjectTool의 종속성 속성을 식별합니다.
        /// </summary>
        public static readonly DependencyProperty SubjectToolProperty =
            DependencyProperty.Register(nameof(SubjectTool), typeof(CvsLineDetectTool), typeof(LineDetectToolEditor),
                new PropertyMetadata(new CvsLineDetectTool()));
        /// <summary>
        /// 현재 에디터의 주체가 되는 검사 도구를 가져옵니다.
        /// </summary>
        public CvsLineDetectTool SubjectTool
        {
            get { return GetValue(SubjectToolProperty) as CvsLineDetectTool; }
            set
            {
                SetValue(SubjectToolProperty, value);
                m_Tool = value;
                this.UpdateToolData();
            }
        }
        #endregion

        #endregion

        public LineDetectToolEditor()
        {
            InitializeComponent();
            DataContext = this;
            m_Tool = SubjectTool;
        }


        #region Methods
        private void Editor_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateToolData();
        }
        /// <summary>
        /// 모든 속성 업데이트하기.
        /// </summary>
        private void UpdateToolData()
        {
            m_Tool = SubjectTool;

            this.RaisePropertyChanged(nameof(OriginX));
            this.RaisePropertyChanged(nameof(OriginY));
            this.RaisePropertyChanged(nameof(Radian));
            this.RaisePropertyChanged(nameof(Rotation));
            this.RaisePropertyChanged(nameof(SegmentLength));
            this.RaisePropertyChanged(nameof(CaliperCount));
            this.RaisePropertyChanged(nameof(ConsensusThreshold));

            this.RaisePropertyChanged(nameof(ProjectionLength));
            this.RaisePropertyChanged(nameof(SearchLength));
            this.RaisePropertyChanged(nameof(ContrastThreshold));
            this.RaisePropertyChanged(nameof(HalfPixelCount));
            this.RaisePropertyChanged(nameof(SelectedEdgeDirection));

            this.RaisePropertyChanged(nameof(Overlay));
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
                    m_CurrentBitmap = bmp;
                    var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                    OriginSource = BitmapSource.Create(data.Width, data.Height, bmp.HorizontalResolution, bmp.VerticalResolution, PixelFormats.Gray8, null, data.Scan0, data.Stride * data.Height, data.Stride);
                    OriginSource.Freeze();
                    bmp.UnlockBits(data);
                    if (m_Tool.InputImage != null) m_Tool.InputImage.Dispose();
                    m_Tool.InputImage = m_CurrentBitmap;
                }
            }
            this.RaisePropertyChanged(nameof(Overlay));
            this.RaisePropertyChanged(nameof(Message));
        }
        // 도구 불러오기 콜백
        private void LoadToolBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Crevis Vision Tools. (*.cvt)|*.cvt"
            };
            if ((bool)dialog.ShowDialog())
            {
                m_Tool.Load(dialog.FileName);
                this.UpdateToolData();
            }
        }
        // 도구 저장하기 콜백
        private void SaveToolBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Crevis Vision Tools. (*.cvt)|*.cvt"
            };

            if ((bool)dialog.ShowDialog())
            {
                m_Tool.Save(dialog.FileName);
                this.RaisePropertyChanged(nameof(Message));
            }
        }
        // 검사 실행하기 콜백
        private void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            m_Tool.Run();
            IsEditing = false;

            this.RaisePropertyChanged(nameof(Overlay));
            this.RaisePropertyChanged(nameof(Message));
        }
        #endregion

        #endregion

    }
}
