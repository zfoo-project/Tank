namespace Spring.Event
{
    public interface IEventReceiver
    {
        void Invoke(IEvent eve);
    }
}