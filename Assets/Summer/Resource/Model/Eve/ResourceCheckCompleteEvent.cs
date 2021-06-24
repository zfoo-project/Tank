using Spring.Event;

namespace Summer.Resource.Model.Eve
{
    public class ResourceCheckCompleteEvent : IEvent
    {
        public int movedCount;
        public int removedCount;
        public int updateCount;
        public long updateTotalLength;
        public long updateTotalZipLength;

        public static ResourceCheckCompleteEvent ValueOf(int movedCount, int removedCount, int updateCount, long updateTotalLength, long updateTotalZipLength)
        {
            var eve = new ResourceCheckCompleteEvent();
            eve.movedCount = movedCount;
            eve.removedCount = removedCount;
            eve.updateCount = updateCount;
            eve.updateTotalLength = updateTotalLength;
            eve.updateTotalZipLength = updateTotalZipLength;
            return eve;
        }
    }
}