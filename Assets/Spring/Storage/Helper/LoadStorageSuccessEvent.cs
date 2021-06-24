using System;
using Spring.Event;

namespace Spring.Storage.Helper
{
    public class LoadStorageSuccessEvent : IEvent
    {
        public Type resourceType;

        public static LoadStorageSuccessEvent ValueOf(Type resourceType)
        {
            var eve = new LoadStorageSuccessEvent();
            eve.resourceType = resourceType;
            return eve;
        }
    }
}