using Spring.Event;

namespace Summer.Net.Core.Model
{
    public class NetCloseEvent : IEvent
    {
        public static NetCloseEvent ValueOf()
        {
            var eve = new NetCloseEvent();
            return eve;
        }
    }
}