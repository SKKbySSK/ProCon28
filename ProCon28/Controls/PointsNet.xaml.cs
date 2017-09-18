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
using Reactive.Bindings;
using ProCon28.Linker;

namespace ProCon28.Controls
{
    /// <summary>
    /// PointsNet.xaml の相互作用ロジック
    /// </summary>
    public partial class PointsNet : UserControl
    {
        public static DependencyProperty HorizontalPointsProperty { get; }
            = DependencyProperty.Register("HorizontalPoints", typeof(int), typeof(PointsNet),
                new FrameworkPropertyMetadata(10, new PropertyChangedCallback(Points_PropertyChanged)));
        public static DependencyProperty VerticalPointsProperty { get; }
            = DependencyProperty.Register("VerticalPoints", typeof(int), typeof(PointsNet),
                new FrameworkPropertyMetadata(10, new PropertyChangedCallback(Points_PropertyChanged)));

        List<PointRectangle> rect = new List<PointRectangle>();

        public PieceCollection Pieces { get; } = new PieceCollection();

        public int HorizontalPoints
        {
            get { return (int)GetValue(HorizontalPointsProperty); }
            set { SetValue(HorizontalPointsProperty, value); }
        }

        public int VerticalPoints
        {
            get { return (int)GetValue(VerticalPointsProperty); }
            set { SetValue(VerticalPointsProperty, value); }
        }

        public PointsNet()
        {
            InitializeComponent();

            Pieces.CollectionChanged += Pieces_CollectionChanged;
            UpdatePoints();
        }

        private void Pieces_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdatePoints();
        }

        private static void Points_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((PointsNet)obj).UpdatePoints();
        }

        void UpdateLines()
        {
            for (int i = 0; ParentG.Children.Count > i; i++)
            {
                UIElement element = ParentG.Children[i];
                if (element is Line)
                {
                    ParentG.Children.Remove(element);
                    i--;
                }
            }

            foreach (PointRectangle pr in rect)
                pr.Reactangle.Fill = Brushes.Black;
            
            foreach (Piece piece in Pieces)
            {
                for (int i = 0; piece.Vertexes.Count > i; i++)
                {
                    Linker.Point p1;
                    if (i == 0) p1 = piece.Vertexes[piece.Vertexes.Count - 1];
                    else p1 = piece.Vertexes[i - 1];
                    Linker.Point p2 = piece.Vertexes[i];

                    PointRectangle pr1 = FindPoint(p1);
                    PointRectangle pr2 = FindPoint(p2);
                    if (pr1 != null && pr2 != null)
                    {
                        pr1.Reactangle.Fill = Brushes.Orange;
                        pr2.Reactangle.Fill = Brushes.Orange;

                        Line line = new Line();
                        line.Stroke = Brushes.Purple;
                        line.StrokeThickness = 10;
                        line.HorizontalAlignment = HorizontalAlignment.Left;
                        line.VerticalAlignment = VerticalAlignment.Top;
                        line.X1 = pr1.Left + 5;
                        line.Y1 = pr1.Top + 5;
                        line.X2 = pr2.Left + 5;
                        line.Y2 = pr2.Top + 5;
                        ParentG.Children.Insert(0, line);
                    }
                }
            }
        }

        void UpdatePoints()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                int vval = VerticalPoints;
                int hval = HorizontalPoints;
                int total = hval * vval;
                double ew = ActualWidth / (hval + 1);
                double eh = ActualHeight / (vval + 1);

                int rectc = rect.Count;
                if (rectc > total)
                {
                    int diff = rectc - total;
                    for (int i = 0; diff > i; i++)
                    {
                        Rectangle r = rect[0].Reactangle;
                        ParentG.Children.Remove(r);
                        rect.RemoveAt(0);
                    }
                }
                else if (rectc < total)
                {
                    int diff = total - rectc;
                    for (int i = 0; diff > i; i++)
                    {
                        PointRectangle pr = new PointRectangle();
                        ParentG.Children.Add(pr.Reactangle);
                        rect.Add(pr);
                    }
                }

                for (int v = 0; vval > v; v++)
                {
                    for (int h = 0; hval > h; h++)
                    {
                        PointRectangle pr = rect[(v * hval) + h];
                        pr.X = h;
                        pr.Y = v;
                        pr.Reactangle.Margin = new Thickness((h + 1) * ew, (v + 1) * eh, 0, 0);
                    }
                }

                UpdateLines();
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        PointRectangle FindPoint(Linker.Point Point)
        {
            foreach(PointRectangle pr in rect)
            {
                if (Point.X == pr.X && Point.Y == pr.Y)
                    return pr;
            }
            return null;
        }

        PointRectangle FindNearbyPoint(double X, double Y, double Threshold)
        {
            foreach(PointRectangle pr in rect)
            {
                if (InRange(pr.Left, X, Threshold) && InRange(pr.Top, Y, Threshold))
                    return pr;
            }
            return null;
        }

        bool InRange(double X, double Y, double Range)
        {
            return X - Range <= Y && X + Range >= Y;
        }

        private void ParentG_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePoints();
        }

        bool drag = false;
        Linker.Point LastPoint;

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point mouse = e.GetPosition(MouseCanvas);
            PointRectangle pr = FindNearbyPoint(mouse.X, mouse.Y, 20);
            if (pr == null) return;

            drag = true;
            MouseCanvas.CaptureMouse();
            LastPoint = new Linker.Point(pr.X, pr.Y);
        }

        private void MouseCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            drag = false;
            MouseCanvas.ReleaseMouseCapture();
        }

        private void MouseCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag)
            {
                System.Windows.Point mouse = e.GetPosition(MouseCanvas);
                PointRectangle pr = FindNearbyPoint(mouse.X, mouse.Y, 30);
                if (pr == null) return;
                foreach (Piece p in Pieces)
                {
                    for (int i = 0; p.Vertexes.Count > i; i++)
                    {
                        Linker.Point vertex = p.Vertexes[i];
                        if (LastPoint.X == vertex.X && LastPoint.Y == vertex.Y)
                        {
                            vertex = new Linker.Point(pr.X, pr.Y);
                            p.Vertexes[i] = vertex;
                            LastPoint = vertex;
                        }
                    }
                }
                UpdateLines();
            }
        }

        class PointRectangle
        {
            Rectangle rect = new Rectangle();
            public PointRectangle()
            {
                rect.Fill = Brushes.Black;
                rect.Width = 10;
                rect.Height = 10;
                rect.HorizontalAlignment = HorizontalAlignment.Left;
                rect.VerticalAlignment = VerticalAlignment.Top;
            }

            public int X { get; set; }
            public int Y { get; set; }

            public double Left { get { return rect.Margin.Left; } }
            public double Top { get { return rect.Margin.Top; } }

            public Rectangle Reactangle { get { return rect; } }
        }
    }
}
