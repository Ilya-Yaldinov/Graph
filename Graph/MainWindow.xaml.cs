using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
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
        private List<List<int>> adjacencyMatrix = new List<List<int>>();
        private Point? movePoint;

        private bool isCreateBtnOn = false;
        private bool isConnectBtnOn = false;
        private bool isDeleteBtnOn = false;
        private bool isWidthBtnOn = false;
        private bool isHeightBtnOn = false;
        private bool isShortestPathBtnOn = false;

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

        private void widthBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            isWidthBtnOn = !isWidthBtnOn;
            button.Background = isWidthBtnOn == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")) :
                                                       (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
            if (isWidthBtnOn) WidthTraversal();
            else GetBackAllElement();
        }

        private void heightBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            isHeightBtnOn = !isHeightBtnOn;
            button.Background = isHeightBtnOn == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")) :
                                                        (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
            if (isHeightBtnOn) HeightTraversal();
            else GetBackAllElement();
        }

        private void shortestPathBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            isShortestPathBtnOn = !isShortestPathBtnOn;
            button.Background = isShortestPathBtnOn == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")) :
                                                              (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
            if (!isShortestPathBtnOn) GetBackAllElement();
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            movePoint = null;
            ConnectionFigures connectionFigures = ConnectionFigures.GetInstance();
            createFigure = new CreateFigure();
            connectionFigures.Clear();
            connections.Clear();
            MainRoot.Children.Clear();
            adjacencyMatrix.Clear();
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

        private void Connection(object sender, MouseEventArgs args)
        {
            ConnectionFigures connectionFigures = ConnectionFigures.GetInstance();
            if (isConnectBtnOn == false || isDeleteBtnOn == true || isShortestPathBtnOn == true) return;

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
                        if (lineStart == lineEnd)
                        {
                            connectionFigures.Clear();
                            return;
                        }

                connections[connectionFigures.gridFirst].Add(line);
                connections[connectionFigures.gridLast].Add(line);
                MainRoot.Children.Add(line);

                int firstIndex = GetIndexOfGrid(connectionFigures.gridFirst);
                int secondIndex = GetIndexOfGrid(connectionFigures.gridLast);

                AppendAdjacenciesMatrix(firstIndex, secondIndex);

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

        private void AddNodeToCanvas(object sender, MouseButtonEventArgs e)
        {
            if (isCreateBtnOn != true) return;

            Point point = e.GetPosition(MainRoot);
            Grid grid = createFigure.CreateGrid();
            connections.Add(grid, new List<Line>());
            MainRoot.Children.Add(grid);
            Canvas.SetLeft(grid, point.X - 25);
            Canvas.SetTop(grid, point.Y - 25);

            GetAdjacenciesMatrix();

            grid.MouseLeftButtonDown += FigureMouseDown;
            grid.MouseRightButtonDown += Delete;
            grid.MouseMove += FigureMouseMove;
            grid.MouseLeftButtonUp += FigureMouseUp;
            grid.MouseRightButtonDown += Connection;
            grid.MouseRightButtonDown += FindShortestPath;
        }

        private void Delete(object sender, MouseButtonEventArgs e)
        {
            if (isDeleteBtnOn == false || isShortestPathBtnOn == true || isConnectBtnOn == true) return;
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

        private void GetAdjacenciesMatrix()
        {
            for (int i = 0; i < connections.Keys.Count - 1; i++)
            {
                adjacencyMatrix[i].Add(0);
            }
            adjacencyMatrix.Add(new List<int>());
            for (int i = 0; i < connections.Keys.Count; i++)
            {
                adjacencyMatrix[connections.Keys.Count - 1].Add(0);
            }
        }

        private void AppendAdjacenciesMatrix(int firstIndex, int secondIndex)
        {
            adjacencyMatrix[firstIndex][secondIndex] = 1;
            adjacencyMatrix[secondIndex][firstIndex] = 1;
        }

        private int GetIndexOfGrid(Grid grid)
        {
            int index = -1;
            foreach (Grid value in connections.Keys)
            {
                index++;
                if (grid == value)
                    return index;
            }
            return -1;
        }

        private async void WidthTraversal()
        {
            Queue<int> queue = new Queue<int>();
            List<int> nodes = new List<int>();
            for(int i = 0; i < adjacencyMatrix.Count; i++)
            {
                nodes.Add(0);
            }
            queue.Enqueue(0);
            while (queue.Count != 0)
            {
                int node = queue.Dequeue();
                nodes[node] = 2;
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i] == 0 && adjacencyMatrix[node][i] == 1)
                    {
                        queue.Enqueue(i);
                        nodes[i] = 1;
                    }
                }
                await Task.Delay(1000);
                HighlightElements(nodes);
            }
        }

        private async void HeightTraversal()
        {
            Stack<int> stack = new Stack<int>();
            List<int> nodes = new List<int>();
            for (int i = 0; i < adjacencyMatrix.Count; i++)
            {
                nodes.Add(0);
            }
            stack.Push(0);
            while(stack.Count != 0)
            {
                int node = stack.Pop();
                if (nodes[node] == 2) continue;
                nodes[node] = 2;
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    if(adjacencyMatrix[node][i] == 1 && nodes[i] != 2)
                    {
                        stack.Push(i);
                        nodes[i] = 1;
                    }
                }
                await Task.Delay(1000);
                HighlightElements(nodes);
            }
        }

        private async void FindShortestPath(object sender, MouseEventArgs args)
        {
            PathBetweenGrid pathBetweenGrid = PathBetweenGrid.GetInstance();
            if (isShortestPathBtnOn == false || isDeleteBtnOn == true || isConnectBtnOn == true) return;
            Point point = args.GetPosition(MainRoot);

            if (pathBetweenGrid.start.X == 0 && pathBetweenGrid.start.Y == 0)
            {
                pathBetweenGrid.start = point;
                pathBetweenGrid.gridFirst = (Grid)sender;
            }
            else if (pathBetweenGrid.end.X == 0 && pathBetweenGrid.end.Y == 0)
            {
                pathBetweenGrid.end = point;
                pathBetweenGrid.gridLast = (Grid?)sender;
                
                if(pathBetweenGrid.gridFirst == pathBetweenGrid.gridLast)
                {
                    pathBetweenGrid.Clear();
                    return;
                }

                Queue<int> queue = new Queue<int>();
                Stack<Edge> edges = new Stack<Edge>();
                int req;
                Edge edge;
                List<int> nodes = new List<int>();
                for (int i = 0; i < adjacencyMatrix.Count; i++)
                {
                    nodes.Add(0);
                }
                req = GetIndexOfGrid(pathBetweenGrid.gridLast);
                queue.Enqueue(GetIndexOfGrid(pathBetweenGrid.gridFirst));
                while(queue.Count != 0)
                {
                    int node = queue.Dequeue();
                    nodes[node] = 2;
                    for(int i = 0; i < nodes.Count(); i++)
                    {
                        if(adjacencyMatrix[node][i] == 1 && nodes[i] == 0)
                        {
                            queue.Enqueue(i);
                            nodes[i] = 1;
                            edge.begin = node;
                            edge.end = i;
                            edges.Push(edge);
                            if (node == req) break;
                        }
                    }
                }

                while (edges.Count != 0)   
                {
                    edge = edges.Pop();
                    if(edge.end == req)
                    {
                        req = edge.begin;
                        await Task.Delay(1000);
                        HighlightPath(GetGridFromIndex(edge.end));
                    }
                }
                await Task.Delay(1000);
                HighlightPath(GetGridFromIndex(req));
                pathBetweenGrid.Clear();
            }
        }

        private void HighlightPath(Ellipse ellipse)
        {
            ellipse.StrokeThickness = 5;
            ellipse.Fill = Brushes.Orange;
            ellipse.Stroke = Brushes.Gray;
        }

        private void HighlightElements(List<int> nodes)
        {
            for(int i = 0; i < nodes.Count; i++)
            {
                Ellipse ellipse = GetGridFromIndex(i);
                if (nodes[i] == 1)
                {
                    ellipse.Fill = Brushes.Gray;
                }
                if (nodes[i] == 2)
                {
                    ellipse.StrokeThickness = 5;
                    ellipse.Fill = Brushes.Orange;
                    ellipse.Stroke = Brushes.Gray;
                }
            }
        }

        private Ellipse GetGridFromIndex(int node)
        {
            Ellipse ellipse = new Ellipse();
            int count = 0;
            foreach(var grid in connections.Keys)
            {
                if(count == node)
                {
                    foreach(var child in grid.Children)
                    {
                        if(child.GetType() == typeof(Ellipse))
                        {
                            ellipse = (Ellipse)child;
                            return ellipse;
                        }
                    }
                }
                count++;
            }
            return ellipse;
        }

        private void GetBackAllElement()
        {
            foreach (var grid in connections.Keys)
            {
                foreach (var child in grid.Children)
                {
                    if (child.GetType() == typeof(Ellipse))
                    {
                        Ellipse ellipse = (Ellipse)child;
                        ellipse.StrokeThickness = 0;
                        ellipse.Stroke = Brushes.Gray;
                    }
                }
            }
        }
    }
}
