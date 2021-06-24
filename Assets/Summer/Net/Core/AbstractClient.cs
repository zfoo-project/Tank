using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using CsProtocol;
using CsProtocol.Buffer;
using Spring.Collection;
using Spring.Logger;
using Spring.Util;

namespace Summer.Net.Core
{
    public abstract class AbstractClient
    {
        /////////////////////////////////////////////////////////
        // common code
        // incoming message queue of <connectionId, message>
        // (not a HashSet because one connection can have multiple new messages)
        protected ConcurrentQueue<Message> receiveQueue = new ConcurrentQueue<Message>();

        // send queue
        // => SafeQueue is twice as fast as ConcurrentQueue, see SafeQueue.cs!
        protected SafeQueue<byte[]> sendQueue = new SafeQueue<byte[]>();

        private int connectionId = 0;

        protected Thread receiveThread;
        protected Thread sendThread;

        // ManualResetEvent to wake up the send thread. better than Thread.Sleep
        // -> call Set() if everything was sent
        // -> call Reset() if there is something to send again
        // -> call WaitOne() to block until Reset was called
        protected ManualResetEvent sendPending = new ManualResetEvent(false);

        // queue count, useful for debugging / benchmarks
        public int ReceiveQueueCount => receiveQueue.Count;

        // warning if message queue gets too big
        // if the average message is about 20 bytes then:
        // -   1k messages are   20KB
        // -  10k messages are  200KB
        // - 100k messages are 1.95MB
        // 2MB are not that much, but it is a bad sign if the caller process
        // can't call GetNextMessage faster than the incoming messages.
        public static int messageQueueSizeWarning = 100000;

        // TcpClient has no 'connecting' state to check. We need to keep track of it manually.
        // -> checking 'thread.IsAlive && !Connected' is not enough because the
        //    thread is alive and connected is false for a short moment after
        //    disconnecting, so this would cause race conditions.
        // -> we use a threadsafe bool wrapper so that ThreadFunction can remain
        //    static (it needs a common lock)
        // => Connecting is true from first Connect() call in here, through the
        //    thread start, until TcpClient.Connect() returns. Simple and clear.
        // => bools are atomic according to
        //    https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/variables
        //    made volatile so the compiler does not reorder access to it
        protected volatile bool connecting;

        // removes and returns the oldest message from the message queue.
        // (might want to call this until it doesn't return anything anymore)
        // -> Connected, Data, Disconnected events are all added here
        // -> bool return makes while (GetMessage(out Message)) easier!
        // -> no 'is client connected' check because we still want to read the
        //    Disconnected message after a disconnect
        public bool GetNextMessage(out Message message)
        {
            return receiveQueue.TryDequeue(out message);
        }

        // NoDelay disables nagle algorithm. lowers CPU% and latency but
        // increases bandwidth
        public bool NoDelay = true;

        // Prevent allocation attacks. Each packet is prefixed with a length
        // header, so an attacker could send a fake packet with length=2GB,
        // causing the server to allocate 2GB and run out of memory quickly.
        // -> simply increase max packet size if you want to send around bigger
        //    files!
        // -> 16KB per message should be more than enough.
        public int MaxMessageSize = 16 * 1024;

        // Send would stall forever if the network is cut off during a send, so
        // we need a timeout (in milliseconds)
        public int SendTimeout = 5000;

        // avoid header[4] allocations but don't use one buffer for all threads
        [ThreadStatic]
        protected static byte[] header;

        // avoid payload[packetSize] allocations but don't use one buffer for
        // all threads
        [ThreadStatic]
        protected static byte[] payload;

        public abstract void Start();

        public abstract bool Connected();

        public abstract void Close();

        public abstract string ToConnectUrl();

        protected abstract void SendMessagesBlocking(byte[] messages, int offset, int size);

        protected abstract bool ReadMessageBlocking(out byte[] content);

        // .Read returns '0' if remote closed the connection but throws an
        // IOException if we voluntarily closed our own connection.
        //
        // let's add a ReadSafely method that returns '0' in both cases so we don't
        // have to worry about exceptions, since a disconnect is a disconnect...
        public int ReadSafely(Stream stream, byte[] buffer, int offset, int size)
        {
            try
            {
                return stream.Read(buffer, offset, size);
            }
            catch (IOException)
            {
                return 0;
            }
        }


