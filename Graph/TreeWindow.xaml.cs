using Graph;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Graph
{
    /// <summary>
    /// Логика взаимодействия для TreeWindow.xaml
    /// </summary>
    public partial class TreeWindow : Window
    {
        #region Properties
        private Dictionary<NodeOst, List<EdgeOst>> connections = new Dictionary<NodeOst, List<EdgeOst>>();
        private List<NodeOst> nodesOst = new List<NodeOst>();
        private List<EdgeOst> edgesOst = new List<EdgeOst>();
        private List<List<int>> adjacencyMatrix = new List<List<int>>();
        private List<EdgeOst> ostTree = null;
        private Point? movePoint;

        private bool isCreateBtnOn = false;
        private bool isConnectBtnOn = false;
        private bool isDeleteBtnOn = false;
        #endregion
        public TreeWindow()
        {
            InitializeComponent();
            AddLoggerContentToCanvas("Открыли дополнительное окно с построением остовного дерева");
        }
        #region All Buttons Click
        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            ReadFromFile();
        }
        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }
        private void clearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearTextBox();
        }
        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            isConnectBtnOn = !isConnectBtnOn;
            button.Background = isConnectBtnOn == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")) :
                                                         (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
            AddLoggerContentToCanvas("Cостояние соединения узлов " + (isConnectBtnOn ?"активировано":"деактивировано"));
        }
        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            isCreateBtnOn = !isCreateBtnOn;
            button.Background = isCreateBtnOn == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")) :
                                                        (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
            AddLoggerContentToCanvas("Cостояние создания узлов " + (isCreateBtnOn ? "активировано" : "деактивировано"));
        }
        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            isDeleteBtnOn = !isDeleteBtnOn;
            button.Background = isDeleteBtnOn == true ? (Brush)(new BrushConverter().ConvertFrom("#FF7373")) :
                                                        (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
            AddLoggerContentToCanvas("Cостояние удаления обьектов " + (isDeleteBtnOn ? "активировано" : "деактивировано"));
        }
        private void saveToFileBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveToNewFile(false);
        }
        private void starterBtn_Click(object sender, RoutedEventArgs e)
        {
            algorithmByPrim(nodesOst,edgesOst, connections);
            AddLoggerContentToCanvas("Остовное дерево построено");
        }
        private void undoneBtn_Click(object sender, RoutedEventArgs e)
        {
            ostTree = null;
            foreach (EdgeOst edge in edgesOst) edge.GetLine().Stroke = (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
            AddLoggerContentToCanvas("Остовное дерево удалено");
            RedrawCanvas();
        }
        private void saveToFileOstBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveToNewFile(true);
        }
        #endregion
        #region ReadFromFile
        private void ReadFromFile()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Text documents (.csv)|*.csv";
            Nullable<bool> result = openFile.ShowDialog();

            if (result == true)
            {
                Clear();
                string fileName = openFile.FileName;
                AddLoggerContentToCanvas("Открыли файл " + fileName);
                InfoFromFileToCanvas(fileName);
                RedrawCanvas();
            }
            else AddLoggerContentToCanvas("Попытка открыть файл оказалась неудачной");
        }
        private void InfoFromFileToCanvas(string fileName)
        {
            string[] file = File.ReadAllLines(fileName);
            adjacencyMatrix.Clear();
            for (int i = 0;i<file.Length ;i++)
            {
                List<string> elems = file[i].Split('-')
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
                AddGridToCanvasFromFile(positions,i+1);
            }
            GetConnectionFromFile();
        }

        private void AddGridToCanvasFromFile(List<double> positions, int numOfNode)
        {
            NodeOst node = new NodeOst(numOfNode);
            connections.Add(node, new List<EdgeOst>());
            nodesOst.Add(node);
            TreeRoot.Children.Add(node.grid);
            Canvas.SetLeft(node.grid, positions[0]);
            Canvas.SetTop(node.grid, positions[1]);

            AddLoggerContentToCanvas($"Добавили узел {node.numOfNode} на канву");

            node.grid.MouseLeftButtonDown += FigureMouseDown;
            node.grid.MouseRightButtonDown += Delete;
            node.grid.MouseMove += FigureMouseMove;
            node.grid.MouseLeftButtonUp += FigureMouseUp;
            node.grid.MouseRightButtonDown += Connection;
        }
        private void GetConnectionFromFile()
        {
            for (int i = 0; i < adjacencyMatrix.Count; i++)
            {
                for (int j = 0; j < adjacencyMatrix[i].Count; j++)
                {
                    if (adjacencyMatrix[i][j] != 0)
                    {
                        NodeOst node1 = GetNodeFromIndex(i+1);
                        NodeOst node2 = GetNodeFromIndex(j+1);
                        EdgeOst edge = new EdgeOst(node1,node2,adjacencyMatrix[i][j]);

                        foreach (EdgeOst lineStart in connections[node1])
                            foreach (EdgeOst lineEnd in connections[node2])
                                if (lineStart.grid == lineEnd.grid)
                                {
                                    AddLoggerContentToCanvas($"Cоединение между {lineEnd.v1.numOfNode} и {lineEnd.v2.numOfNode} c весом {edge.weight} уже отрисовано");
                                    continue; 
                                }
                        if (i != j)
                        {
                            connections[node1].Add(edge);
                            connections[node2].Add(edge);
                        }
                        edgesOst.Add(edge);
                        TreeRoot.Children.Add(edge.grid);
                        AddLoggerContentToCanvas($"Добавили соединение между {edge.v1.numOfNode} и {edge.v2.numOfNode} c весом {edge.weight}") ;
                        edge.GetLine().MouseRightButtonDown += Delete;
                    }
                }
            }
            RedrawCanvas();
        }
        #endregion
        #region Clear
        private void Clear()
        {
            TreeRoot.Children.Clear();
            ostTree = null;
            connections.Clear();
            edgesOst.Clear();
            nodesOst.Clear();
            AddLoggerContentToCanvas("Очистили канву и внутренние хранилища элементов");
        }
        #endregion
        #region Action With Grid
        private NodeOst GetNodeFromIndex(int numOfNode)
        {
            NodeOst node = new NodeOst();
            foreach (var grid in connections.Keys)
            {
                if (grid.numOfNode==numOfNode)
                {
                    node = (NodeOst)grid;
                    return node;
                }
            }
            return node;
        }
        private void AddGridToCanvas(object sender, MouseButtonEventArgs e)
        {
            if (isCreateBtnOn != true) return;

            Point point = e.GetPosition(TreeRoot);
            NodeOst node = new NodeOst(nodesOst.Count+1);
            connections.Add(node, new List<EdgeOst>());
            nodesOst.Add(node);
            TreeRoot.Children.Add(node.grid);
            Canvas.SetLeft(node.grid, point.X - 25);
            Canvas.SetTop(node.grid, point.Y - 25);

            AddLoggerContentToCanvas($"Создание узла №{node.numOfNode}") ;
            GetAdjacenciesMatrix();

            node.grid.MouseLeftButtonDown += FigureMouseDown;
            node.grid.MouseRightButtonDown += Delete;
            node.grid.MouseMove += FigureMouseMove;
            node.grid.MouseLeftButtonUp += FigureMouseUp;
            node.grid.MouseRightButtonDown += Connection;
        }
        private string GetPositionOfGridToString(int count)
        {
            Grid grid = GetNodeFromIndex(count).grid;
            double posX = Canvas.GetLeft(grid);
            double posY = Canvas.GetTop(grid);
            return $"{Math.Round(posX, 2)};{Math.Round(posY, 2)}";
        }
        #endregion
        #region Mouse Movement
        private void FigureMouseDown(object sender, MouseButtonEventArgs args)//UNDONE
        {
            Grid grid = (Grid)sender;
            movePoint = args.GetPosition(grid);
            grid.CaptureMouse();
        }
        private void FigureMouseUp(object sender, MouseButtonEventArgs args)//UNDONE
        {
            Grid grid = (Grid)sender;
            movePoint = null;
            grid.ReleaseMouseCapture();
        }
        private void FigureMouseMove(object sender, MouseEventArgs args)//DONE
        {
            NodeOst node = new NodeOst();
            foreach (NodeOst value in nodesOst) if (value.grid == (Grid)sender) node = value;

            if (movePoint == null) return;

            Point point = args.GetPosition(TreeRoot) - (Vector)movePoint.Value;

            Canvas.SetLeft(node.grid, point.X);
            Canvas.SetTop(node.grid, point.Y);

            foreach (EdgeOst edge in connections[node])
            {
                Line line = edge.GetLine();

                double line1 = Math.Sqrt(Math.Pow(point.X + node.grid.ActualWidth / 2 - line.X1, 2) + Math.Pow(point.Y + node.grid.ActualHeight / 2 - line.Y1, 2));
                double line2 = Math.Sqrt(Math.Pow(point.X + node.grid.ActualWidth / 2 - line.X2, 2) + Math.Pow(point.Y + node.grid.ActualHeight / 2 - line.Y2, 2));

                if (line1 < line2)
                {
                    line.X1 = point.X + node.grid.ActualHeight / 2;
                    line.Y1 = point.Y + node.grid.ActualHeight / 2;
                }
                else
                {
                    line.X2 = point.X + node.grid.ActualHeight / 2;
                    line.Y2 = point.Y + node.grid.ActualHeight / 2;
                }
                edge.UpdateTextBlock();
            }
        }
        #endregion
        #region Matrix Action
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
            AddLoggerContentToCanvas("Добавление в матрицу смежности полей для нового узла");
        }
        private void AppendAdjacenciesMatrix(EdgeOst edge)
        {
            adjacencyMatrix[edge.v1.numOfNode-1][edge.v2.numOfNode-1] = edge.weight;
            adjacencyMatrix[edge.v2.numOfNode-1][edge.v1.numOfNode-1] = edge.weight;
            AddLoggerContentToCanvas($"Добавление в таблицу смежности соединения");
        }
        #endregion
        #region Delete
        private void Delete(object sender, MouseButtonEventArgs e)
        {
            if (isDeleteBtnOn == false || isConnectBtnOn == true) return;
            if (sender.GetType() == typeof(Grid))
            {
                NodeOst node = new NodeOst();
                foreach (var value in nodesOst) if (value.grid == (Grid)sender) node = value;
                DeleteNode(node);
            }
            else if (sender.GetType() == typeof(Line))
            {
                EdgeOst edge = new EdgeOst();
                foreach (var value in edgesOst) if (value.GetLine() == (Line)sender) edge = value;
                DeleteEdge(edge);
            }
            RedrawCanvas();
        }

        private void DeleteNode(NodeOst node)
        {
            nodesOst.Remove(node);
            AddLoggerContentToCanvas($"Удаление узла №{node.numOfNode}");
            List<EdgeOst> edges = new List<EdgeOst>(connections[node]);
            while (edges.Count != 0)
            {
                DeleteEdge(edges[0]);
                edges.RemoveAt(0);
            }
            connections.Remove(node);
        }

        private void DeleteEdge(EdgeOst curLine)
        {
            foreach (var lines in connections.Values)
            {
                foreach (var line in lines)
                {
                    if (line == curLine)
                    {
                        lines.Remove(line);
                        AddLoggerContentToCanvas($"Удаление соединения между {curLine.v1.numOfNode} и {curLine.v2.numOfNode} весом {curLine.weight}");
                        break;
                    }
                }
            }
            edgesOst.Remove(curLine);
        }
        #endregion
        #region Connection
        private void Connection(object sender, MouseEventArgs args)//UNDONE
        {
            ConnectionFigures connectionFigures = ConnectionFigures.GetInstance();
            if (isConnectBtnOn == false || isDeleteBtnOn == true) return;

            Point point = args.GetPosition(TreeRoot);

            if (connectionFigures.start.X == 0 && connectionFigures.start.Y == 0)
            {
                connectionFigures.start = point;
                connectionFigures.gridFirst = (Grid)sender;
                AddLoggerContentToCanvas($"Установка первого узла для соединения");
            }

            else if (connectionFigures.end.X == 0 && connectionFigures.end.Y == 0)
            {
                connectionFigures.gridLast = (Grid)sender;
                connectionFigures.end = point;
                AddLoggerContentToCanvas($"Установка второго узла для соединения");

                NodeOst nodeFirst = new NodeOst();
                NodeOst nodeLast = new NodeOst();
                SetPathCostWindow setPathCostWindow = new();
                setPathCostWindow.ShowDialog();
                int cost = setPathCostWindow.pathCost;
                foreach (var value in nodesOst) if (value.grid == connectionFigures.gridFirst) nodeFirst = value;
                foreach (var value in nodesOst) if (value.grid == connectionFigures.gridLast) nodeLast = value;
                EdgeOst edge = new EdgeOst(nodeFirst,nodeLast,cost);
                AddLoggerContentToCanvas($"Создание соединения между {edge.v1.numOfNode} и {edge.v2.numOfNode} весом {edge.weight}");

                Line line = edge.GetLine();

                line.X1 = connectionFigures.start.X;
                line.Y1 = connectionFigures.start.Y;
                line.X2 = connectionFigures.end.X;
                line.Y2 = connectionFigures.end.Y;

                if (connectionFigures.gridFirst == connectionFigures.gridLast)
                {
                    connectionFigures.Clear();
                    return;
                }

                foreach (EdgeOst edgeStart in connections[nodeFirst])
                    foreach (EdgeOst edgeEnd in connections[nodeLast])
                        if (edgeStart == edgeEnd)
                        {
                            connectionFigures.Clear();
                            return;
                        }

                connections[nodeFirst].Add(edge);
                connections[nodeLast].Add(edge);
                edgesOst.Add(edge);

                int firstIndex = nodeFirst.numOfNode;
                int secondIndex = nodeLast.numOfNode; 
                AppendAdjacenciesMatrix(edge);

                line.MouseRightButtonDown += Delete;
                RedrawCanvas();
                connectionFigures.Clear();
            }
        }
        #endregion 
        #region Save To File

        private void SaveToNewFile(bool isOstTree)
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
                int count = 1;
                if (isOstTree)
                {
                    AddLoggerContentToCanvas($"Попытка записи в файл минимального остовного графа");
                    List<List<int>> maskAdjacencyMatrix = new List<List<int>>();
                    for (int i = 0; i < adjacencyMatrix.Count; i++)
                    {
                        maskAdjacencyMatrix.Add(new List<int>());
                        for (int j = 0; j < adjacencyMatrix.Count; j++)
                        {
                            maskAdjacencyMatrix[i].Add(0);
                        }
                    }
                    AddLoggerContentToCanvas($"Создание маски для таблицы смежности");
                    foreach (EdgeOst edge in ostTree) maskAdjacencyMatrix[edge.v1.numOfNode - 1][edge.v2.numOfNode - 1] = 1;
                    for (int i = 0; i < adjacencyMatrix.Count; i++)
                    {
                        List<int> row = adjacencyMatrix[i];
                        for (int j = 0; j < row.Count; j++)
                        {
                            sw.Write(maskAdjacencyMatrix[i][j] == 1 ? $"{row[j]};" : $"0;");
                        }
                        sw.Write($"---;{GetPositionOfGridToString(count++)}");
                        sw.WriteLine();
                        AddLoggerContentToCanvas($"Запись в файл соединений и координат {count - 1}-го узла");
                    }
                }
                else
                {
                    AddLoggerContentToCanvas($"Попытка записи в файл обычного графа");
                    foreach (List<int> row in adjacencyMatrix)
                    {
                        row.ForEach(x => sw.Write($"{x};"));
                        sw.Write($"---;{GetPositionOfGridToString(count++)}");
                        sw.WriteLine();
                        AddLoggerContentToCanvas($"Запись в файл соединений и координат {count - 1}-го узла");
                    }
                }
                sw.Close();
                AddLoggerContentToCanvas($"Запись данных в файл {filename} успешна");
            }
            else AddLoggerContentToCanvas($"Попытка сохранения в файл {saveFile.FileName} провалилась") ;
        }
        #endregion
        #region Redraw
        private void RedrawCanvas(List<NodeOst>nodes ,List<EdgeOst> edges)
        {
            TreeRoot.Children.Clear();
            
            foreach (var edge in edges) TreeRoot.Children.Add(edge.grid);
            foreach (var node in nodes) TreeRoot.Children.Add(node.grid);
            AddLoggerContentToCanvas($"Перерисовка канвы");
        }
        private void RedrawCanvas()
        {
            TreeRoot.Children.Clear();

            foreach (var keyValuePair in connections)
            {
                foreach (EdgeOst edge in keyValuePair.Value)
                {
                    if (!TreeRoot.Children.Contains(edge.grid))
                    {
                        TreeRoot.Children.Add(edge.grid);
                    }
                }
                TreeRoot.Children.Add(keyValuePair.Key.grid);
            }
            AddLoggerContentToCanvas($"Перерисовка канвы");
        }
        #endregion
        #region Textbox Helper
        private void AddLoggerContentToCanvas(string log)
        {
                textBlock.Inlines.Add($"{log}");
                textBlock.Inlines.Add(new LineBreak());
        }

        private void ClearTextBox()
        {
            textBlock.Inlines.Clear();
            AddLoggerContentToCanvas($"Логи очищены");
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
        #region Prim
        private async void algorithmByPrim(List<NodeOst> N, List<EdgeOst> E, Dictionary<NodeOst, List<EdgeOst>> connections)
        {
            foreach (var value in E) value.GetLine().Stroke = (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
            foreach (var value in N) value.GetEllipse().Fill = Brushes.Orange;
            List<EdgeOst> ostTree = new List<EdgeOst>();
            //неиспользованные ребра
            List<EdgeOst> notUsedE = new List<EdgeOst>(E);
            //использованные вершины
            List<NodeOst> usedN = new List<NodeOst>();
            //неиспользованные вершины
            List<NodeOst> notUsedN = new List<NodeOst>(connections.Where(x => x.Value.Count != 0).Select(x => x.Key).ToList());
            //выбираем случайную начальную вершину
            Random rand = new Random();
            int randNum = rand.Next(0, notUsedN.Count);
            usedN.Add(notUsedN[randNum]);
            AddLoggerContentToCanvas($"Выбрали случайный узел {randNum+1}");
            AddLoggerContentToCanvas($"Добавили узел в список использованных, удалили из списка неиспользованных");
            notUsedN.RemoveAt(randNum);
            while (!(ostTree.Count == connections.Where(x => x.Value.Count != 0).ToList().Count - 1))
            {
                List<EdgeOst> tempEdges = new List<EdgeOst>();
                foreach (var node in usedN)
                {
                    foreach (var keyValuePair in connections)
                    {
                        if (keyValuePair.Key == node)
                        {
                            foreach (var value in keyValuePair.Value)
                            {
                                bool isNormal = true;
                                if (ostTree.Count != 0)
                                {
                                    foreach (var edge in ostTree)
                                        if (!(edge != value && !(usedN.Contains(value.v1) && usedN.Contains(value.v2)) && !(value.v1 == value.v2)))
                                            //if (!(edge != value && !(value.v1 == edge.v2 && value.v2 == edge.v1))) 
                                            isNormal = false;
                                }
                                if (!tempEdges.Contains(value) && isNormal)
                                {
                                    tempEdges.Add(value);
                                    AddLoggerContentToCanvas($"Нашли соединение между {value.v1.numOfNode} и {value.v2.numOfNode} весом {value.weight}, которое не связано одновременно с двумя используемыми вершинами");
                                    value.GetLine().Stroke = Brushes.Red;
                                    await Task.Delay(1000);
                                }
                            }
                        }
                    }//нашли все ребра, которые связаны с вершинами
                }
                EdgeOst minEdge = new EdgeOst { };
                int minWeight = int.MaxValue;
                foreach (var edge in tempEdges) if (edge.weight < minWeight)
                    {
                        minEdge = edge;
                        minWeight = minEdge.weight;
                    }
                AddLoggerContentToCanvas($"Cоединение между {minEdge.v1.numOfNode} и {minEdge.v2.numOfNode} весом {minEdge.weight} минимально по весу");
                //поиск наименьшего ребра
                if (minEdge.weight == minWeight)
                {
                    NodeOst tempNode = usedN.Contains(minEdge.v2) ? minEdge.v1 : minEdge.v2;
                    usedN.Add(tempNode);
                    notUsedN.Remove(tempNode);
                    tempNode.GetEllipse().Fill = Brushes.Maroon;
                    AddLoggerContentToCanvas($"Добавили узел {tempNode.numOfNode} в список использованных, удалили из списка неиспользованных");
                    //foreach (var child in tempNode.grid.Children)
                    //{
                    //    if (child.GetType() == typeof(Ellipse))
                    //    {
                    //        Ellipse ellipse = (Ellipse)child;
                    //        ellipse.Fill = Brushes.;
                    //    }
                    //}
                    //заносим новую вершину в список использованных и удаляем ее из списка неиспользованных
                    ostTree.Add(minEdge);
                    notUsedE.Remove(minEdge);
                    minEdge.GetLine().Stroke = Brushes.Green;
                    //заносим новое ребро в дерево и удаляем его из списка неиспользованных
                    AddLoggerContentToCanvas($"Нашли соединение c минимальным весом {minEdge.weight} между {minEdge.v1.numOfNode} и {minEdge.v2.numOfNode}");
                    AddLoggerContentToCanvas($"Заносим ребро в дерево, удаляем из списка неиспользованных");
                    foreach (var value in notUsedE) value.GetLine().Stroke = (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
                    await Task.Delay(1000);
                    
                }
            }

            foreach (var value in usedN) value.GetEllipse().Fill = Brushes.Orange;

        }
        #endregion
    }
}
