using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ClientDLL
{
    internal class StateObject
    {
        public Socket workSocket;
        public const int BufferSize = 307200;
        public byte[] buffer = new byte[BufferSize];
    }
}
