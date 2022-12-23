using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Graph
{
    public class CreateFigure
    {
        private int count = 0;

        private string GetCount()
        {
            count++;
            return count.ToString();
        }

        private TextBlock CreateTextBlock()
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = GetCount();
            textBlock.FontSize = 16;
            textBlock.FontFamily = new FontFamily("Yu Gothic UI Semibold");
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Foreground = Brushes.White;

            return textBlock;
        }

        private Label CreateLable()
        {
            Label label = new Label();
            label.FontSize = 10;
            label.FontFamily = new FontFamily("Yu Gothic UI Semibold");
            label.HorizontalAlignment = HorizontalAlignment.Right;
            label.VerticalAlignment = VerticalAlignment.Bottom;

            return label;
        }

        private Ellipse CreateEllipse()
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = 50;
            ellipse.Height = 50;
            ellipse.Fill = Brushes.Orange;

            return ellipse;
        }

        public Grid CreateGrid()
        {
            Grid grid = new Grid();
            Ellipse ellipse = CreateEllipse();
            TextBlock textBlock = CreateTextBlock();
            Label label = CreateLable();
            grid.Children.Add(ellipse);
            grid.Children.Add(textBlock);
            grid.Children.Add(label);

            return grid;
        }

        public ArrowLine CreateLine()
        {
            ArrowLine arrowLine = new();
            arrowLine.Stroke = Brushes.Black;
            arrowLine.StrokeThickness = 2;
            arrowLine.ArrowAngle = 45;
            arrowLine.ArrowLength = 10;
            arrowLine.IsArrowClosed = true;

            return arrowLine;
        }
    }
}
