using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static Graph.MainWindow;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace Graph
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CreateFigure createFigure = new CreateFigure();
        private Dictionary<Grid, List<Line>> connections = new Dictionary<Grid, List<Line>>();
        private List<string> nodes = new List<string>();
        private Point? movePoint;

        private bool isCreateBtnOn = false;
        private bool isConnectBtnOn = false;
        private bool isDeleteBtnOn = false;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            isCreateBtnOn = !isCreateBtnOn;
            button.Background = isCreateBtnOn == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")):
                                                        (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
        }

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            isConnectBtnOn = !isConnectBtnOn;
            button.Background = isConnectBtnOn == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")):
                                                         (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            isDeleteBtnOn = !isDeleteBtnOn;
            button.Background = isDeleteBtnOn == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")) :
                                                         (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            movePoint = null;
            ConnectionFigures connectionFigures = ConnectionFigures.GetInstance();
            createFigure = new CreateFigure();
            connectionFigures.Clear();
            connections.Clear();
            MainRoot.Children.Clear();
        }

        private void AddNodeToCanvas(object sender, MouseButtonEventArgs e)
        {
            if (isCreateBtnOn != true) return;

            Point point = e.GetPosition(MainRoot);
            Grid grid = createFigure.CreateGrid();
            connections.Add(grid, new List<Line>());
            MainRoot.Children.Add(grid);
            Canvas.SetLeft(grid, point.X - 25);
            Canvas.SetTop(grid, point.Y - 25);

            grid.MouseLeftButtonDown += FigureMouseDown;
            grid.MouseRightButtonDown += Delete;
            grid.MouseMove += FigureMouseMove;
            grid.MouseLeftButtonUp += FigureMouseUp;
            grid.MouseRightButtonDown += Connection;
        }

        private void Delete(object sender, MouseButtonEventArgs e)
        {
            if (isDeleteBtnOn == false) return;
            if (sender.GetType() == typeof(Grid)) DeleteGrid((Grid)sender);
            else if (sender.GetType() == typeof(Line)) DeleteLine((Line)sender);
            RedrawCanvas();
        }

        private void DeleteGrid(Grid curGrid)
        {
            List<Line> lines = new List<Line>();
            foreach(var grid in connections)
            {
                if(curGrid == grid.Key)
                {
                    lines = grid.Value;
                    connections.Remove(grid.Key);
                    foreach (var line in lines) DeleteLine(line);
                    break;
                }
            }
        }

        private void DeleteLine(Line curLine)
        {
            foreach(var lines in connections.Values)
            {
                foreach(var line in lines)
                {
                    if (line == curLine)
                    {
                        lines.Remove(line);
                        break;
                    }
                }
            }
        }

        private void Connection(object sender, MouseEventArgs args)
        {
            ConnectionFigures connectionFigures = ConnectionFigures.GetInstance();
            if (isConnectBtnOn == false) return;
            connectionFigures.connection = true;

            Point point = args.GetPosition(MainRoot);

            if (connectionFigures.start.X == 0 && connectionFigures.start.Y == 0)
            {
                connectionFigures.start = point;
                connectionFigures.gridFirst = (Grid)sender;
            }

            else if (connectionFigures.end.X == 0 && connectionFigures.end.Y == 0)
            {
                connectionFigures.gridLast = (Grid)sender;
                connectionFigures.end = point;

                Line line = createFigure.CreateLine();

                line.X1 = connectionFigures.start.X;
                line.Y1 = connectionFigures.start.Y;
                line.X2 = connectionFigures.end.X;
                line.Y2 = connectionFigures.end.Y;

                if (connectionFigures.gridFirst == connectionFigures.gridLast)
                {
                    connectionFigures.Clear();
                    return;
                }

                foreach (Line lineStart in connections[connectionFigures.gridFirst])
                    foreach (Line lineEnd in connections[connectionFigures.gridLast])
                        if (lineStart == lineEnd) return;

                connections[connectionFigures.gridFirst].Add(line);
                connections[connectionFigures.gridLast].Add(line);
                MainRoot.Children.Add(line);

                line.MouseRightButtonDown += Delete;
                RedrawCanvas();
                connectionFigures.Clear();
            }

        }

        private void RedrawCanvas()
        {
            MainRoot.Children.Clear();

            foreach (var keyValuePair in connections)
            {
                foreach (Line line in keyValuePair.Value)
                {
                    if (!MainRoot.Children.Contains(line))
                    {
                        MainRoot.Children.Add(line);
                    }
                }
                MainRoot.Children.Add(keyValuePair.Key);
            }
        }

        private void FigureMouseUp(object sender, MouseButtonEventArgs args)
        {
            Grid grid = (Grid)sender;
            movePoint = null;
            grid.ReleaseMouseCapture();
        }

        private void FigureMouseMove(object sender, MouseEventArgs args)
        {
            Grid grid = (Grid)sender;

            if (movePoint == null) return; 

            Point point = args.GetPosition(MainRoot) - (Vector)movePoint.Value;

            Canvas.SetLeft(grid, point.X);
            Canvas.SetTop(grid, point.Y);

            foreach (Line line in connections[grid])
            {
                double line1 = Math.Sqrt(Math.Pow(point.X + grid.ActualWidth / 2 - line.X1, 2) + Math.Pow(point.Y + grid.ActualHeight / 2 - line.Y1, 2));
                double line2 = Math.Sqrt(Math.Pow(point.X + grid.ActualWidth / 2 - line.X2, 2) + Math.Pow(point.Y + grid.ActualHeight / 2 - line.Y2, 2));

                if (line1 < line2)
                {
                    line.X1 = point.X + grid.ActualHeight / 2;
                    line.Y1 = point.Y + grid.ActualHeight / 2;
                }
                else
                {
                    line.X2 = point.X + grid.ActualHeight / 2;
                    line.Y2 = point.Y + grid.ActualHeight / 2;
                }
            }
        }

        private void FigureMouseDown(object sender, MouseButtonEventArgs args)
        {
            Grid grid = (Grid)sender;
            movePoint = args.GetPosition(grid);
            grid.CaptureMouse();
        }

        private void widthBtn_Click(object sender, RoutedEventArgs e)
        {
            string firstNode = GetHighest();
        }

        private void GetNodesQueue(string firstNode)
        {
            
        }

        private string GetHighest() 
        {
            double minHeight = double.MaxValue;
            string number = "";
            foreach (var grid in connections.Keys)
            {
                Point position = grid.PointToScreen(new Point(0d, 0d));
                if (position.Y < minHeight)
                {
                    minHeight = position.Y;
                    number = GetNumberOfNode(grid);
                }
            }
            return number;
        }

        private string GetNumberOfNode(Grid grid)
        {
            string numOfNode = "";
            foreach (var children in grid.Children)
            {
                if (children.GetType() == typeof(TextBlock))
                {
                    TextBlock textBlock = (TextBlock)children;
                    numOfNode = textBlock.Text;
                }
            }
            return numOfNode;
        }
       
        /*private void HighlightElements()
        {
            foreach (var grid in connections.Keys)
            {
                foreach (var children in grid.Children)
                {
                    if (children.GetType() == typeof(Ellipse))
                    {
                        Ellipse ellipse = (Ellipse)children;
                        ellipse.StrokeThickness = 5;
                        ellipse.Stroke = Brushes.Gray;
                    }
                }
            }
        }*/
    }
}
