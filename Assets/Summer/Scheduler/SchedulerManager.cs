using Spring.Core;
using Spring.Event;
using Spring.Logger;
using Spring.Util;
using Summer.Base;
using Summer.Base.Model;
using Summer.Scheduler.Model;

namespace Summer.Scheduler
{
    [Bean]
    public class SchedulerManager : AbstractManager, ISchedulerManager
    {
        [Autowired]
        private BaseComponent baseComponent;

        private byte count;

        private long minuteSchedulerTimestamp = TimeUtils.Now();

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (++count < baseComponent.frameRate)
            {
                return;
            }

            // 每60帧更新一下当前的服务器时间戳
            var now = TimeUtils.Now() + TimeUtils.MILLIS_PER_SECOND;
            TimeUtils.SetNow(now);
            count = 0;

            // 每一分钟抛出一个MinuteSchedulerAsyncEvent事件
            if (now - minuteSchedulerTimestamp > TimeUtils.MILLIS_PER_MINUTE)
            {
                minuteSchedulerTimestamp = now;
                EventBus.AsyncSubmit(MinuteSchedulerAsyncEvent.ValueOf());
            }
        }

        public override void Shutdown()
        {
        }

    }
}