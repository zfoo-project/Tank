using Spring.Collection.Reference;

namespace Summer.Download.Model.Vo
{
    public sealed class DownloadCounterNode : IReference
    {
        private long m_DownloadedLength;
        private float m_ElapseSeconds;

        public DownloadCounterNode()
        {
            m_DownloadedLength = 0;
            m_ElapseSeconds = 0f;
        }

        public long DownloadedLength
        {
            get { return m_DownloadedLength; }
        }

        public float ElapseSeconds
        {
            get { return m_ElapseSeconds; }
        }

        public static DownloadCounterNode Create()
        {
            DownloadCounterNode downloadCounterNode = ReferenceCache.Acquire<DownloadCounterNode>();
            return downloadCounterNode;
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_ElapseSeconds += realElapseSeconds;
        }

        public void AddDownloadedLength(int downloadedLength)
        {
            m_DownloadedLength += downloadedLength;
        }

        public void Clear()
        {
            m_DownloadedLength = 0L;
            m_ElapseSeconds = 0f;
        }
    }
}