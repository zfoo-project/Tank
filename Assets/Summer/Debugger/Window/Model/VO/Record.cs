namespace Summer.Debugger.Window.Model.VO
{
    public sealed class Record
    {
        private readonly string name;
        private int count;
        private long size;

        public Record(string name)
        {
            this.name = name;
            count = 0;
            size = 0L;
        }

        public string Name
        {
            get { return name; }
        }

        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        public long Size
        {
            get { return size; }
            set { size = value; }
        }
    }
}