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
using ProCon28.Linker.Extensions;
using System.IO;

namespace ProCon28.Windows
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class Main : Window
    {
        public Main()
        {
            InitializeComponent();
            PieceG.VertexAdded += PieceG_VertexChanged;
            PieceG.VertexRemoved += PieceG_VertexChanged;
            PieceG.VertexMoved += PieceG_VertexChanged;
        }

        private void PieceG_VertexChanged(object sender, EventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Linker.Piece p = new Linker.Piece();

            Random rnd = new Random();
            int count = rnd.Next(3, 5);

            for(int i = 0; count > i; i++)
            {
                Linker.Point point = new Linker.Point(new Random(Environment.TickCount + 1 + (i * i)).Next(0, 30), new Random(Environment.TickCount + 2 + (i * i)).Next(0, 30));
                p.Vertexes.Add(point);
            }
            
            for(int i = 0;p.Vertexes.Count > i; i++)
            {
                Console.WriteLine("Angle 0({0} deg):{1}", Math.PI / 2, p.GetAngle(i));
            }

            PieceG.Piece = p;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (PieceG.Piece == null) return;
            PieceG.Piece.SortVertexes(Linker.PointSortation.Clockwise);

            Linker.Piece p = PieceG.Piece.Convert();
            PieceG.Piece = p;
        }

        private void AddB_Click(object sender, RoutedEventArgs e)
        {
            if (PieceList.Pieces.Contains(PieceG.Piece))
                PieceList.Pieces.Remove(PieceG.Piece);
            
            PieceList.Pieces.Add(PieceG.Piece);
        }

        private void PieceList_SelectedPieceChanged(object sender, EventArgs e)
        {
            if(PieceList.SelectedPiece != null)
            {
                PieceG.Piece = PieceList.SelectedPiece;
            }
        }

        private void TransportB_Click(object sender, RoutedEventArgs e)
        {
            Linker.Tcp.RemotePieces ps = new Linker.Tcp.RemotePieces();
            ps.BytePieces = PieceList.Pieces.AsBytes();

            MarshalDialog dialog = new MarshalDialog(ps);
            dialog.ShowDialog();
        }

        private void LoadB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Linker.DoublePoint> points = new List<Linker.DoublePoint>();
                using (StreamReader sr = new StreamReader(LoadT.Text))
                {
                    string line = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] pair = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        double x = double.Parse(pair[0]);
                        double y = double.Parse(pair[1]);
                        points.Add(new Linker.DoublePoint(x / 100, y / 100));
                    }
                }

                Linker.Piece piece = new Linker.Piece();
                foreach (Linker.DoublePoint dp in points)
                {
                    Linker.Point p = new Linker.Point(dp);
                    if (!piece.Vertexes.Contains(p))
                        piece.Vertexes.Add(p);
                }
                piece = piece.Convert();
                piece.SortVertexes(Linker.PointSortation.Clockwise);

                double thre = Math.PI / 2;
                List<Linker.Point> sRem = new List<Linker.Point>();
                for (int i = 0;piece.Vertexes.Count > i; i++)
                {
                    double angle = piece.GetAngle(i);
                    double dangle = Math.PI - angle;
                    if (dangle > thre || Math.Abs(angle - Math.PI) <= 0.01)
                    {
                        sRem.Add(piece.Vertexes[i]);
                    }
                }

                foreach (Linker.Point p in sRem)
                    piece.Vertexes.Remove(p);

                PieceG.Piece = piece;
            }
            catch (Exception) { }
        }
    }
}
