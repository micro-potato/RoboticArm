using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UDPDLL
{
    public class AsyncUDP
    {
        private IPEndPoint ipEndPoint = null;
        private UdpClient udpReceive = null;
        UdpState udpReceiveState = null;

        public delegate void DelDataIn(string ip, string data);
        public event DelDataIn OnDataIn;

        /// <summary>
        /// UDPServer初始化
        /// </summary>
        /// <param name="listenPort">监听端口</param>
        /// <returns></returns>
        public bool InitUDPServer(int serverPort)
        {
            try
            {
                //UDP初始化
                ipEndPoint = new IPEndPoint(IPAddress.Any, serverPort);
                udpReceive = new UdpClient(ipEndPoint);
                udpReceiveState = new UdpState();
                udpReceiveState.udpClient = udpReceive;
                udpReceiveState.ipEndPoint = ipEndPoint;

                //异步回调
                udpReceive.BeginReceive(new AsyncCallback(ReceiveCallback), udpReceiveState);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 接收回调函数
        private void ReceiveCallback(IAsyncResult iar)
        {
            try
            {
                UdpState udpReceiveState = iar.AsyncState as UdpState;
                if (iar.IsCompleted)
                {
                    Byte[] receiveBytes = udpReceiveState.udpClient.EndReceive(iar, ref udpReceiveState.ipEndPoint);
                    string receiveString = Encoding.GetEncoding("UTF-8").GetString(receiveBytes);
                    if (OnDataIn != null)
                    {
                        OnDataIn(udpReceiveState.ipEndPoint.Address.ToString(), receiveString);
                    }
                }
            }
            catch
            {

            }
            finally
            {
                udpReceive.BeginReceive(new AsyncCallback(ReceiveCallback), udpReceiveState);
            }
        }

        /// <summary>
        /// UDP发送
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">端口</param>
        /// <param name="data">发送内容</param>
        public void Send(string ip, int port, string data)
        {
            try
            {
                IPEndPoint sendEP = new IPEndPoint(IPAddress.Parse(ip), port);
                UdpClient udpSend = new UdpClient();

                UdpState udpSendState = new UdpState();
                udpSendState.ipEndPoint = sendEP;
                udpSend.Connect(udpSendState.ipEndPoint);
                udpSendState.udpClient = udpSend;

                Byte[] sendBytes = Encoding.GetEncoding("UTF-8").GetBytes(data);
                udpSend.BeginSend(sendBytes, sendBytes.Length, new AsyncCallback(SendCallback), udpSendState);
            }
            catch
            {

            }
        }

        // 发送回调函数
        private void SendCallback(IAsyncResult iar)
        {
            UdpState udpState = iar.AsyncState as UdpState;
        }
    }
}