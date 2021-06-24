using Spring.Event;

namespace Summer.Scheduler.Model
{
    /**
     * 异步事件
     */
    public class MinuteSchedulerAsyncEvent : IEvent
    {
        public static MinuteSchedulerAsyncEvent ValueOf()
        {
            return new MinuteSchedulerAsyncEvent();
        }
    }
}