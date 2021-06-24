namespace Summer.Debugger.Window.Model.VO
{
    public sealed class Sample
    {
        private readonly string name;
        private readonly string type;
        private readonly long size;
        private bool highlight;

        public Sample(string name, string type, long size)
        {
            this.name = name;
            this.type = type;
            this.size = size;
        }

        public string Name
        {
            get { return name; }
        }

        public string Type
        {
            get { return type; }
        }

        public long Size
        {
            get { return size; }
        }

        public bool Highlight
        {
            get { return highlight; }
            set { highlight = value; }
        }
    }
}