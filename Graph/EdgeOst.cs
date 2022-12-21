using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Graph
{
    public class EdgeOst
    {
        public NodeOst v1, v2;

        public int weight;
        public Grid grid;
        public EdgeOst(NodeOst v1, NodeOst v2, int weight)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.weight = weight;
            CreateGrid();
        }
        public EdgeOst()
        {
        }
        public Line GetLine()
        {
            foreach (var child in grid.Children) if (child.GetType() == typeof(Line)) return (Line)child;
            return new Line();
        }
        public TextBlock GetTextBlock()
        {
            foreach (var child in grid.Children) if (child.GetType() == typeof(TextBlock)) return (TextBlock)child;
            return new TextBlock();
        }
        public void UpdateTextBlock()
        {
            TextBlock textBlock = GetTextBlock();
            textBlock.Margin = new Thickness
            {
                Left = (Canvas.GetLeft(v1.grid) + Canvas.GetLeft(v2.grid)) / 2,
                Top = (Canvas.GetTop(v1.grid) + Canvas.GetTop(v2.grid)) / 2
            };
        }
        private void CreateLine()
        {
            Line line = new Line();
            line.Stroke = (Brush)(new BrushConverter().ConvertFrom("#9ED5C5"));
            line.StrokeThickness = 5;
            line.StrokeStartLineCap = PenLineCap.Round;
            line.StrokeEndLineCap = PenLineCap.Round;
            line.X1 = Canvas.GetLeft(v1.grid) + 25;
            line.Y1 = Canvas.GetTop(v1.grid) + 25;
            line.X2 = Canvas.GetLeft(v2.grid) + 25;
            line.Y2 = Canvas.GetTop(v2.grid) + 25;
            grid.Children.Add(line);
        }
        private void CreateGrid()
        {
            grid = new Grid();
            CreateLine();
            CreateTextBlock();


        }
        private void CreateTextBlock()
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = weight.ToString();
            textBlock.FontSize = 16;
            textBlock.FontFamily = new FontFamily("Yu Gothic UI Semibold");
            //textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            //textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Foreground = Brushes.Red;
            textBlock.Margin = new Thickness
            {
                Left = (Canvas.GetLeft(v1.grid) + Canvas.GetLeft(v2.grid)) / 2,
                Top = (Canvas.GetTop(v1.grid) + Canvas.GetTop(v2.grid)) / 2
            };
            grid.Children.Add(textBlock);
        }
    }
}
