using System;
using System.Collections.Generic;
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

namespace ProCon28.Controls
{
    /// <summary>
    /// PieceGenerator.xaml の相互作用ロジック
    /// </summary>
    public partial class PieceGenerator : UserControl
    {
        bool Initializing = false;
        public PieceGenerator()
        {
            Initializing = true;
            InitializeComponent();

            Net.VertexAdded += (sender, e) => VertexAdded?.Invoke(this, e);
            Net.VertexRemoved += (sender, e) => VertexRemoved?.Invoke(this, e);
            Net.VertexMoved += (sender, e) => VertexMoved?.Invoke(this, e);
            Initializing = false;
        }

        public event EventHandler VertexAdded;
        public event EventHandler VertexRemoved;
        public event EventHandler VertexMoved;

        public Linker.Piece Piece
        {
            get { return Net.Piece; }
            set { Net.Piece = value; }
        }

        public void RedrawPiece()
        {
            Net.RedrawPiece();
        }

        public void Fit()
        {
            if(Piece != null)
            {

            }
        }

        private void KeepRatioC_Checked(object sender, RoutedEventArgs e)
        {
            if (Initializing) return;
            int max = Math.Max((int)SliderH.Value, (int)SliderV.Value);
            SliderH.Value = max;
            SliderV.Value = max;
        }

        bool Ignore = false;
        private void SliderH_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Initializing || Ignore) return;
            if (KeepRatioC.IsChecked ?? false)
            {
                Ignore = true;
                SliderV.Value = SliderH.Value;
                Ignore = false;
            }
        }

        private void SliderV_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Initializing || Ignore) return;
            if (KeepRatioC.IsChecked ?? false)
            {
                Ignore = true;
                SliderH.Value = SliderV.Value;
                Ignore = false;
            }
        }
    }
}
