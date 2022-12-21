using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace Graph
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Propertys
        private CreateFigure createFigure = new CreateFigure();
        private Dictionary<Grid, List<ArrowLine>> connections = new Dictionary<Grid, List<ArrowLine>>();
        private Dictionary<ArrowLine, Label> pathCosts = new Dictionary<ArrowLine, Label>();
        private static List<List<int>> adjacencyMatrix = new List<List<int>>();
        private Point? movePoint;
        private List<string> logger = new List<string>();

        private bool isCreateBtnOn = false;
        private bool isDeleteBtnOn = false;
        private bool isWidthBtnOn = false;
        private bool isHeightBtnOn = false;
        private bool isShortestPathBtnOn = false;
        private bool isDirectionConnection = false;
        private bool isFFBtnOn = false;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        #region All Buttons Click
        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            isCreateBtnOn = !isCreateBtnOn;
            button.Background = isCreateBtnOn == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")):
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
            else
            {
                GetBackAllElement(); 
                ClearTextBox();
            }
        }

        private void heightBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            isHeightBtnOn = !isHeightBtnOn;
            button.Background = isHeightBtnOn == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")) :
                                                        (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
            if (isHeightBtnOn) HeightTraversal();
            else
            {
                GetBackAllElement(); 
                ClearTextBox();
            }
        }

        private void shortestPathBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            isShortestPathBtnOn = !isShortestPathBtnOn;
            button.Background = isShortestPathBtnOn == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")) :
                                                              (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
            if (!isShortestPathBtnOn)
            {
                GetBackAllElement();
                ClearTextBox();
            }
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
            textBlock.Text = string.Empty;
        }

        private void dirConnectBtn_Click(object sender, RoutedEventArgs args)
        {
            Button button = sender as Button;
            isDirectionConnection = !isDirectionConnection;
            button.Background = isDirectionConnection == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")) :
                                                                (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
        }

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
        private void treeBtn_Click(object sender, RoutedEventArgs e)
        {
            TreeWindow treeWindow = new TreeWindow();
            treeWindow.Show();
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

            foreach (ArrowLine line in connections[grid])
            {
                double line1 = Math.Sqrt(Math.Pow(point.X + grid.ActualWidth / 2 - line.X1, 2) + Math.Pow(point.Y + grid.ActualHeight / 2 - line.Y1, 2));
                double line2 = Math.Sqrt(Math.Pow(point.X + grid.ActualWidth / 2 - line.X2, 2) + Math.Pow(point.Y + grid.ActualHeight / 2 - line.Y2, 2));

                Label tmp = pathCosts[line];

                Point mdl = new Point();
                mdl.X = (line.X1 + line.X2) / 2;
                mdl.Y = (line.Y1 + line.Y2) / 2;

                if (line1 < line2)
                {
                    line.X1 = point.X + grid.ActualHeight / 2 - 10;
                    line.Y1 = point.Y + grid.ActualHeight / 2;
                }
                else
                {
                    line.X2 = point.X + grid.ActualHeight / 2 - 30;
                    line.Y2 = point.Y + grid.ActualHeight / 2;           
                }

                pathCosts[line].Margin = new System.Windows.Thickness(mdl.X - 20, mdl.Y - 30, 0, 0);
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
            connections.Add(grid, new List<ArrowLine>());
            MainRoot.Children.Add(grid);
            Canvas.SetLeft(grid, point.X - 25);
            Canvas.SetTop(grid, point.Y - 25);

            GetAdjacenciesMatrix();

            grid.MouseLeftButtonDown += FigureMouseDown;
            grid.MouseRightButtonDown += Delete;
            grid.MouseMove += FigureMouseMove;
            grid.MouseLeftButtonUp += FigureMouseUp;
            grid.MouseRightButtonDown += FindShortestPath;
            grid.MouseRightButtonDown += DirectionConnection;
        }

        private void DirectionConnection(object sender, MouseEventArgs args)
        {
            ConnectionFigures connectionFigures = ConnectionFigures.GetInstance();
            if (isDeleteBtnOn == true || isShortestPathBtnOn == true || isDirectionConnection == false) return;

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

                ArrowLine line = createFigure.CreateLine();

                line.X1 = connectionFigures.start.X - 10;
                line.Y1 = connectionFigures.start.Y - 10;
                line.X2 = connectionFigures.end.X - 20;
                line.Y2 = connectionFigures.end.Y;

                Point mArrow = new Point();

                mArrow.X = (line.X1 + line.X2) / 2;
                mArrow.Y = (line.Y1 + line.Y2) / 2;

                if (connectionFigures.gridFirst == connectionFigures.gridLast)
                {
                    connectionFigures.Clear();
                    return;
                }

                foreach (ArrowLine lineStart in connections[connectionFigures.gridFirst])
                    foreach (ArrowLine lineEnd in connections[connectionFigures.gridLast])
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
                RedrawCanvas();

                SetPathCostWindow setPathCostWindow = new();
                setPathCostWindow.ShowDialog();
                connectionFigures.cost = setPathCostWindow.pathCost;

                if (setPathCostWindow.pathCost != 0)
                    AppendDirectionMatrix(firstIndex, secondIndex, setPathCostWindow.pathCost);

                Label pCost = new Label { Margin = new Thickness(mArrow.X - 20, mArrow.Y - 30, 0, 0), Content = connectionFigures.cost, FontSize = 15, Background = Brushes.White };
                pathCosts.Add(line, pCost);

                line.MouseRightButtonDown += Delete;
                RedrawCanvas();
                connectionFigures.Clear();
            }
        }
        #endregion

        #region Delete
        private void Delete(object sender, MouseButtonEventArgs e)
        {
            if (isDeleteBtnOn == false || isShortestPathBtnOn == true) return;
            if (sender.GetType() == typeof(Grid)) DeleteGrid((Grid)sender);
            else if (sender.GetType() == typeof(ArrowLine)) DeleteLine((ArrowLine)sender);
            RedrawCanvas();
        }

        private void DeleteGrid(Grid curGrid)
        {
            List<ArrowLine> lines = new List<ArrowLine>();
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

        private void DeleteLine(ArrowLine curLine)
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
            connections.Add(grid, new List<ArrowLine>());
            MainRoot.Children.Add(grid);
            Canvas.SetLeft(grid, positions[0]);
            Canvas.SetTop(grid, positions[1]);

            grid.MouseLeftButtonDown += FigureMouseDown;
            grid.MouseRightButtonDown += Delete;
            grid.MouseMove += FigureMouseMove;
            grid.MouseLeftButtonUp += FigureMouseUp;
            grid.MouseRightButtonDown += FindShortestPath;
            grid.MouseRightButtonDown += DirectionConnection;
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

                        ArrowLine line = createFigure.CreateLine();

                        line.X1 = Canvas.GetLeft(grid1) + 15;
                        line.Y1 = Canvas.GetTop(grid1) + 25;

                        line.X2 = Canvas.GetLeft(grid2) - 5;
                        line.Y2 = Canvas.GetTop(grid2) + 25;

                        Point mArrow = new();
                        mArrow.X = (line.X1 + line.X2) / 2;
                        mArrow.Y = (line.Y1 + line.Y2) / 2;

                        foreach (ArrowLine lineStart in connections[grid1])
                            foreach (ArrowLine lineEnd in connections[grid2])
                                if (lineStart == lineEnd) continue;

                        connections[grid1].Add(line);
                        connections[grid2].Add(line);

                        Label pCost = new Label { Margin = new Thickness(mArrow.X - 20, mArrow.Y - 30, 0, 0), Content = adjacencyMatrix[i][j], FontSize = 15, Background = Brushes.White };
                        pathCosts.Add(line, pCost);

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
            logger.Add("Добавили элемент \"1\".");
            while (queue.Count != 0)
            {
                int node = queue.Dequeue();
                logger.Add($"Взяли элемент \"{node + 1}\".");
                logger.Add($"Перешли в элемент \"{node + 1}\".");
                nodes[node] = 2;
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i] == 0 && adjacencyMatrix[node][i] != 0)
                    {
                        queue.Enqueue(i);
                        logger.Add($"Обнаружили элемент \"{i + 1}\".");
                        logger.Add($"Добавили элемент \"{i + 1}\".");
                        nodes[i] = 1;
                    }
                }
                logger.Add($"{GetAllElementOfCollection(queue)}");
                await Task.Delay(1000);
                AddLoggerContentToCanvas();
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
            logger.Add("Добавили элемент \"1\".");
            while (stack.Count != 0)
            {
                int node = stack.Pop();
                logger.Add($"Взяли элемент \"{node + 1}\".");
                if (nodes[node] == 2)
                {
                    logger.Add($"Элемент \"{node + 1}\" ранее был посещён.");
                    logger.Add($"{GetAllElementOfCollection(stack)}");
                    AddLoggerContentToCanvas(); 
                    continue;
                }
                logger.Add($"Перешли в элемент \"{node + 1}\".");
                nodes[node] = 2;
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    if (adjacencyMatrix[node][i] != 0 && nodes[i] != 2)
                    {
                        stack.Push(i);
                        logger.Add($"Обнаружили элемент \"{i + 1}\".");
                        logger.Add($"Добавили элемент \"{i + 1}\".");
                        nodes[i] = 1;
                    }
                }
                logger.Add($"{GetAllElementOfCollection(stack)}");
                await Task.Delay(1000);
                AddLoggerContentToCanvas();
                HighlightElements(nodes);
            }
        }

        private async void FindShortestPath(object sender, MouseEventArgs args)
        {
            PathBetweenGrid pathBetweenGrid = PathBetweenGrid.GetInstance();
            if (isShortestPathBtnOn == false || isDeleteBtnOn == true) return;
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
                logger.Add($"Элемент, откуда идем: {req + 1}");
                int lastIndex = GetIndexOfGrid(pathBetweenGrid.gridFirst);
                queue.Enqueue(lastIndex);
                logger.Add($"Элемент, куда надо прийти: {lastIndex + 1}.");
                logger.Add($"Добавили элемент \"{lastIndex + 1}\".");
                while (queue.Count != 0)
                {
                    int node = queue.Dequeue();
                    logger.Add($"Взяли элемент \"{node + 1}\".");
                    nodes[node] = 2;
                    int count = 0;
                    for (int i = 0; i < nodes.Count(); i++)
                    {
                        if (adjacencyMatrix[node][i] != 0 && nodes[i] == 0)
                        {
                            count++;
                            logger.Add($"Перешли в элемент \"{node + 1}\".");
                            queue.Enqueue(i);
                            logger.Add($"Обнаружили элемент \"{i + 1}\".");
                            nodes[i] = 1;
                            edge.begin = node;
                            logger.Add($"Установили элемент \"{node + 1}\" концом Node.");
                            edge.end = i;
                            logger.Add($"Установили элемент \"{i + 1}\" началом Node.");
                            edges.Push(edge);
                            logger.Add($"Добавили Node({edge.ToString()}) в очередь.");
                            if (node == req) break;
                        }
                    }
                    if(count != 0) logger.Add($"Элемент \"{node + 1}\" нам не подхдит.");
                    await Task.Delay(1000);
                    logger.Add($"{GetAllElementOfCollection(queue)}");
                    AddLoggerContentToCanvas();
                }
                logger.Add($"");
                logger.Add($"Путь построен.");
                logger.Add("Переходим к отрисовке.");
                logger.Add($"");
                while (edges.Count != 0)
                {
                    edge = edges.Pop();
                    logger.Add($"Взяли Node({edge.ToString()}).");
                    if (edge.end == req)
                    {
                        logger.Add($"Элемент \"{edge.end + 1}\" равен конечному.");
                        req = edge.begin;
                        logger.Add($"Устанавливаем элемент \"{edge.begin + 1}\" конечным.");
                        await Task.Delay(1000);
                        HighlightPath(GetEllipseFromIndex(edge.end));
                        logger.Add($"Отрисовываем элемент \"{edge.end + 1}\"");
                    }
                    else
                    {
                        logger.Add($"Node({edge.ToString()}) нам не подходит.");
                    }
                    AddLoggerContentToCanvas();
                }
                await Task.Delay(1000);
                HighlightPath(GetEllipseFromIndex(req));
                logger.Add($"Отрисовываем элемент \"{req + 1}\"");
                AddLoggerContentToCanvas();
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

        private static void AppendDirectionMatrix(int firstIndex, int secondIndex, int cost)
        {
            adjacencyMatrix[firstIndex][secondIndex] = cost;
            adjacencyMatrix[secondIndex][firstIndex] = 0;
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
                foreach (ArrowLine line in keyValuePair.Value)
                {
                    if (!MainRoot.Children.Contains(line))
                    {
                        MainRoot.Children.Add(line);
                        if (pathCosts.ContainsKey(line))
                        {
                            MainRoot.Children.Add(pathCosts[line]);
                        }
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

        private void HighlightElements(List<int> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Ellipse ellipse = GetEllipseFromIndex(i);
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

        private async void HighlightElementsBFS(List<int> nodes)
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
                    }
                }
            }
        }
        #endregion

        #region Action With TextBox
        private void AddLoggerContentToCanvas()
        {
            textBlock.Inlines.Clear();
            foreach (var log in logger)
            {
                textBlock.Inlines.Add($"{log}");
                textBlock.Inlines.Add(new LineBreak());
            }
        }

        private void ClearTextBox()
        {
            logger.Clear();
            textBlock.Inlines.Clear();
        }

        private string GetAllElementOfCollection(IEnumerable<int> collection)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var elem in collection)
            {
                sb.Append($"\"{(int)elem + 1}\";");
            }
            return sb.Length > 0 ?
                $"Состояние очереди: {sb.ToString().Substring(0, sb.Length - 1)}." :
                "Коллекция пуста.";
        }

        #endregion

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

            int[,] rGraph = new int[V, V];

            for (u = 0; u < V; u++)
                for (v = 0; v < V; v++)
                    rGraph[u, v] = graph[u, v];

            int[] parent = new int[V];

            int max_flow = 0; 

            while (bfs(rGraph, s, t, parent))
            {
                int path_flow = int.MaxValue;

                for (v = t; v != s; v = parent[v])
                {
                    u = parent[v];
                    path_flow = Math.Min(path_flow, rGraph[u, v]);
                }

                List<string> tmp = new();
                List<int> tmpInt = new();
                List<int> tmpIntHighlight = new();

                for (v = t; v != s; v = parent[v])
                {
                    u = parent[v];

                    rGraph[u, v] -= path_flow;
                    rGraph[v, u] += path_flow;
                    tmpInt.Add(v + 1);
                    tmpIntHighlight.Add(v);
                    if (u + 1 == 1)
                    {
                        tmpInt.Add(u + 1);
                        tmpIntHighlight.Add(v);
                    }
                    tmp.Add($"Путь {u + 1}->{v + 1} Поток равен {path_flow + rGraph[u, v]} Свободного потока: {path_flow + rGraph[u, v]} - {path_flow} = {rGraph[u, v]}");
                }

                tmp.Reverse();
                tmpInt.Reverse();
                tmpIntHighlight.Reverse();
                HighlightElementsBFS(tmpInt);

                StringBuilder stringBuilder1 = new StringBuilder();
                foreach (int n in tmpInt)
                    stringBuilder1.Append($"{n} ");

                logger.AddRange(tmp);
                StringBuilder stringBuilder = new StringBuilder();
                foreach (int n in tmpInt)
                    stringBuilder.Append($"{n}->");
                stringBuilder.Remove(stringBuilder.Length - 2, 2);
                logger.Add($"Путь {stringBuilder} Макс поток данного пути равен {path_flow}\n");
                AddLoggerContentToCanvas();
                await Task.Delay(1000);
                GetBackAllElement();

                max_flow += path_flow;
            }


            Console.WriteLine(max_flow);
            logger.Add($"Максимальный поток в пункт {t + 1} равен {max_flow}");
            AddLoggerContentToCanvas();
            logger.Clear();
        }

        private static bool bfs(int[,] rGraph, int s, int t, int[] parent)
        {
            bool[] visited = new bool[V];
            for (int i = 0; i < V; ++i)
                visited[i] = false;
            List<int> queue = new List<int>();
            queue.Add(s);
            visited[s] = true;
            parent[s] = -1;

            while (queue.Count != 0)
            {
                int u = queue[0];
                queue.RemoveAt(0);

                for (int v = 0; v < V; v++)
                {
                    if (visited[v] == false
                        && rGraph[u, v] > 0)
                    {
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
            return false;
        }
        #endregion
    }
}
