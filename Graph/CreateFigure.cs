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

        private Ellipse CreateEllipse()
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = 50;
            ellipse.Height = 50;
            /*ellipse.StrokeThickness = 5;
            ellipse.Stroke = Brushes.Gray;*/
            ellipse.Fill = Brushes.Orange;

            return ellipse;
        }

        public Grid CreateGrid()
        {
            Grid grid = new Grid();
            Ellipse ellipse = CreateEllipse();
            TextBlock textBlock = CreateTextBlock();
            grid.Children.Add(ellipse);
            grid.Children.Add(textBlock);

            return grid;
        }

        public Line CreateLine()
        {
            Line line = new Line();
            line.Stroke = Brushes.Violet;
            line.StrokeThickness = 5;
            line.StrokeStartLineCap = PenLineCap.Round;
            line.StrokeEndLineCap = PenLineCap.Round;

            return line;
        }
    }
}