        /////////////////////////////////////////////
        // static helper functions
        // send message (via stream) with the <size,content> message structure
        // this function is blocking sometimes!
        // (e.g. if someone has high latency or wire was cut off)
        private bool SendMessagesBlocking(byte[][] messages)
        {
            // stream.Write throws exceptions if client sends with high
            // frequency and the server stops
            try
            {
                // we might have multiple pending messages. merge into one
                // packet to avoid TCP overheads and improve performance.
                int packetSize = 0;
                for (int i = 0; i < messages.Length; ++i)
                {
                    // header + content
                    packetSize += messages[i].Length;
                }

                // create payload buffer if not created yet or previous one is
                // too small
                // IMPORTANT: payload.Length might be > packetSize! don't use it!
                if (payload == null || payload.Length < packetSize)
                {
                    payload = new byte[packetSize];
                }

                // create the packet
                int position = 0;
                for (int i = 0; i < messages.Length; ++i)
                {
                    Array.Copy(messages[i], 0, payload, position, messages[i].Length);
                    position += messages[i].Length;
                }

                // write the whole thing
                SendMessagesBlocking(payload, 0, packetSize);

                return true;
            }
            catch (Exception exception)
            {
                // log as regular message because servers do shut down sometimes
                Log.Info("Send: stream.Write exception: " + exception);
                return false;
            }
        }

        public virtual bool Send(byte[] data)
        {
            if (Connected())
            {
                // respect max message size to avoid allocation attacks.
                if (data.Length <= MaxMessageSize)
                {
                    // add to send queue and return immediately.
                    // calling Send here would be blocking (sometimes for long times if other side lags or wire was disconnected)
                    sendQueue.Enqueue(data);
                    // interrupt SendThread WaitOne()
                    sendPending.Set();
                    return true;
                }

                Log.Error($"Client.Send: message too big: {data.Length}. Limit: {MaxMessageSize}");
                return false;
            }

            Log.Warning("Client.Send: not connected!");
            return false;
        }

