using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Graph
{
    public class ArrowPolyline : ArrowLineBase
    {
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points",
                typeof(PointCollection), typeof(ArrowPolyline),
                new FrameworkPropertyMetadata(null,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        public PointCollection Points
        {
            set { SetValue(PointsProperty, value); }
            get { return (PointCollection)GetValue(PointsProperty); }
        }

        public ArrowPolyline()
        {
            Points = new PointCollection();
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                pathgeo.Figures.Clear();
                if (Points.Count > 0)
                {
                    pathfigLine.StartPoint = Points[0];
                    polysegLine.Points.Clear();
                    for (int i = 1; i < Points.Count; i++)
                        polysegLine.Points.Add(Points[i]);

                    pathgeo.Figures.Add(pathfigLine);
                }
                return base.DefiningGeometry;
            }
        }
    }
}
