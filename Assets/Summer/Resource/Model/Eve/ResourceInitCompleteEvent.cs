using Spring.Event;

namespace Summer.Resource.Model.Eve
{
    public class ResourceInitCompleteEvent : IEvent
    {
        public static ResourceInitCompleteEvent ValueOf()
        {
            return new ResourceInitCompleteEvent();
        }
    }
}