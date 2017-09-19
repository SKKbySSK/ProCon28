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
        public PieceGenerator()
        {
            InitializeComponent();
        }

        public Linker.Piece Piece
        {
            get { return Net.Piece; }
            set { Net.Piece = (Linker.Piece)value.Clone(); }
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
    }
}
