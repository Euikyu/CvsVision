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

namespace CvsVision_TESTApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _TEXT;

        public CvsVision.Caliper.CvsEdgeDetectTool Tool { get; set; }
        public string TEXT { get => _TEXT; set { _TEXT = value; R(nameof(TEXT)); } }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void R(string p)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        }

        private void EdgeDetectToolEditor_Loaded(object sender, RoutedEventArgs e)
        {
            //var m_Tool = new CvsVision.Caliper.CvsEdgeDetectTool();
            //m_Tool.Load(@"C:\Users\crevis_TS 박의규\Desktop\edge.cvt");
            //Tool = m_Tool;
            //R(nameof(Tool));

            //TEXT = "AAAAAAAAAAAAAA";
            //R(nameof(TEXT));
        }
    }
}
