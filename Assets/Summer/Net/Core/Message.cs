// incoming message queue of <connectionId, message>
// (not a HashSet because one connection can have multiple new messages)
// -> a struct to minimize GC

using System.Runtime.InteropServices;
using CsProtocol.Buffer;

namespace Summer.Net.Core
{
    [StructLayout(LayoutKind.Auto)]
    public struct Message
    {
        public MessageType messageType;
        public IPacket packet;

        public Message(MessageType messageType, IPacket packet)
        {
            this.messageType = messageType;
            this.packet = packet;
        }
    }
}