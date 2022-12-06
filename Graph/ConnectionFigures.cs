using System.Windows.Controls;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace Graph
{
    public class ConnectionFigures
    {
        public Point start;
        public Point end;
        public Grid? gridFirst;
        public Grid? gridLast;

        private static ConnectionFigures? instance;

        private ConnectionFigures() { }

        public static ConnectionFigures GetInstance()
        {
            if (instance == null) instance = new ConnectionFigures();
            return instance;
        }

        public void Clear()
        {
            start = new Point();
            end = new Point();
            gridFirst = null;
            gridLast = null;
            instance = null;
        }
    }
}
