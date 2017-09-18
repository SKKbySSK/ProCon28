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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Linker.Piece p = new Linker.Piece();
            p.Vertexes.Add(new Linker.Point(0, 0));
            p.Vertexes.Add(new Linker.Point(0, 1));
            p.Vertexes.Add(new Linker.Point(1, 0));
            Console.WriteLine("Angle 0({0} deg):{1}", Math.PI / 2, p.GetAngle(0));
        }
    }
}
