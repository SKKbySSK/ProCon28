﻿using System;
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
using ProCon28.Linker;
using ProCon28.Linker.Extensions;

namespace ProCon28.Controls
{
    /// <summary>
    /// PieceView.xaml の相互作用ロジック
    /// </summary>
    public partial class PieceView : UserControl
    {
        public PieceView()
        {
            InitializeComponent();
        }

        Piece p;
        public Piece Piece
        {
            get { return p; }
            set
            {
                p = value;
                RedrawPiece();
            }
        }

        public void RedrawPiece()
        {
            if (Piece == null)
            {
                PImage.Source = null;
                PLabel.Content = null;
            }
            else
            {
                PImage.Source = CreateImage(Piece);

                int w = 0, h = 0, mw = -1, mh = -1;
                foreach(Linker.Point p in Piece.Vertexes)
                {
                    w = p.X > w ? p.X : w;
                    h = p.Y > h ? p.Y : h;
                    mw = p.X < mw || mw == -1 ? p.X : mw;
                    mh = p.Y < mh || mh == -1 ? p.Y : mh;
                }

                if(Piece is Linker.Frame)
                {
                    PLabel.Foreground = Brushes.Black;
                    PLabel.Content = string.Format("フレーム W:{0}, H:{1}", w - mw, h - mh);
                }
                else
                {
                    PLabel.Foreground = Piece.Vertexes.Count > Constants.MaximumVertex ? Brushes.Red : Brushes.Black;
                    PLabel.Content = string.Format("V:{0}, W:{1}, H:{2}", Piece.Vertexes.Count, w - mw, h - mh);
                }
            }
        }

        int _sample = 10;
        public int Sample
        {
            get { return _sample; }
            set { _sample = value; }
        }

        ImageSource CreateImage(Piece Piece)
        {
            GeometryGroup gg = new GeometryGroup();

            if(Piece is CompositePiece cp)
            {
                foreach(Piece p in cp.Source)
                {
                    var paths = new GeometryGroup();
                    var lines = p.Vertexes.AsLines();
                    foreach (var line in lines)
                        paths.Children.Add(new LineGeometry(
                            new System.Windows.Point(line.Item1.X * Sample, line.Item1.Y * Sample),
                            new System.Windows.Point(line.Item2.X * Sample, line.Item2.Y * Sample)));
                    gg.Children.Add(paths);
                }
            }
            else
            {
                var lines = Piece.Vertexes.AsLines();
                foreach (var line in lines)
                    gg.Children.Add(new LineGeometry(
                        new System.Windows.Point(line.Item1.X * Sample, line.Item1.Y * Sample),
                        new System.Windows.Point(line.Item2.X * Sample, line.Item2.Y * Sample)));
            }

            DrawingImage di = new DrawingImage(new GeometryDrawing(Brushes.Transparent, new Pen(Brushes.Black, 1), gg));
            di.Freeze();

            return di;
        }
    }
}
