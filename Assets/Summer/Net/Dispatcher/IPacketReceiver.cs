using CsProtocol.Buffer;

namespace Summer.Net.Dispatcher
{
    public interface IPacketReceiver
    {
        void Invoke(IPacket packet);
    }
}