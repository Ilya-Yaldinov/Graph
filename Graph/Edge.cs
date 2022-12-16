namespace Graph
{
    struct Edge
    {
        public int begin;
        public int end;

        public override string ToString()
        {
            return $"{begin + 1};{end + 1}";
        }
    }
}
