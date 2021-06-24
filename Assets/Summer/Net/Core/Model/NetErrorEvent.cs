using Spring.Event;

namespace Summer.Net.Core.Model
{
    public class NetErrorEvent : IEvent
    {
        public static NetErrorEvent ValueOf()
        {
            var eve = new NetErrorEvent();
            return eve;
        }
    }
}