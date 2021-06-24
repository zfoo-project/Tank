using Spring.Event;

namespace Summer.Net.Core.Model
{
    public class NetOpenEvent : IEvent
    {
        public static NetOpenEvent ValueOf()
        {
            return new NetOpenEvent();
        }
    }
}