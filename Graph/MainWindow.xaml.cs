using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
        private Dictionary<Grid, List<Polyline>> connections = new Dictionary<Grid, List<Polyline>>();
        private Dictionary<Polyline, Label> pathCosts = new Dictionary<Polyline, Label>();
        public static List<List<int>> adjacencyMatrix = new List<List<int>>();
        private Point? movePoint;
        private List<string> logger = new List<string>();

        private bool isCreateBtnOn = false;
        private bool isConnectBtnOn = false;
        private bool isDeleteBtnOn = false;
        private bool isWidthBtnOn = false;
        private bool isHeightBtnOn = false;
        private bool isShortestPathBtnOn = false;
        private bool isDirectionConnection = false;
        private bool isFFBtnOn = false;

        public MainWindow()
        {
            InitializeComponent();
            ArrowLine arrowLine = new();
            arrowLine.Stroke = Brushes.Black;
            arrowLine.StrokeThickness = 2;
            arrowLine.X1= 20;
            arrowLine.Y1= 30;
            arrowLine.X2= 80;
            arrowLine.Y2= 100;
            arrowLine.ArrowLength = 10;
            arrowLine.ArrowAngle = 45;
            arrowLine.ArrowEnds = ArrowEnds.End;

            MainRoot.Children.Add(arrowLine);
        }

        #region All Buttons Click
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

        private void openFileBtn_Click(object sender, RoutedEventArgs e)
        {
            ReadFromFile();
        }

        private void saveToFileBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveToNewFile();
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
            pathCosts.Clear();
            textBlock.Text = string.Empty;
        }
        #endregion

        #region Actions With Grid
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

            foreach (Polyline line in connections[grid])
            {
                double line1 = Math.Sqrt(Math.Pow(point.X + grid.ActualWidth / 2 - line.Points[0].X, 2) + Math.Pow(point.Y + grid.ActualHeight / 2 - line.Points[0].Y, 2));
                double line2 = Math.Sqrt(Math.Pow(point.X + grid.ActualWidth / 2 - line.Points[1].X, 2) + Math.Pow(point.Y + grid.ActualHeight / 2 - line.Points[1].Y, 2));

                Label tmp = pathCosts[line];
                Point mdl = new Point();

                if (line1 < line2)
                {
                    Point pointT = new Point();
                    pointT.X = point.X + grid.ActualHeight / 2;
                    pointT.Y = point.Y + grid.ActualHeight / 2;
                    line.Points[0] = pointT;

                    pointT.X = (line.Points[0].X + line.Points[1].X) / 2;
                    pointT.Y = (line.Points[0].Y + line.Points[1].Y) / 2;

                    mdl.X = pointT.X;
                    mdl.Y = pointT.Y;

                    if (line.Points.Count > 2)
                    {
                        line.Points[2] = pointT;

                        pointT.X = line.Points[2].X - 10;
                        pointT.Y = line.Points[2].Y - 10;
                        line.Points[3] = pointT;

                        pointT.X = line.Points[2].X - 10;
                        pointT.Y = line.Points[2].Y + 10;
                        line.Points[4] = pointT;

                        pointT.X = line.Points[2].X;
                        pointT.Y = line.Points[2].Y;
                        line.Points[5] = pointT;
                    }

                    //pathCosts.Remove(line);
                    //tmp.Margin = new System.Windows.Thickness(pointT.X - 10, pointT.Y - 20, 0, 0);
                    //pathCosts.Add(line, tmp);
                    //pathCosts[line].Margin = new System.Windows.Thickness(pointT.X - 10, pointT.Y - 20, 0, 0);
                }
                else
                {
                    Point pointT = new Point();
                    pointT.X = point.X + grid.ActualHeight / 2;
                    pointT.Y = point.Y + grid.ActualHeight / 2;
                    line.Points[1] = pointT;

                    pointT.X = (line.Points[0].X + line.Points[1].X) / 2;
                    pointT.Y = (line.Points[0].Y + line.Points[1].Y) / 2;

                    mdl.X = pointT.X;
                    mdl.Y = pointT.Y;

                    if (line.Points.Count > 2)
                    {
                        line.Points[2] = pointT;

                        pointT.X = line.Points[2].X - 10;
                        pointT.Y = line.Points[2].Y - 10;
                        line.Points[3] = pointT;

                        pointT.X = line.Points[2].X - 10;
                        pointT.Y = line.Points[2].Y + 10;
                        line.Points[4] = pointT;

                        pointT.X = line.Points[2].X;
                        pointT.Y = line.Points[2].Y;
                        line.Points[5] = pointT;
                    }

                    //pathCosts.Remove(line);
                    //tmp.Margin = new System.Windows.Thickness(pointT.X - 10, pointT.Y - 20, 0, 0);
                    //pathCosts.Add(line, tmp);              
                }

                pathCosts[line].Margin = new System.Windows.Thickness(mdl.X - 10, mdl.Y - 20, 0, 0);
            }
        }

        private void FigureMouseDown(object sender, MouseButtonEventArgs args)
        {
            Grid grid = (Grid)sender;
            movePoint = args.GetPosition(grid);
            grid.CaptureMouse();
        }

        private void AddGridToCanvas(object sender, MouseButtonEventArgs e)
        {
            if (isCreateBtnOn != true) return;

            Point point = e.GetPosition(MainRoot);
            Grid grid = createFigure.CreateGrid();
            connections.Add(grid, new List<Polyline>());
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

            grid.MouseRightButtonDown += DirectionConnection;
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

                Polyline line = createFigure.CreateLine();

                Point pOne = new Point();
                Point pTwo = new Point();

                pOne.X = connectionFigures.start.X;
                pOne.Y = connectionFigures.start.Y;
                pTwo.X = connectionFigures.end.X;
                pTwo.Y = connectionFigures.end.Y;

                line.Points.Add(pOne);
                line.Points.Add(pTwo);

                if (connectionFigures.hasDirection)
                {
                    Point mArrow = new Point();
                    Point pRarrow = new Point();
                    Point pLarrow = new Point();
                    Point сArrow = new Point();

                    mArrow.X = (pOne.X + pTwo.X) / 2;
                    mArrow.Y = (pOne.Y + pTwo.Y) / 2;

                    pRarrow.X = mArrow.X - 10;
                    pRarrow.Y = mArrow.Y - 10;
                    pLarrow.X = mArrow.X - 10;
                    pLarrow.Y = mArrow.Y + 10;
                    сArrow.X = mArrow.X;
                    сArrow.Y = mArrow.Y;

                    line.Points.Add(mArrow);
                    line.Points.Add(pRarrow);
                    line.Points.Add(pLarrow);
                    line.Points.Add(сArrow);
                }

                if (connectionFigures.gridFirst == connectionFigures.gridLast)
                {
                    connectionFigures.Clear();
                    return;
                }

                foreach (Polyline lineStart in connections[connectionFigures.gridFirst])
                    foreach (Polyline lineEnd in connections[connectionFigures.gridLast])
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
        #endregion

        #region Delete
        private void Delete(object sender, MouseButtonEventArgs e)
        {
            if (isDeleteBtnOn == false || isShortestPathBtnOn == true || isConnectBtnOn == true) return;
            if (sender.GetType() == typeof(Grid)) DeleteGrid((Grid)sender);
            else if (sender.GetType() == typeof(Polyline)) DeleteLine((Polyline)sender);
            RedrawCanvas();
        }

        private void DeleteGrid(Grid curGrid)
        {
            List<Polyline> lines = new List<Polyline>();
            foreach (var grid in connections)
            {
                if (curGrid == grid.Key)
                {
                    lines = grid.Value;
                    connections.Remove(grid.Key);
                    foreach (var line in lines) DeleteLine(line);
                    break;
                }
            }
        }

        private void DeleteLine(Polyline curLine)
        {
            foreach (var lines in connections.Values)
            {
                foreach (var line in lines)
                {
                    if (line == curLine)
                    {
                        lines.Remove(line);
                        break;
                    }
                }
            }
        }
        #endregion

        #region Write To File
        private void SaveToNewFile()
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.FileName = "Graphs";
            saveFile.DefaultExt = ".csv";
            saveFile.Filter = "Text documents (.csv)|*.csv";
            Nullable<bool> result = saveFile.ShowDialog();

            if (result == true)
            {
                string filename = saveFile.FileName;
                StreamWriter sw = new StreamWriter(filename);
                int count = 0;

                foreach (List<int> row in adjacencyMatrix)
                {
                    row.ForEach(x => sw.Write($"{x};"));
                    sw.Write($"---;{GetPositionOfGridToString(count++)}");
                    sw.WriteLine();
                }
                sw.Close();
            }
        }

        private string GetPositionOfGridToString(int count)
        {
            Grid grid = (Grid)GetEllipseFromIndex(count).Parent;
            double posX = Canvas.GetLeft(grid);
            double posY = Canvas.GetTop(grid);
            return $"{Math.Round(posX, 2)};{Math.Round(posY, 2)}";
        }
        #endregion

        #region Read From File
        private void ReadFromFile()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Text documents (.csv)|*.csv";
            Nullable<bool> result = openFile.ShowDialog();

            if (result == true)
            {
                string fileName = openFile.FileName;
                InfoFromFileToCanvas(fileName);
                RedrawCanvas();
            }
        }

        private void InfoFromFileToCanvas(string fileName)
        {
            string[] file = File.ReadAllLines(fileName);
            adjacencyMatrix.Clear();
            foreach (string line in file)
            {
                List<string> elems = line.Split('-')
                    .Where(x => !String.IsNullOrWhiteSpace(x))
                    .ToList();
                List<int> row = elems[0].Split(';')
                    .Where(x => !String.IsNullOrWhiteSpace(x))
                    .Select(x => Convert.ToInt32(x))
                    .ToList();
                adjacencyMatrix.Add(row);
                List<double> positions = elems[1].Split(';')
                    .Where(x => !String.IsNullOrWhiteSpace(x))
                    .Select(x => Convert.ToDouble(x))
                    .ToList();
                AddGridToCanvasFromFile(positions);
            }
            GetConnectionFromFile();
        }

        private void AddGridToCanvasFromFile(List<double> positions)
        {
            Grid grid = createFigure.CreateGrid();
            connections.Add(grid, new List<Polyline>());
            MainRoot.Children.Add(grid);
            Canvas.SetLeft(grid, positions[0]);
            Canvas.SetTop(grid, positions[1]);

            grid.MouseLeftButtonDown += FigureMouseDown;
            grid.MouseRightButtonDown += Delete;
            grid.MouseMove += FigureMouseMove;
            grid.MouseLeftButtonUp += FigureMouseUp;
            grid.MouseRightButtonDown += Connection;
            grid.MouseRightButtonDown += DirectionConnection;
            grid.MouseRightButtonDown += FindShortestPath;
        }

        private void GetConnectionFromFile()
        {
            for (int i = 0; i < adjacencyMatrix.Count; i++)
            {
                for (int j = 0; j < adjacencyMatrix[i].Count; j++)
                {
                    if (adjacencyMatrix[i][j] >= 1)
                    {
                        Grid grid1 = (Grid)GetEllipseFromIndex(i).Parent;
                        Grid grid2 = (Grid)GetEllipseFromIndex(j).Parent;
                        CreateFigure createFigure = new CreateFigure();

                        Polyline line = createFigure.CreateLine();
                        Point pOne = new();
                        pOne.X = Canvas.GetLeft(grid1) + 25;
                        pOne.Y = Canvas.GetTop(grid1) + 25;
                        Point pTwo = new();
                        pTwo.X = Canvas.GetLeft(grid2) + 25;
                        pTwo.Y = Canvas.GetTop(grid2) + 25;

                        Point mArrow = new();
                        mArrow.X = (pOne.X + pTwo.X) / 2;
                        mArrow.Y = (pOne.Y + pTwo.Y) / 2;

                        line.Points.Add(pOne);
                        line.Points.Add(pTwo);

                        foreach (Polyline lineStart in connections[grid1])
                            foreach (Polyline lineEnd in connections[grid2])
                                if (lineStart == lineEnd) continue;

                        connections[grid1].Add(line);
                        connections[grid2].Add(line);

                        Label pCost = new Label { Margin = new System.Windows.Thickness(mArrow.X - 10, mArrow.Y - 20, 0, 0), Content = adjacencyMatrix[i][j] };
                        pathCosts.Add(line, pCost);

                        MainRoot.Children.Add(line);
                        line.MouseRightButtonDown += Delete;
                    }
                }
            }
            RedrawCanvas();
        }
        #endregion

        #region Graph Traversal
        private async void WidthTraversal()
        {
            Queue<int> queue = new Queue<int>();
            List<int> nodes = new List<int>();
            for (int i = 0; i < adjacencyMatrix.Count; i++)
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
            while (stack.Count != 0)
            {
                int node = stack.Pop();
                if (nodes[node] == 2) continue;
                nodes[node] = 2;
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    if (adjacencyMatrix[node][i] == 1 && nodes[i] != 2)
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

                if (pathBetweenGrid.gridFirst == pathBetweenGrid.gridLast)
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
                while (queue.Count != 0)
                {
                    int node = queue.Dequeue();
                    nodes[node] = 2;
                    for (int i = 0; i < nodes.Count(); i++)
                    {
                        if (adjacencyMatrix[node][i] == 1 && nodes[i] == 0)
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
                    if (edge.end == req)
                    {
                        req = edge.begin;
                        await Task.Delay(1000);
                        HighlightPath(GetEllipseFromIndex(edge.end));
                    }
                }
                await Task.Delay(1000);
                HighlightPath(GetEllipseFromIndex(req));
                pathBetweenGrid.Clear();
            }
        }
        #endregion

        #region Actions With Collections
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

        private Ellipse GetEllipseFromIndex(int node)
        {
            Ellipse ellipse = new Ellipse();
            int count = 0;
            foreach (var grid in connections.Keys)
            {
                if (count == node)
                {
                    foreach (var child in grid.Children)
                    {
                        if (child.GetType() == typeof(Ellipse))
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
        #endregion

        #region Redrawing
        private void RedrawCanvas()
        {
            MainRoot.Children.Clear();

            foreach (var keyValuePair in connections)
            {
                foreach (Polyline line in keyValuePair.Value)
                {
                    if (!MainRoot.Children.Contains(line))
                    {
                        MainRoot.Children.Add(line);
                        if (pathCosts.ContainsKey(line))
                            MainRoot.Children.Add(pathCosts[line]);
                    }
                }
                MainRoot.Children.Add(keyValuePair.Key);
            }
        }

        private void HighlightPath(Ellipse ellipse)
        {
            ellipse.StrokeThickness = 5;
            ellipse.Fill = Brushes.Orange;
            ellipse.Stroke = Brushes.Gray;
        }

        private async void HighlightElements(List<int> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Ellipse ellipse = GetEllipseFromIndex(nodes[i] - 1);
                ellipse.Fill = Brushes.Gray;
                await Task.Delay(500);
            }
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
                        ellipse.Fill = Brushes.Orange;
                    }
                }
            }
        }
        #endregion


        private void FFBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            isFFBtnOn = !isFFBtnOn;
            button.Background = isFFBtnOn == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")) :
                                                    (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
            if (isFFBtnOn) GetFordFulkerson(0, adjacencyMatrix[0].Count - 1);
            else
            {
                GetBackAllElement();
                textBlock.Text = string.Empty;
            }
        }

        private void dirConnectBtn_Click(object sender, RoutedEventArgs args)
        {
            Button button = sender as Button;
            isDirectionConnection = !isDirectionConnection;
            button.Background = isDirectionConnection == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")) :
                                                                (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
        }

        private void DirectionConnection(object sender, MouseEventArgs args)
        {
            ConnectionFigures connectionFigures = ConnectionFigures.GetInstance();
            if (isConnectBtnOn == true || isDeleteBtnOn == true || isShortestPathBtnOn == true || isDirectionConnection == false) return;

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

                Polyline line = createFigure.CreateLine();

                Point pOne = new Point();
                Point pTwo = new Point();

                pOne.X = connectionFigures.start.X - 10;
                pOne.Y = connectionFigures.start.Y - 10;
                pTwo.X = connectionFigures.end.X;
                pTwo.Y = connectionFigures.end.Y;

                line.Points.Add(pOne);
                line.Points.Add(pTwo);

                Point mArrow = new Point();
                Point pRarrow = new Point();
                Point pLarrow = new Point();
                Point сArrow = new Point();

                mArrow.X = (pOne.X + pTwo.X) / 2;
                mArrow.Y = (pOne.Y + pTwo.Y) / 2;

                pRarrow.X = mArrow.X - 10;
                pRarrow.Y = mArrow.Y - 10;
                pLarrow.X = mArrow.X - 10;
                pLarrow.Y = mArrow.Y + 10;
                сArrow.X = mArrow.X;
                сArrow.Y = mArrow.Y;

                line.Points.Add(mArrow);
                line.Points.Add(pRarrow);
                line.Points.Add(pLarrow);
                line.Points.Add(сArrow);

                if (connectionFigures.gridFirst == connectionFigures.gridLast)
                {
                    connectionFigures.Clear();
                    return;
                }

                foreach (Polyline lineStart in connections[connectionFigures.gridFirst])
                    foreach (Polyline lineEnd in connections[connectionFigures.gridLast])
                        if (lineStart == lineEnd)
                        {
                            connectionFigures.Clear();
                            return;
                        }

                connections[connectionFigures.gridFirst].Add(line);
                connections[connectionFigures.gridLast].Add(line);
                MainRoot.Children.Add(line);

                int firstIndex  = GetIndexOfGrid(connectionFigures.gridFirst);
                int secondIndex = GetIndexOfGrid(connectionFigures.gridLast );
                RedrawCanvas();

                SetPathCostWindow setPathCostWindow = new();
                setPathCostWindow.ShowDialog();
                connectionFigures.cost = setPathCostWindow.pathCost;

                if (setPathCostWindow.pathCost != 0)
                    AppendDirectionMatrix(firstIndex, secondIndex, setPathCostWindow.pathCost);

                Label pCost = new Label { Margin = new System.Windows.Thickness(mArrow.X - 10, mArrow.Y - 20,0,0), Content = connectionFigures.cost};
                pathCosts.Add(line, pCost);

                line.MouseRightButtonDown += Delete;
                RedrawCanvas();
                connectionFigures.Clear();
            }
        }

        private static void AppendDirectionMatrix(int firstIndex, int secondIndex, int cost)
        {
            adjacencyMatrix[firstIndex][secondIndex] = cost;
            adjacencyMatrix[secondIndex][firstIndex] = 0;
        }

        private void AddLoggerContentToCanvas()
        {
            textBlock.Inlines.Clear();
            foreach (var log in logger)
            {
                textBlock.Inlines.Add($"{log}");
                textBlock.Inlines.Add(new LineBreak());
            }
        }

        #region FordFulkerson
        private static int V;
        private static int[,] GetArrayMatrix()
        {
            int c = adjacencyMatrix[0].Count();

            int[,] graph = new int[c, c];

            for (int i = 0; i < c; i++)
                for (int j = 0; j < c; j++)
                    graph[i, j] = adjacencyMatrix[i][j];
            return graph;
        }

        public async void GetFordFulkerson(int s, int t)
        {
            int[,] graph = GetArrayMatrix();
            V = graph.GetLength(0);
            int u, v;
            // Create a residual graph and fill
            // the residual graph with given
            // capacities in the original graph as
            // residual capacities in residual graph
            // Создайте остаточный график и заполните
            // остаточный график с заданным
            // мощности в исходном графике как
            // остаточные мощности в остаточном графике

            // Residual graph where rGraph[i,j]
            // indicates residual capacity of
            // edge from i to j (if there is an
            // edge. If rGraph[i,j] is 0, then
            // there is not)
            // // Остаточный граф, где граф[i,j]
            // указывает остаточную емкость
            // ребра от i до j (если есть
            // ребро. Если график[i,j] равен 0, то
            // его нет)

            int[,] rGraph = new int[V, V];

            for (u = 0; u < V; u++)
                for (v = 0; v < V; v++)
                    rGraph[u, v] = graph[u, v];

            // This array is filled by BFS and to store path
            // Этот массив заполняется BFS и для хранения пути
            int[] parent = new int[V];

            int max_flow = 0; // Объявляем максимальный поток, по умолчанию ноль

            // Augment the flow while there is path from source
            // to sink
            // Увеличьте поток, пока есть путь от источника
            // утонуть
            while (bfs(rGraph, s, t, parent))
            {
                // Find minimum residual capacity of the edhes
                // along the path filled by BFS. Or we can say
                // find the maximum flow through the path found.
                // // Найти минимальную остаточную емкость edhes
                // по пути, заполненному BFS. Или мы можем сказать
                // найдите максимальный поток по найденному пути.
                int path_flow = int.MaxValue;
                for (v = t; v != s; v = parent[v])
                {
                    u = parent[v];
                    path_flow = Math.Min(path_flow, rGraph[u, v]);
                }

                // update residual capacities of the edges and
                // reverse edges along the path
                // обновите остаточные емкости ребер и
                // переверните ребра вдоль пути
                List<string> tmp = new();
                List<int> tmpInt = new();
                List<int> tmpIntHighlight = new();
                for (v = t; v != s; v = parent[v])
                {
                    u = parent[v];
                    //tmp.Add($"{u}");
                    //tmp.Add($"Путь {u + 1}->{v + 1} равен {path_flow} Свободного потока: {rGraph[u, v]}");
                    rGraph[u, v] -= path_flow;
                    rGraph[v, u] += path_flow;
                    tmpInt.Add(v + 1);
                    tmpIntHighlight.Add(v);
                    if (u + 1 == 1)
                    {
                        tmpInt.Add(u + 1);
                        tmpIntHighlight.Add(v);
                    }
                    tmp.Add($"Путь {u + 1}->{v + 1} ({path_flow + rGraph[u, v]}) Поток равен {path_flow} Свободного потока: {rGraph[u, v]}");

                    List<int> hgltE = new List<int>() {u, v};               
                }
                //GetBackAllElement();
                

                // Add path flow to overall flow
                // Добавить поток пути к общему потоку
                tmp.Reverse();
                tmpInt.Reverse();
                tmpIntHighlight.Reverse();
                HighlightElements(tmpInt);

                StringBuilder stringBuilder1 = new StringBuilder();
                foreach (int n in tmpInt)
                    stringBuilder1.Append($"{n} ");
                logger.Add($"Дебаг пути: {stringBuilder1}");

                logger.AddRange(tmp);
                StringBuilder stringBuilder= new StringBuilder();
                foreach(int n in tmpInt) 
                    stringBuilder.Append($"{n}->");
                stringBuilder.Remove(stringBuilder.Length - 2,2);
                logger.Add($"Путь {stringBuilder} равен {path_flow}\n");
                AddLoggerContentToCanvas();
                await Task.Delay(4000);
                GetBackAllElement();

                max_flow += path_flow;
            }


            Console.WriteLine(max_flow);
            // Return the overall flow
            // Вернуть общий поток
            logger.Add($"Максимальный поток в пункт {t + 1} равен {max_flow}");
            AddLoggerContentToCanvas();
            logger.Clear();
        }

        private static bool bfs(int[,] rGraph, int s, int t, int[] parent)
        {
            // Create a visited array and mark
            // all vertices as not visited
            // Создайте посещенный массив и отметьте
            // все вершины как не посещенные
            bool[] visited = new bool[V];
            for (int i = 0; i < V; ++i)
                visited[i] = false;

            // Create a queue, enqueue source vertex and mark
            // source vertex as visited
            // Создайте очередь, поставьте исходную вершину в очередь и отметьте
            // исходную вершину как посещенную
            List<int> queue = new List<int>();
            queue.Add(s);
            visited[s] = true;
            parent[s] = -1;

            // Standard BFS Loop
            // Стандартный цикл BFS
            while (queue.Count != 0)
            {
                int u = queue[0];
                queue.RemoveAt(0);

                for (int v = 0; v < V; v++)
                {
                    if (visited[v] == false
                        && rGraph[u, v] > 0)
                    {
                        // If we find a connection to the sink
                        // node, then there is no point in BFS
                        // anymore We just have to set it's parent
                        // and can return true
                        // Если мы найдем соединение с приемником
                        // узла, то в BFS нет смысла
                        // больше нам просто нужно установить его родителя
                        // и может возвращать значение true
                        if (v == t)
                        {
                            parent[v] = u;
                            return true;
                        }
                        queue.Add(v);
                        parent[v] = u;
                        visited[v] = true;
                    }
                }
            }

            // We didn't reach sink in BFS starting from source,
            // so return false
            // Мы не достигли sink в BFS, начиная с исходного кода,
            // поэтому возвращаем false
            return false;
        }
        #endregion
    }
}
