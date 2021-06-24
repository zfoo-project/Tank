using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Spring.Logger;
using Spring.Util;

namespace Summer.Net.Core.Tcp
{
    public class NetTcpClient : AbstractClient
    {
        public string hostAddress;
        public int port;

        private NetworkStream stream;
        private TcpClient client;

        public bool Connecting => connecting;


        public NetTcpClient(string hostAddress, int port)
        {
            this.hostAddress = hostAddress;
            this.port = port;
        }

        public override void Start()
        {
            // not if already started
            if (Connecting || Connected())
            {
                Log.Warning("Telepathy Client can not create connection because an existing connection is connecting or connected");
                return;
            }

            // We are connecting from now until Connect succeeds or fails
            connecting = true;

            // create a TcpClient with perfect IPv4, IPv6 and hostname resolving
            // support.
            //
            // * TcpClient(hostname, port): works but would connect (and block)
            //   already
            // * TcpClient(AddressFamily.InterNetworkV6): takes Ipv4 and IPv6
            //   addresses but only connects to IPv6 servers (e.g. Telepathy).
            //   does NOT connect to IPv4 servers (e.g. Mirror Booster), even
            //   with DualMode enabled.
            // * TcpClient(): creates IPv4 socket internally, which would force
            //   Connect() to only use IPv4 sockets.
            //
            // => the trick is to clear the internal IPv4 socket so that Connect
            //    resolves the hostname and creates either an IPv4 or an IPv6
            //    socket as needed (see TcpClient source)
            // creates IPv4 socket
            client = new TcpClient();
            // clear internal IPv4 socket until Connect()
            client.Client = null;
            // connect (blocking)
            client.Connect(hostAddress, port);
            stream = client.GetStream();
            connecting = false;

            // set socket options after the socket was created in Connect()
            // (not after the constructor because we clear the socket there)
            client.NoDelay = NoDelay;
            client.SendTimeout = SendTimeout;

            // clear old messages in queue, just to be sure that the caller
            // doesn't receive data from last time and gets out of sync.
            // -> calling this in Disconnect isn't smart because the caller may
            //    still want to process all the latest messages afterwards
            sendQueue.Clear();

            // client.Connect(ip, port) is blocking. let's call it in the thread
            // and return immediately.
            // -> this way the application doesn't hang for 30s if connect takes
            //    too long, which is especially good in games
            // -> this way we don't async client.BeginConnect, which seems to
            //    fail sometimes if we connect too many clients too fast
            receiveThread = new Thread(() => { ReceiveLoop(); });
            receiveThread.IsBackground = true;
            receiveThread.Start();

            // start send thread only after connected
            sendThread = new Thread(() => { SendLoop(); });
            sendThread.IsBackground = true;
            sendThread.Start();
        }

        // TcpClient.Connected doesn't check if socket != null, which
        // results in NullReferenceExceptions if connection was closed.
        // -> let's check it manually instead
        public override bool Connected()
        {
            return client != null && client.Client != null && client.Client.Connected;
        }

        public override void Close()
        {
            // close client
            IOUtils.CloseIO(stream, client);

            // wait until thread finished. this is the only way to guarantee
            // that we can call Connect() again immediately after Disconnect
            // -> calling .Join would sometimes wait forever, e.g. when
            //    calling Disconnect while trying to connect to a dead end
            if (sendThread != null)
            {
                try
                {
                    sendThread.Interrupt();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            if (receiveThread != null)
            {
                try
                {
                    receiveThread.Interrupt();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            // we interrupted the receive Thread, so we can't guarantee that
            // connecting was reset. let's do it manually.
            connecting = false;

            // clear send queues. no need to hold on to them.
            // (unlike receiveQueue, which is still needed to process the
            //  latest Disconnected message, etc.)
            sendQueue.Clear();

            // let go of this one completely. the thread ended, no one uses
            // it anymore and this way Connected is false again immediately.
            stream = null;
            client = null;
        }

        public override string ToConnectUrl()
        {
            return StringUtils.Format("{}:{}", hostAddress, port);
        }

        protected override void SendMessagesBlocking(byte[] messages, int offset, int size)
        {
            stream.Write(messages, offset, size);
        }
        
        // helper function to read EXACTLY 'n' bytes
        // -> default .Read reads up to 'n' bytes. this function reads exactly 'n'
        //    bytes
        // -> this is blocking until 'n' bytes were received
        // -> immediately returns false in case of disconnects
        public bool ReadExactly(NetworkStream stream, byte[] buffer, int amount)
        {
            // there might not be enough bytes in the TCP buffer for .Read to read
            // the whole amount at once, so we need to keep trying until we have all
            // the bytes (blocking)
            //
            // note: this just is a faster version of reading one after another:
            //     for (int i = 0; i < amount; ++i)
            //         if (stream.Read(buffer, i, 1) == 0)
            //             return false;
            //     return true;
            int bytesRead = 0;
            while (bytesRead < amount)
            {
                // read up to 'remaining' bytes with the 'safe' read extension
                int remaining = amount - bytesRead;
                int result = ReadSafely(stream, buffer, bytesRead, remaining);

                // .Read returns 0 if disconnected
                if (result == 0)
                    return false;

                // otherwise add to bytes read
                bytesRead += result;
            }

            return true;
        }

        protected override bool ReadMessageBlocking(out byte[] content)
        {
            content = null;

            // create header buffer if not created yet
            if (header == null)
            {
                header = new byte[4];
            }

            // read exactly 4 bytes for header (blocking)
            if (!ReadExactly(stream, header, 4))
            {
                return false;
            }

            // convert to int
            int size = ConverterUtils.BytesToIntBigEndian(header);

            // protect against allocation attacks. an attacker might send
            // multiple fake '2GB header' packets in a row, causing the server
            // to allocate multiple 2GB byte arrays and run out of memory.
            //
            // also protect against size <= 0 which would cause issues
            if (size > 0 && size <= MaxMessageSize)
            {
                // read exactly 'size' bytes for content (blocking)
                content = new byte[size];
                return ReadExactly(stream, content, size);
            }

            Log.Warning("ReadMessageBlocking: possible header attack with a header of: " + size + " bytes.");
            return false;
        }
    }
}