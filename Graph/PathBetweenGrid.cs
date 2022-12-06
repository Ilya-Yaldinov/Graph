using System.Windows.Controls;
using Point = System.Windows.Point;

namespace Graph
{
    public class PathBetweenGrid
    {
        public Grid? gridFirst;
        public Grid? gridLast;
        public Point start;
        public Point end;

        private static PathBetweenGrid? instance;

        private PathBetweenGrid() { }

        public static PathBetweenGrid GetInstance()
        {
            if (instance == null) instance = new PathBetweenGrid();
            return instance;
        }

        public void Clear()
        {
            instance = null;
            gridFirst = null;
            gridLast = null;
        }
    }
}
