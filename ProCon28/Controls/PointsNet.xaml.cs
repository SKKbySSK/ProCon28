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
        public static DependencyProperty MaximumVertexCountProperty { get; }
            = DependencyProperty.Register("MaximumVertexCount", typeof(int), typeof(PointsNet),
                new FrameworkPropertyMetadata(-1, new PropertyChangedCallback(Points_PropertyChanged)));

        public event EventHandler VertexAdded;
        public event EventHandler VertexRemoved;
        public event EventHandler VertexMoved;

       double Vspace = 0, Hspace = 0;

        List<PointRectangle> rect = new List<PointRectangle>();

        Piece _piece = null;
        public Piece Piece
        {
            get { return _piece; }
            set
            {
                _piece = value;
                UpdateLines();
            }
        }

        public void RedrawPiece()
        {
            UpdateLines();
        }
        
        public int MaximumVertexCount
        {
            get { return (int)GetValue(MaximumVertexCountProperty); }
            set
            {
                SetValue(MaximumVertexCountProperty, value);
                UpdateLines();
            }
        }

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

            if (Piece == null) return;

            int count = MaximumVertexCount > -1 ? Math.Min(Piece.Vertexes.Count, MaximumVertexCount) : Piece.Vertexes.Count;
            for (int i = 0; count > i; i++)
            {
                Linker.Point p1;
                if (i == 0) p1 = Piece.Vertexes[count - 1];
                else p1 = Piece.Vertexes[i - 1];
                Linker.Point p2 = Piece.Vertexes[i];

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

        void UpdatePoints()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                int vval = VerticalPoints;
                int hval = HorizontalPoints;
                int total = hval * vval;
                double ew = ActualWidth / (hval + 1);
                double eh = ActualHeight / (vval + 1);

                Hspace = eh;
                Vspace = ew;

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

        PointRectangle FindNearbyPoint(double X, double Y, double ThresholdX, double ThresholdY)
        {
            foreach(PointRectangle pr in rect)
            {
                if (InRange(pr.Left - (pr.Width / 2), X, ThresholdX) && InRange(pr.Top - (pr.Height / 2), Y, ThresholdY))
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
        int vertex = -1;

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point mouse = e.GetPosition(MouseCanvas);
            PointRectangle pr = FindNearbyPoint(mouse.X, mouse.Y, Hspace / 2, Vspace / 2);
            if (pr == null || Piece == null) return;

            if(e.LeftButton == MouseButtonState.Pressed)
            {
                drag = true;
                vertex = -1;
                MouseCanvas.CaptureMouse();
                for(int i = 0;Piece.Vertexes.Count > i; i++)
                {
                    Linker.Point point = Piece.Vertexes[i];
                    if (point.X == pr.X && point.Y == pr.Y)
                    {
                        vertex = i;
                        break;
                    }
                }
            }
            else if(e.MiddleButton == MouseButtonState.Pressed)
            {
                Piece.Vertexes.Add(new Linker.Point(pr.X, pr.Y));
                VertexAdded?.Invoke(this, new EventArgs());
                UpdateLines();
            }
            else if(e.RightButton == MouseButtonState.Pressed)
            {
                for (int i = 0; Piece.Vertexes.Count > i; i++)
                {
                    Linker.Point point = Piece.Vertexes[i];
                    if (point.X == pr.X && point.Y == pr.Y)
                    {
                        Piece.Vertexes.RemoveAt(i);
                        VertexRemoved?.Invoke(this, new EventArgs());
                    }
                }
                UpdateLines();
            }
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
                PointRectangle pr = FindNearbyPoint(mouse.X, mouse.Y, Hspace / 2, Vspace / 2);
                if (pr == null || Piece == null || vertex < 0) return;

                Piece.Vertexes[vertex] = new Linker.Point(pr.X, pr.Y);
                VertexMoved?.Invoke(this, new EventArgs());
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

            public double Width
            {
                get { return rect.Width; }
                set { rect.Width = value; }
            }

            public double Height
            {
                get { return rect.Height; }
                set { rect.Height = value; }
            }

            public int X { get; set; }
            public int Y { get; set; }

            public double Left { get { return rect.Margin.Left; } }
            public double Top { get { return rect.Margin.Top; } }

            public Rectangle Reactangle { get { return rect; } }
        }
    }
}