        protected void ReceiveLoop()
        {
            // keep track of last message queue warning
            var messageQueueLastWarningTimestamp = TimeUtils.Now();

            // absolutely must wrap with try/catch, otherwise thread exceptions
            // are silent
            try
            {
                // add connected event to queue with ip address as data in case
                // it's needed
                receiveQueue.Enqueue(new Message(MessageType.Connected, null));

                // let's talk about reading data.
                // -> normally we would read as much as possible and then
                //    extract as many <size,content>,<size,content> messages
                //    as we received this time. this is really complicated
                //    and expensive to do though
                // -> instead we use a trick:
                //      Read(2) -> size
                //        Read(size) -> content
                //      repeat
                //    Read is blocking, but it doesn't matter since the
                //    best thing to do until the full message arrives,
                //    is to wait.
                // => this is the most elegant AND fast solution.
                //    + no resizing
                //    + no extra allocations, just one for the content
                //    + no crazy extraction logic
                while (true)
                {
                    // read the next message (blocking) or stop if stream closed
                    byte[] content;
                    if (!ReadMessageBlocking(out content))
                    {
                        // break instead of return so stream close still happens!
                        break;
                    }

                    ByteBuffer byteBuffer = null;
                    try
                    {
                        byteBuffer = ByteBuffer.ValueOf();
                        byteBuffer.WriteBytes(content);
                        var packet = ProtocolManager.Read(byteBuffer);
                        // queue it
                        receiveQueue.Enqueue(new Message(MessageType.Data, packet));
                    }
                    finally
                    {
                        if (byteBuffer != null)
                        {
                            byteBuffer.Clear();
                        }
                    }

                    // and show a warning if the queue gets too big
                    // -> we don't want to show a warning every single time,
                    //    because then a lot of processing power gets wasted on
                    //    logging, which will make the queue pile up even more.
                    // -> instead we show it every 10s, so that the system can
                    //    use most it's processing power to hopefully process it.
                    if (receiveQueue.Count > messageQueueSizeWarning)
                    {
                        var elapsed = TimeUtils.Now() - messageQueueLastWarningTimestamp;
                        if (elapsed > 10 * TimeUtils.MILLIS_PER_SECOND)
                        {
                            Log.Warning("ReceiveLoop: messageQueue is getting big(" + receiveQueue.Count + "), try calling GetNextMessage more often. You can call it more than once per frame!");
                            messageQueueLastWarningTimestamp = TimeUtils.Now();
                        }
                    }
                }
            }
            catch (SocketException exception)
            {
                // this happens if (for example) the ip address is correct
                // but there is no server running on that ip/port
                Log.Error("Client Recv: failed to connect to {} reason={}", ToConnectUrl(), exception.ToString());

                // add 'Disconnected' event to message queue so that the caller
                // knows that the Connect failed. otherwise they will never know
                receiveQueue.Enqueue(new Message(MessageType.Disconnected, null));
            }
            catch (ThreadInterruptedException)
            {
                // expected if Disconnect() aborts it
            }
            catch (ThreadAbortException)
            {
                // expected if Disconnect() aborts it
            }
            catch (Exception exception)
            {
                // something went wrong. the thread was interrupted or the
                // connection closed or we closed our own connection or ...
                // -> either way we should stop gracefully
                Log.Info("ReceiveLoop: finished receive function for connectionId=" + connectionId + " reason: " + exception);
                // something went wrong. probably important.
                Log.Error($"Client Recv Exception: {exception}");
            }
            finally
            {
                // Connect might have failed. thread might have been closed.
                // let's reset connecting state no matter what.
                connecting = false;

                // clean up no matter what
                // if we got here then we are done. ReceiveLoop cleans up already,
                // but we may never get there if connect fails. so let's clean up here too.
                Close();

                // add 'Disconnected' message after disconnecting properly.
                // -> always AFTER closing the streams to avoid a race condition
                //    where Disconnected -> Reconnect wouldn't work because
                //    Connected is still true for a short moment before the stream
                //    would be closed.
                receiveQueue.Enqueue(new Message(MessageType.Disconnected, null));
            }
        }

        // thread send function
        // note: we really do need one per connection, so that if one connection
        //       blocks, the rest will still continue to get sends
        protected void SendLoop()
        {
            try
            {
                // try this. client will get closed eventually.
                while (Connected())
                {
                    // reset ManualResetEvent before we do anything else. this
                    // way there is no race condition. if Send() is called again
                    // while in here then it will be properly detected next time
                    // -> otherwise Send might be called right after dequeue but
                    //    before .Reset, which would completely ignore it until
                    //    the next Send call.
                    // WaitOne() blocks until .Set() again
                    sendPending.Reset();

                    // dequeue all
                    // SafeQueue.TryDequeueAll is twice as fast as
                    // ConcurrentQueue, see SafeQueue.cs!
                    byte[][] messages;
                    if (sendQueue.TryDequeueAll(out messages))
                    {
                        // send message (blocking) or stop if stream is closed
                        if (!SendMessagesBlocking(messages))
                        {
                            // break instead of return so stream close still happens!
                            break;
                        }
                    }

                    // don't choke up the CPU: wait until queue not empty anymore
                    sendPending.WaitOne();
                }
            }
            catch (ThreadAbortException)
            {
                // happens on stop. don't log anything.
            }
            catch (ThreadInterruptedException)
            {
                // happens if receive thread interrupts send thread.
            }
            catch (Exception exception)
            {
                // something went wrong. the thread was interrupted or the
                // connection closed or we closed our own connection or ...
                // -> either way we should stop gracefully
                Log.Info("SendLoop Exception: connectionId=" + connectionId + " reason: " + exception);
            }
            finally
            {
                // clean up no matter what
                // we might get SocketExceptions when sending if the 'host has
                // failed to respond' - in which case we should close the connection
                // which causes the ReceiveLoop to end and fire the Disconnected
                // message. otherwise the connection would stay alive forever even
                // though we can't send anymore.
                Close();
            }
        }
    }
}