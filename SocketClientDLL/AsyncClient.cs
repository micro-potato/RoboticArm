using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ClientDLL
{
    /// <summary>
    /// 客户端通讯类(TCP)
    /// </summary>
    public class AsyncClient
    {
        //事件
        public delegate void Connected();
        public event Connected onConnected;

        public delegate void DisConnect();
        public event DisConnect onDisConnect;

        public delegate void DataByteIn(byte[] SocketData);
        public event DataByteIn onDataByteIn;

        Socket socket;
        public void Connect(string ip, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint hostEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.BeginConnect(hostEndPoint, new AsyncCallback(ConnectCallback), socket);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                StateObject obj_SocketState = new StateObject();
                obj_SocketState.workSocket = client;
                client.BeginReceive(obj_SocketState.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), obj_SocketState);

                if (onConnected != null)
                {
                    onConnected();
                }
            }
            catch
            {
                if (onDisConnect != null)
                {
                    onDisConnect();
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject obj_SocketState = (StateObject)ar.AsyncState;
                Socket obj_Socket = obj_SocketState.workSocket;
                int BytesRead = obj_Socket.EndReceive(ar);
                byte[] tmp = new byte[BytesRead];
                Array.ConstrainedCopy(obj_SocketState.buffer, 0, tmp, 0, BytesRead);
                if (onDataByteIn != null)
                {
                    onDataByteIn(tmp);
                }
                obj_Socket.BeginReceive(obj_SocketState.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), obj_SocketState);
            }
            catch
            {
                if (onDisConnect != null)
                {
                    onDisConnect();
                }
            }

        }

        /// <summary>
        /// 发送信息到服务器
        /// </summary>
        /// <param name="data"></param>
        public void Send(string data)
        {
            try
            {
                byte[] sendByte = Encoding.UTF8.GetBytes(data+"\n");
                //byte[] sendByte = Encoding.GetEncoding("utf-8").GetBytes(data);
                socket.BeginSend(sendByte, 0, sendByte.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            }
            catch
            {

            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket obj_Socket = (Socket)ar.AsyncState;
                int bytesSent = obj_Socket.EndSend(ar);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 释放Socket通讯
        /// </summary>
        public void Dispose()
        {
            try
            {
                socket.Close();
                //socket.Dispose();
            }
            catch
            {

            }
        }
    }
}
