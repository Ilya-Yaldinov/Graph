using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Graph
{
    public class NodeOst
    {
        public int numOfNode;
        public Grid grid;

        public NodeOst(int numOfNode)
        {
            this.numOfNode = numOfNode;
            CreateGrid();

        }
        public NodeOst()
        {

        }
        public Ellipse GetEllipse()
        {
            foreach (var child in grid.Children) if (child.GetType() == typeof(Ellipse)) return (Ellipse)child;
            return new Ellipse();
        }

        private void CreateGrid()
        {
            grid = new Grid();
            Ellipse ellipse = CreateEllipse();
            TextBlock textBlock = CreateTextBlock();
            grid.Children.Add(ellipse);
            grid.Children.Add(textBlock);
        }
        private TextBlock CreateTextBlock()
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = numOfNode.ToString();
            textBlock.FontSize = 16;
            textBlock.FontFamily = new FontFamily("Yu Gothic UI Semibold");
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Foreground = Brushes.White;

            return textBlock;
        }
        private Ellipse CreateEllipse()
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = 50;
            ellipse.Height = 50;
            ellipse.Fill = Brushes.Orange;

            return ellipse;
        }
    }
}
